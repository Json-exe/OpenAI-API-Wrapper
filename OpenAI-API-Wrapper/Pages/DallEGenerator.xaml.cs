using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NLog;
using OpenAI_API_Wrapper.Classes;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using ToastNotifications.Messages;

namespace OpenAI_API_Wrapper.Pages;

public partial class DallEGenerator : Page
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public DallEGenerator()
    {
        InitializeComponent();
        ImageSize.SelectedItem = ImageSize.Items[0];
        _systemHandler.DallEGen = this;
    }
    
    private void DisableControls()
    {
        Prompt.IsEnabled = false;
        ImageCount.IsEnabled = false;
        ImageSize.IsEnabled = false;
        GenerateButton.IsEnabled = false;
        ImagesLoading.Visibility = Visibility.Visible;
        ResultHolder.Children.Clear();
    }
    
    private void EnableControls()
    {
        Prompt.IsEnabled = true;
        ImageCount.IsEnabled = true;
        ImageSize.IsEnabled = true;
        GenerateButton.IsEnabled = true;
        ImagesLoading.Visibility = Visibility.Collapsed;
    }

    private async void GenerateImage_OnClick(object sender, RoutedEventArgs e)
    {
        if (_systemHandler.OpenAiService == null)
        {
            _systemHandler.Notifier.ShowWarning("OpenAI API Key is not set! Please set it in the settings page!");
            return;
        }
        // Check if something is inside the prompt text box
        if (string.IsNullOrEmpty(Prompt.Text))
        {
            MessageBox.Show("Please enter a prompt!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        DisableControls();
        var imageResult = await _systemHandler.OpenAiService!.Image.CreateImage(new ImageCreateRequest
        {
            Prompt = Prompt.Text,
            N = int.TryParse(ImageCount.Text, out var count) ? count : 1,
            Size = ImageSize.Text,
            ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
            User = "OpenAI-API-Wrapper"
        });
        if (imageResult.Successful)
        {
            // Create a new Image foreach result and add it to the wrap panel
            foreach (var image in imageResult.Results)
            {
                var imageControl = new Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Width = 250,
                    Height = 250,
                    Margin = new Thickness(5)
                };
                // Each image needs a context menu to copy the image url
                var contextMenu = new ContextMenu();
                var copyUrl = new MenuItem
                {
                    Header = "Copy URL"
                };
                copyUrl.Click += (o, args) =>
                {
                    Clipboard.SetText(image.Url);
                };
                contextMenu.Items.Add(copyUrl);
                imageControl.ContextMenu = contextMenu;
                ResultHolder.Children.Add(imageControl);
            }
        }
        else
        {
            if (imageResult.Error?.Code == "invalid_api_key")
            {
                _systemHandler.Notifier.ShowError("Invalid API Key! Please check your API Key in the settings page!");
                _systemHandler.OpenAiService = null;
                Log.Warn("Invalid API Key");
            }
            else
            {
                MessageBox.Show(imageResult.Error?.Message);
                Log.Error("Error while generating image: {0}", imageResult.Error?.Message);
            }
        }
        EnableControls();
    }
}