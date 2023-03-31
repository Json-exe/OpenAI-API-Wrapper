using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OpenAI_API_Wrapper.Classes;
using OpenAI.GPT3.ObjectModels;
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

        if (int.Parse(ChatGptTokens.Text) > 4092)
        {
            _systemHandler.Notifier.ShowError("The GPT-Model only supports a maximum of 4092 tokens!");
            return;
        }

        Properties.Settings.Default.APIKEY = ApiToken.Text;
        Properties.Settings.Default.ChatGPTTokens = int.Parse(ChatGptTokens.Text);
        Properties.Settings.Default.Save();
        _systemHandler.Notifier.ShowSuccess("Settings saved!");
        _systemHandler.InitOpenApiService();
    }

    private async void Settings_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_systemHandler.OpenAiService == null)
        {
            _systemHandler.Notifier.ShowInformation("The OpenAI API Service is not initialized yet. Models cannot be loaded yet!");
            return;
        }
        var modelList = await _systemHandler.OpenAiService.Models.ListModel();
        foreach (var model in modelList.Models.Where(model => model.Id.Contains("gpt-3.5-turbo")))
        {
            var item = new ComboBoxItem
            {
                Content = model.Id,
                Tag = model.Id,
                ToolTip = model.Owner
            };
            if (model.Id == Properties.Settings.Default.ChatGPTModel)
            {
                item.IsSelected = true;
            }
            ChatGptModels.Items.Add(item);
        }
    }
}