using System;
using System.Windows;
using NLog;
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
}