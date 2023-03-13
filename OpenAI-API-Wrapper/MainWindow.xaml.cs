using System.Windows;
using OpenAI_API_Wrapper.Classes;

namespace OpenAI_API_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var systemHandler = SystemHandler.Instance;
            systemHandler.InitOpenApiService();
            _systemHandler = systemHandler;
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
    }
}