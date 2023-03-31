using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NLog;
using OpenAI_API_Wrapper.Classes;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using ToastNotifications.Messages;

namespace OpenAI_API_Wrapper.Pages;

public partial class Chat : Page
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly ChatClass _chatClass;
    private bool _chatBlocked;
    
    public Chat(ChatClass chatClass)
    {
        InitializeComponent();
        _chatClass = chatClass;
        foreach (var chatMessage in _chatClass.ChatHistory)
        {
            ChatBox.Children.Add(new TextBox()
            {
                Text = chatMessage,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                FontSize = 15,
                IsHitTestVisible = true
            });
            ChatBox.Children.Add(new Separator());
        }
    }

    private void DisableControls()
    {
        SendMessage.IsEnabled = false;
        ChatInput.IsEnabled = false;
    }
    
    private void EnableControls()
    {
        if (_chatBlocked) return;
        SendMessage.IsEnabled = true;
        ChatInput.IsEnabled = true;
    }

    private async Task AskAi()
    {
        var messages = new List<ChatMessage>();
        // Foreach message in the chat history, add it to the chat message. But the first message is the Users message and the second message is the AI's message and so on
        for (var i = 0; i < _chatClass.ChatHistory.Count; i++)
        {
            messages.Add(new ChatMessage(i % 2 == 0 ? "user" : "assistant", _chatClass.ChatHistory[i]));
        }
        Log.Debug("Message history built. Asking the AI");
        // Create a new ChatRequest
        var completionResult = await _systemHandler.OpenAiService!.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Messages = messages,
            Model = Properties.Settings.Default.ChatGPTModel,
            MaxTokens = Properties.Settings.Default.ChatGPTTokens
        });
        if (completionResult.Successful)
        {
            var tokens = completionResult.Usage.TotalTokens;
            switch (tokens)
            {
                case >= 3500 and < 4092:
                    _systemHandler.Notifier.ShowWarning($"The chat history is reaching token limit! Chats above this limit cannot be continued! Remaining tokens: {4092 - tokens}");
                    break;
                case > 4092:
                    _systemHandler.Notifier.ShowError("The chat history has reached the token limit! Please create a new chat!");
                    _chatBlocked = true;
                    return;
            }
            _chatClass.ChatHistory.Add(completionResult.Choices.First().Message.Content);
            Log.Debug("AI answered successfully. Response: {0}", completionResult.Choices.First().Message.Content);
        }
        else
        {
            if (completionResult.Error?.Code == "invalid_api_key")
            {
                _systemHandler.Notifier.ShowError("Invalid API Key! Please check your API Key in the settings page!");
                _systemHandler.OpenAiService = null;
                Log.Warn("Invalid API Key");
            }
            else
            {
                Log.Error("Error while asking the AI: {0}", completionResult.Error);
                MessageBox.Show("Error while asking the AI: " + completionResult.Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _chatClass.ChatHistory.RemoveAt(_chatClass.ChatHistory.Count - 1);
        }
    }
    
    private async void SendMessage_OnClick(object sender, RoutedEventArgs e)
    {
        if (_systemHandler.OpenAiService == null)
        {
            _systemHandler.Notifier.ShowWarning("OpenAI API Key is not set! Please set it in the settings page!");
            return;
        }
        DisableControls();
        if (string.Join(';', _chatClass.ChatHistory).Length > 15000)
        {
            var availableSavingSpace = 20000 - string.Join(';', _chatClass.ChatHistory).Length;
            _systemHandler.Notifier.ShowWarning($"The chat history is reaching saving limit! Chats above this limit cannot be saved! Remaining space: {availableSavingSpace} characters");
        }
        Log.Info("Sending message. Prompt: {0}", ChatInput.Text);
        // Get the message from the text box
        var message = ChatInput.Text;
        // If the message is empty, don't send it
        if (string.IsNullOrWhiteSpace(message)) return;
        // Add the message to the chat history
        _chatClass.ChatHistory.Add(message);
        ChatBox.Children.Add(new TextBox()
        {
            Text = message,
            Margin = new Thickness(0, 0, 0, 8),
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            IsReadOnly = true,
            FontSize = 15,
            IsHitTestVisible = true
        });
        // Clear the text box
        ChatInput.Clear();
        await AskAi();
        // Clear the ChatBox and add for each message in the chat history a new TextBlock
        ChatBox.Children.Clear();
        foreach (var chatMessage in _chatClass.ChatHistory)
        {
            ChatBox.Children.Add(new TextBox()
            {
                Text = chatMessage,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                IsReadOnly = true,
                FontSize = 15,
                IsHitTestVisible = true
            });
            ChatBox.Children.Add(new Separator());
        }
        EnableControls();
    }
}