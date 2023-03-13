using System.Windows;
using OpenAI_API_Wrapper.Classes;

namespace OpenAI_API_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigateDallE_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the DallE page, don't navigate to it again
            if (MainFrame.Content is Pages.DallEGenerator) return;
            MainFrame.Navigate(new Pages.DallEGenerator());
        }

        private void NavigateChatGPT_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the ChatGPT page, don't navigate to it again
            if (MainFrame.Content is Pages.ChatGPTMessanger) return;
            MainFrame.Navigate(new Pages.ChatGPTMessanger());
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var systemHandler = SystemHandler.Instance;
            systemHandler.InitOpenApiService();
            MainFrame.Navigate(new Pages.DallEGenerator());
        }

        private void NavigateSettings_OnClick(object sender, RoutedEventArgs e)
        {
            // If the frame is already on the Settings page, don't navigate to it again
            if (MainFrame.Content is Pages.Settings) return;
            MainFrame.Navigate(new Pages.Settings());
        }
    }
}