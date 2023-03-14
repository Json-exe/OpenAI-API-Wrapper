using System.Windows;
using System.Windows.Controls;
using OpenAI_API_Wrapper.Classes;
using ToastNotifications.Messages;

namespace OpenAI_API_Wrapper.Pages;

public partial class Settings : Page
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    
    
    public Settings()
    {
        InitializeComponent();
        ApiToken.Text = Properties.Settings.Default.APIKEY;
        ChatGptTokens.Text = Properties.Settings.Default.ChatGPTTokens.ToString();
    }

    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        if(string.IsNullOrEmpty(ApiToken.Text))
        {
            _systemHandler.Notifier.ShowError("API Key cannot be empty!");
            return;
        }

        if (ApiToken.Text.Length <= 50)
        {
            _systemHandler.Notifier.ShowError("Please enter a valid API Key!");
            return;
        }

        if (string.IsNullOrEmpty(ChatGptTokens.Text) || !int.TryParse(ChatGptTokens.Text, out var _) || int.Parse(ChatGptTokens.Text) <= 0)
        {
            _systemHandler.Notifier.ShowError("ChatGPT Tokens cannot be empty, must be a number and must be greater than 0!");
            return;
        }
        Properties.Settings.Default.APIKEY = ApiToken.Text;
        Properties.Settings.Default.ChatGPTTokens = int.Parse(ChatGptTokens.Text);
        Properties.Settings.Default.Save();
        _systemHandler.Notifier.ShowSuccess("API Key saved!");
        _systemHandler.InitOpenApiService();
    }
}