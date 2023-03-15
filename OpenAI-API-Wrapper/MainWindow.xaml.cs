using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using OpenAI_API_Wrapper.Classes;

namespace OpenAI_API_Wrapper
{
    public partial class MainWindow : Window
    {
        private SystemHandler _systemHandler = null!;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigateDallE_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the DallE page, don't navigate to it again
            if (MainFrame.Content is Pages.DallEGenerator) return;
            MainFrame.Navigate(_systemHandler.DallEGen ??= new Pages.DallEGenerator());
        }

        private void NavigateChatGPT_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the ChatGPT page, don't navigate to it again
            if (MainFrame.Content is Pages.ChatGptMessanger) return;
            MainFrame.Navigate(_systemHandler.ChatGptMessenger ??= new Pages.ChatGptMessanger());
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var systemHandler = SystemHandler.Instance;
            systemHandler.InitOpenApiService();
            _systemHandler = systemHandler;
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\JDS\\OpenAI-API-Wrapper\\data\\chatdata.db"))
            {
                await systemHandler.CreateDatabase();
            }
            
            MainFrame.Navigate(systemHandler.DallEGen ??= new Pages.DallEGenerator());
        }

        private void NavigateSettings_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the Settings page, don't navigate to it again
            if (MainFrame.Content is Pages.Settings) return;
            MainFrame.Navigate(new Pages.Settings());
        }

        private void NavigateWhisper_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the Whisper page, don't navigate to it again
            if (MainFrame.Content is Pages.Whisper) return;
            MainFrame.Navigate(_systemHandler.Whisper ??= new Pages.Whisper());
        }

        private async void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            await _systemHandler.SaveChats();
        }
    }
}