﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        if (_systemHandler.ChatGptMessenger?.ChatHolder.Children.Count > 0)
        {
            // Navigate the user to the first chat in the chatholder
            _systemHandler.ChatGptMessenger?.ChatFrame.NavigationService.Navigate(new Chat(_systemHandler.Chats.First()));
        }
        else
        {
            // Create a new chat
            var chatClass = new ChatClass()
            {
                Title = $"Chat-{_systemHandler.ChatGptMessenger?.ChatHolder.Children.Count + 1}",
                ChatIdentifier = Guid.NewGuid(),
                ChatHistory = new()
            };
            _systemHandler.Chats.Add(chatClass);
            _systemHandler.ChatGptMessenger?.ChatHolder.Children.Add(new ChatControl(chatClass));
            _systemHandler.ChatGptMessenger?.ChatFrame.NavigationService.Navigate(new Chat(_systemHandler.Chats.First()));
        }
    }
}