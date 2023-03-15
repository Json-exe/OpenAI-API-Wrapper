using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using OpenAI_API_Wrapper.Pages;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace OpenAI_API_Wrapper.Classes;

public sealed class SystemHandler
{
    private SystemHandler() {}  
    private static readonly object Lock = new();  
    private static SystemHandler? _instance;  
    public static SystemHandler Instance {  
        get {
            if (_instance != null) return _instance;
            lock(Lock)
            {
                _instance ??= new SystemHandler();
            }
            return _instance;  
        }  
    }
    
    public OpenAIService? OpenAiService { get; set; }
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    public ChatGptMessanger? ChatGptMessenger;
    public DallEGenerator? DallEGen;
    public Whisper? Whisper;
    public List<ChatClass> Chats = new();

    public readonly Notifier Notifier = new(cfg =>
    {
        cfg.PositionProvider = new WindowPositionProvider(
            parentWindow: Application.Current.MainWindow,
            corner: Corner.TopRight,
            offsetX: 10,  
            offsetY: 10);

        cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
            notificationLifetime: TimeSpan.FromSeconds(5),
            maximumNotificationCount: MaximumNotificationCount.FromCount(5));

        cfg.Dispatcher = Application.Current.Dispatcher;
    });

    public SQLiteConnection GetDatabaseConnection()
    {
        var connection = new SQLiteConnection(@$"Data Source={Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\JDS\OpenAI-API-Wrapper\data\chatdata.db");
        connection.Open();
        return connection;
    }

    public void InitOpenApiService()
    {
        if (OpenAiService != null) return;
        if (string.IsNullOrEmpty(Properties.Settings.Default.APIKEY))
        {
            Notifier.ShowError("No API Key found. Please enter your API Key in the settings.");
            Log.Error("No API Key found. Cant initialize OpenAI API Service!");
            return;
        }

        try
        {
            OpenAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = Properties.Settings.Default.APIKEY
            });
            Log.Info("OpenAI API Service initialized");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while initializing OpenAI API Service!");
            Notifier.ShowError("Error while initializing OpenAI API Service! Please check your API Key!");
        }
    }

    public async Task SaveChats()
    {
        if (ChatGptMessenger == null)
        {
            return;
        }
        var connection = GetDatabaseConnection();
        var command = new SQLiteCommand("DELETE FROM ChatClass", connection);
        await command.ExecuteNonQueryAsync();
        foreach (var chat in Chats.Where(chat => string.Join(';', chat.ChatHistory).Length <= 20000))
        {
            command = new SQLiteCommand("INSERT INTO ChatClass (ChatHistory, ChatID) VALUES (@ChatHistory, @ChatID)", connection);
            command.Parameters.AddWithValue("@ChatID", chat.ChatIdentifier.ToString());
            command.Parameters.AddWithValue("@ChatHistory", string.Join(";", chat.ChatHistory));
            await command.ExecuteNonQueryAsync();
        }
        var notSavedChats = Chats.Count(chat => string.Join(';', chat.ChatHistory).Length > 20000);
        if (notSavedChats > 0)
        {
            MessageBox.Show("Chats saved! " + notSavedChats + " chats were not saved because they were too long!", "Chats too long", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        await connection.CloseAsync();
        Log.Info("Chats saved!");
    }

    public async Task CreateDatabase()
    {
        Log.Info("Creating database...");
        // Check if the directory exists
        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\JDS\\OpenAI-API-Wrapper\\data"))
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\JDS\\OpenAI-API-Wrapper\\data");
        }
        // Create the database
        SQLiteConnection.CreateFile(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\JDS\\OpenAI-API-Wrapper\\data\\chatdata.db");
        var connection = GetDatabaseConnection();
        var command = new SQLiteCommand("CREATE TABLE `ChatClass` (`id` integer not null primary key autoincrement, `ChatHistory` VARCHAR(20000) null, `ChatID` varchar(100) NOT NULL)", connection);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }
}