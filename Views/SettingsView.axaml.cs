using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaFileTransfer.Models;
using AvaloniaFileTransfer.Services;

namespace AvaloniaFileTransfer.Views;

public partial class SettingsView : Window
{
    private string _currentDownloadPath = string.Empty;

    public SettingsView()
    {
        InitializeComponent();
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        var settings = SettingsService.Instance.Settings;

        _currentDownloadPath = settings.DefaultDownloadDirectory;
        DownloadPathTextBox.Text = _currentDownloadPath;
        PortTextBox.Text = settings.DefaultPort.ToString();

        ThemeComboBox.SelectedIndex = settings.Theme switch
        {
            "Dark" => 1,
            "System" => 2,
            _ => 0 // Light
        };
    }

    private async void BrowseDownloadDir_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select Default Download Directory",
                AllowMultiple = false
            });

        if (folders.Count > 0)
        {
            var path = folders[0].TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
            {
                _currentDownloadPath = path;
                DownloadPathTextBox.Text = path;
            }
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        ValidationMessageText.IsVisible = false;

        if (!int.TryParse(PortTextBox.Text, out int port) || port < 1 || port > 65535)
        {
            ValidationMessageText.Text = "Port must be an integer between 1 and 65535.";
            ValidationMessageText.IsVisible = true;
            return;
        }

        string theme = ThemeComboBox.SelectedIndex switch
        {
            1 => "Dark",
            2 => "System",
            _ => "Light"
        };

        var newSettings = new AppSettings
        {
            Theme = theme,
            DefaultDownloadDirectory = string.IsNullOrWhiteSpace(_currentDownloadPath) 
                ? AppSettings.GetDefaultDownloadPath() 
                : _currentDownloadPath,
            DefaultPort = port
        };

        SettingsService.Instance.SaveSettings(newSettings);
        Close();
    }
}
