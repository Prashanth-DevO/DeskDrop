using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaFileTransfer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenSender_Click(object? sender, RoutedEventArgs e)
    {
        var senderWindow = new SenderView();
        await senderWindow.ShowDialog(this);
    }

    private async void OpenReceiver_Click(object? sender, RoutedEventArgs e)
    {
        var receiverWindow = new ReceiverView();
        await receiverWindow.ShowDialog(this);
    }

    private async void Settings_Click(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsView();
        await settingsWindow.ShowDialog(this);
    }
}