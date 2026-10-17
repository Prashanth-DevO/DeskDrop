using Avalonia;
using System;
using System.Linq;

namespace AvaloniaFileTransfer;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--verify")
        {
            var app = BuildAvaloniaApp().SetupWithoutStarting();

            Console.WriteLine("[1/5] Verifying NetworkService & SettingsService...");
            var ip = Services.NetworkService.GetPrimaryLocalIp();
            Console.WriteLine($"      Detected IP: {ip}");
            var settings = Services.SettingsService.Instance.Settings;
            Console.WriteLine($"      Settings: Port={settings.DefaultPort}, Theme={settings.Theme}, Dir={settings.DefaultDownloadDirectory}");

            Console.WriteLine("[2/5] Verifying MainWindow...");
            var mainWin = new Views.MainWindow();
            Console.WriteLine($"      MainWindow Title: '{mainWin.Title}'");

            Console.WriteLine("[3/5] Verifying SenderView interactions...");
            var senderWin = new Views.SenderView();
            // Test transfer validation without files
            senderWin.StartTransferButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            if (!senderWin.TransferStatusText.Text!.Contains("Select at least one file"))
                throw new Exception("Sender validation failed!");
            Console.WriteLine($"      Empty file validation: '{senderWin.TransferStatusText.Text}'");

            Console.WriteLine("[4/5] Verifying ReceiverView interactions...");
            var receiverWin = new Views.ReceiverView();
            
            // Test Start Receiving
            receiverWin.StartReceivingButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            if (!receiverWin.IsReceiving || !receiverWin.StatusText.Text!.Contains("Listening"))
                throw new Exception("Receiver start listening failed!");
            Console.WriteLine($"      Start receiving status: '{receiverWin.StatusText.Text}'");

            // Test Stop Receiving
            receiverWin.StopReceivingButton.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Avalonia.Controls.Button.ClickEvent));
            if (receiverWin.IsReceiving || !receiverWin.StatusText.Text!.Contains("stopped"))
                throw new Exception("Receiver stop listening failed!");
            Console.WriteLine($"      Stop receiving status: '{receiverWin.StatusText.Text}'");

            // Test Incoming Transfer Item
            receiverWin.AddIncomingTransfer(new Models.TransferItem
            {
                FileName = "test-doc.pdf",
                FileSize = 1048576,
                SenderIp = "192.168.1.55",
                Status = "Completed"
            });
            Console.WriteLine($"      Incoming transfers list count: '{receiverWin.TransferCountText.Text}'");

            Console.WriteLine("[5/5] Verifying SettingsView interactions...");
            var settingsWin = new Views.SettingsView();
            Console.WriteLine($"      SettingsView Title: '{settingsWin.Title}', Port: {settingsWin.PortTextBox.Text}");

            Console.WriteLine("ALL UI INTERACTIONS AND STATE TRANSITIONS VERIFIED SUCCESSFULLY!");
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
