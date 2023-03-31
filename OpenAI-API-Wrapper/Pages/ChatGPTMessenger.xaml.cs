using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NLog;
using OpenAI_API_Wrapper.Classes;
using OpenAI_API_Wrapper.Controls;

namespace OpenAI_API_Wrapper.Pages;

public partial class ChatGptMessanger : Page
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public ChatGptMessanger()
    {
        InitializeComponent();
        _systemHandler.ChatGptMessenger = this;

        var connection = _systemHandler.GetDatabaseConnection();
        var command = new SQLiteCommand("SELECT * FROM ChatClass", connection);
        var reader = command.ExecuteReader();
        if (!reader.HasRows)
        {
            Log.Info("No data found in the database! Creating new chat...");
            var chatClass = new ChatClass()
            {
                Title = $"Chat-{ChatHolder.Children.Count + 1}",
                ChatIdentifier = Guid.NewGuid(),
                ChatHistory = new List<string>()
            };
            _systemHandler.Chats.Add(chatClass);
            ChatHolder.Children.Add(new ChatControl(chatClass));
        }
        else
        {
            Log.Info("Data found in the database! Loading chats...");
            while (reader.Read())
            {
                var chatClass = new ChatClass()
                {
                    Title = $"Chat-{ChatHolder.Children.Count + 1}",
                    ChatIdentifier = new Guid(reader.GetString("ChatID")),
                    ChatHistory = reader.GetString("ChatHistory").Split(';').ToList()
                };
                _systemHandler.Chats.Add(chatClass);
                ChatHolder.Children.Add(new ChatControl(chatClass));
            }
        }
        connection.Close();
        foreach (ChatControl chat in ChatHolder.Children)
        {
            ChatFrame.NavigationService.Navigate(new Chat(_systemHandler.Chats.First(x => x.ChatIdentifier == chat.ChatId)));
            break;
        }
    }

    private void NewChat_OnClick(object sender, RoutedEventArgs e)
    {
        var guid = Guid.NewGuid();
        while (_systemHandler.Chats.Any(x => x.ChatIdentifier == guid))
        {
            guid = Guid.NewGuid();
        }
        
        var chatClass = new ChatClass()
        {
            Title = $"Chat-{ChatHolder.Children.Count + 1}",
            ChatIdentifier = guid,
            ChatHistory = new List<string>()
        };
        _systemHandler.Chats.Add(chatClass);
        ChatHolder.Children.Add(new ChatControl(chatClass));
        ChatFrame.NavigationService.Navigate(new Chat(_systemHandler.Chats.First(x => x.ChatIdentifier == chatClass.ChatIdentifier)));
    }
}