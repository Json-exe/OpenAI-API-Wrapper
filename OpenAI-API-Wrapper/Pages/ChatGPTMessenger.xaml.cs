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

public partial class ChatGptMessanger : Page
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly List<string> _chatHistory = new();

    public ChatGptMessanger()
    {
        InitializeComponent();
        _systemHandler.ChatGptMessenger = this;
    }
    
    private void DisableControls()
    {
        SendMessage.IsEnabled = false;
        ChatInput.IsEnabled = false;
    }
    
    private void EnableControls()
    {
        SendMessage.IsEnabled = true;
        ChatInput.IsEnabled = true;
    }

    private async Task AskAi()
    {
        var messages = new List<ChatMessage>();
        // Foreach message in the chat history, add it to the chatmessage. But the first message is the Users message and the second message is the AI's message and so on
        for (var i = 0; i < _chatHistory.Count; i++)
        {
            messages.Add(new ChatMessage(i % 2 == 0 ? "user" : "assistant", _chatHistory[i]));
        }
        Log.Debug("Message history built. Asking the AI");
        // Create a new ChatRequest
        var completionResult = await _systemHandler.OpenAiService!.CreateCompletion(new ChatCompletionCreateRequest()
        {
            Messages = messages,
            Model = Models.ChatGpt3_5Turbo,
            MaxTokens = 500
        });
        if (completionResult.Successful)
        {
            // Add the AI's message to the chat history
            _chatHistory.Add(completionResult.Choices.First().Message.Content);
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
        Log.Info("Sending message. Prompt: {0}", ChatInput.Text);
        // Get the message from the text box
        var message = ChatInput.Text;
        // If the message is empty, don't send it
        if (string.IsNullOrWhiteSpace(message)) return;
        // Add the message to the chat history
        _chatHistory.Add(message);
        ChatBox.Children.Add(new TextBlock()
        {
            Text = message,
            Margin = new Thickness(0, 0, 0, 8),
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brushes.White,
            FontSize = 15
        });
        // Clear the text box
        ChatInput.Clear();
        await AskAi();
        // Clear the ChatBox and add for each message in the chat history a new TextBlock
        ChatBox.Children.Clear();
        foreach (var chatMessage in _chatHistory)
        {
            ChatBox.Children.Add(new TextBlock()
            {
                Text = chatMessage,
                Margin = new Thickness(0, 0, 0, 8),
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White,
                FontSize = 15
            });
            ChatBox.Children.Add(new Separator());
        }
        EnableControls();
    }
}