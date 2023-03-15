using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using OpenAI_API_Wrapper.Classes;
using OpenAI_API_Wrapper.Pages;

namespace OpenAI_API_Wrapper.Controls;

public partial class ChatControl : UserControl
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    public Guid ChatId { get; }

    public ChatControl(ChatClass chatClass)
    {
        InitializeComponent();
        ChatTitle.Text = chatClass.Title;
        ChatId = chatClass.ChatIdentifier;
    }

    private void ChatCard_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _systemHandler.ChatGptMessenger?.ChatFrame.NavigationService.Navigate(new Chat(_systemHandler.Chats.First(x => x.ChatIdentifier == ChatId)));
    }

    private void DeleteChat_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Are you sure you want to delete this chat?", "Delete chat", MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        // Delete the chat from the systemhandler
        _systemHandler.Chats.Remove(_systemHandler.Chats.First(x => x.ChatIdentifier == ChatId));
        // Delete the chat from the chatholder
        _systemHandler.ChatGptMessenger?.ChatHolder.Children.Remove(this);
        // Navigate the user to the first chat in the chatholder
        _systemHandler.ChatGptMessenger?.ChatFrame.NavigationService.Navigate(new Chat(_systemHandler.Chats.First()));
    }
}