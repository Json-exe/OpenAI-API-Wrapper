using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using OpenAI_API_Wrapper.Classes;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using ToastNotifications.Messages;

namespace OpenAI_API_Wrapper.Pages;

public partial class Whisper : Page
{
    private readonly SystemHandler _systemHandler = SystemHandler.Instance;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public Whisper()
    {
        InitializeComponent();
        _systemHandler.Whisper = this;
    }
    
    private void DisableControls()
    {
        WhisperMode.IsEnabled = false;
        MagicButton.IsEnabled = false;
        Upload.IsEnabled = false;
        Processing.Visibility = Visibility.Visible;
        Output.Visibility = Visibility.Collapsed;
        Language.IsEnabled = false;
    }
    
    private void EnableControls()
    {
        WhisperMode.IsEnabled = true;
        MagicButton.IsEnabled = true;
        Upload.IsEnabled = true;
        Processing.Visibility = Visibility.Collapsed;
        Output.Visibility = Visibility.Visible;
        Language.IsEnabled = true;
    }

    private async void CallWhisper_OnClick(object sender, RoutedEventArgs e)
    {
        if (_systemHandler.OpenAiService == null)
        {
            _systemHandler.Notifier.ShowWarning("OpenAI API Key is not set! Please set it in the settings page!");
            return;
        }

        if (!File.Exists(FilePath.Text))
        {
            _systemHandler.Notifier.ShowInformation("Please select a file!");
            return;
        }
        DisableControls();
        Output.Text = string.Empty;
        var fileBytes = await File.ReadAllBytesAsync(FilePath.Text);
        if (WhisperMode.Text == "Transcription")
        {
            var whisperMode = await _systemHandler.OpenAiService!.CreateTranscription(new AudioCreateTranscriptionRequest()
            {
                Model = Models.WhisperV1,
                File = fileBytes,
                Language = Language.Text,
                FileName = Path.GetFileName(FilePath.Text)
            });
            if (whisperMode.Successful)
            {
                // The returned data is a JSON string with { "text": "..." }
                var data = JsonConvert.DeserializeObject<Transcription>(whisperMode.Text);
                _systemHandler.Notifier.ShowSuccess("Successfully created transcription!");
                Log.Debug("Successfully created transcription!");
                Log.Info(whisperMode.Text);
                Output.Text = data?.text;
                // var dialog = new SaveFileDialog
                // {
                //     Title = "Save transcription",
                //     Filter = "Text files (*.txt)|*.txt"
                // };
                // if (dialog.ShowDialog() != true) return;
                // var file = dialog.FileName;
                // // Check if the file exists and is not empty
                // if (string.IsNullOrEmpty(file))
                // {
                //     _systemHandler.Notifier.ShowError("Please select a valid file!");
                //     return;
                // }
                // await File.WriteAllTextAsync(file, data);
                // _systemHandler.Notifier.ShowSuccess("Successfully saved transcription!");
                // Log.Info("Successfully saved transcription!");
            }
            else
            {
                _systemHandler.Notifier.ShowError("Something went wrong! Please try again! Error Message: " + whisperMode.Error);
                Log.Error(whisperMode.Error);
            }
        }
        else
        {
            var whisperMode = await _systemHandler.OpenAiService!.CreateTranslation(new AudioCreateTranscriptionRequest()
            {
                File = fileBytes,
                Model = Models.WhisperV1,
                Language = Language.Text,
                FileName = Path.GetFileName(FilePath.Text)
            });
            if (whisperMode.Successful)
            {
                // The returned data is a JSON string with { "text": "..." }
                var data = JsonConvert.DeserializeObject<Transcription>(whisperMode.Text);
                _systemHandler.Notifier.ShowSuccess("Successfully created translation!");
                Log.Debug("Successfully created translation!");
                Log.Info(whisperMode.Text);
                Output.Text = data?.text;
            }
            else
            {
                _systemHandler.Notifier.ShowError("Something went wrong! Please try again! Error Message: " + whisperMode.Error);
                Log.Error(whisperMode.Error);
            }
        }
        EnableControls();
    }

    private void Upload_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a audio file",
            Filter = "Audio files (*.mp3, *.wav, *.mp4, *.mpeg, *.mpga, *.m4a, *.webm)|*.mp3;*.wav;*.mp4;*.mpeg,*.mpga,*.m4a,*.webm"
        };
        if (dialog.ShowDialog() != true) return;
        var file = dialog.FileName;
        // Check if the file exists and is not empty
        if (string.IsNullOrEmpty(file))
        {
            _systemHandler.Notifier.ShowError("Please select a valid audio file!");
            return;
        }
        // Check if the file is bigger than 26214400 bytes (25 MB)
        if (new FileInfo(file).Length > 26214400)
        {
            _systemHandler.Notifier.ShowError("Please select a audio file that is smaller than 25 MB! (Whisper dont support bigger files))");
            return;
        }
        FilePath.Text = file;
    }
}

internal class Transcription
{
    public string text { get; set; }
}