using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaFileTransfer.Models;
using AvaloniaFileTransfer.Services;

namespace AvaloniaFileTransfer.Views;

public partial class ReceiverView : Window
{
    private readonly List<TransferItem> _incomingTransfers = new();
    private bool _isReceiving = false;
    private string _destinationPath = string.Empty;

    public bool IsReceiving => _isReceiving;

    public ReceiverView()
    {
        InitializeComponent();
        InitializeReceiver();
    }

    private void InitializeReceiver()
    {
        // 1. IP
        string primaryIp = NetworkService.GetPrimaryLocalIp();
        LocalIpTextBox.Text = primaryIp;

        // 2. Settings
        var settings = SettingsService.Instance.Settings;
        PortTextBox.Text = settings.DefaultPort.ToString();
        _destinationPath = settings.DefaultDownloadDirectory;
        DestinationPathTextBox.Text = _destinationPath;

        // 3. Initial UI state
        UpdateTransferList();
    }

    private void Back_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void CopyIp_Click(object? sender, RoutedEventArgs e)
    {
        if (Clipboard != null && !string.IsNullOrEmpty(LocalIpTextBox.Text))
        {
            await Clipboard.SetTextAsync(LocalIpTextBox.Text);
            CopyFeedbackText.IsVisible = true;
            await Task.Delay(2000);
            CopyFeedbackText.IsVisible = false;
        }
    }

    private async void BrowseFolder_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select Destination Folder for Received Files",
                AllowMultiple = false
            });

        if (folders.Count > 0)
        {
            var path = folders[0].TryGetLocalPath();
            if (!string.IsNullOrEmpty(path))
            {
                _destinationPath = path;
                DestinationPathTextBox.Text = path;
            }
        }
    }

    private void StartReceiving_Click(object? sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PortTextBox.Text, out int port) || port < 1 || port > 65535)
        {
            StatusText.Text = "Status Error: Enter a valid port between 1 and 65535.";
            StatusDot.Fill = Brush.Parse("#D93636");
            return;
        }

        if (string.IsNullOrWhiteSpace(_destinationPath) || !Directory.Exists(_destinationPath))
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_destinationPath))
                {
                    Directory.CreateDirectory(_destinationPath);
                }
                else
                {
                    _destinationPath = AppSettings.GetDefaultDownloadPath();
                    DestinationPathTextBox.Text = _destinationPath;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Status Error: Invalid destination folder ({ex.Message}).";
                StatusDot.Fill = Brush.Parse("#D93636");
                return;
            }
        }

        _isReceiving = true;
        StatusText.Text = $"Status: Listening on http://{LocalIpTextBox.Text}:{port}/ · Ready to receive files";
        StatusDot.Fill = Brush.Parse("#15952B");

        PortTextBox.IsEnabled = false;
        BrowseFolderButton.IsEnabled = false;
        StartReceivingButton.IsEnabled = false;
        StopReceivingButton.IsEnabled = true;
    }

    private void StopReceiving_Click(object? sender, RoutedEventArgs e)
    {
        _isReceiving = false;
        StatusText.Text = "Status: Receiver stopped. Listener is inactive.";
        StatusDot.Fill = Brush.Parse("#888888");

        PortTextBox.IsEnabled = true;
        BrowseFolderButton.IsEnabled = true;
        StartReceivingButton.IsEnabled = true;
        StopReceivingButton.IsEnabled = false;
    }

    public void AddIncomingTransfer(TransferItem item)
    {
        _incomingTransfers.Add(item);
        UpdateTransferList();
    }

    private void UpdateTransferList()
    {
        IncomingTransfersList.Children.Clear();

        if (_incomingTransfers.Count == 0)
        {
            EmptyTransfersPanel.IsVisible = true;
            TransferCountText.Text = "0 transfers";
            return;
        }

        EmptyTransfersPanel.IsVisible = false;
        TransferCountText.Text = $"{_incomingTransfers.Count} transfer{(_incomingTransfers.Count == 1 ? "" : "s")}";

        foreach (var item in _incomingTransfers)
        {
            var rowBorder = new Border
            {
                BorderBrush = Brush.Parse("#E2E8F0"),
                BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
                Padding = new Avalonia.Thickness(12, 8)
            };

            var rowGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("2*,1.5*,*,Auto"),
                ColumnSpacing = 10
            };

            var nameText = new TextBlock
            {
                Text = item.FileName,
                FontSize = 13,
                Foreground = Brush.Parse("#1E293B"),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var ipText = new TextBlock
            {
                Text = string.IsNullOrEmpty(item.SenderIp) ? "Unknown" : item.SenderIp,
                FontSize = 13,
                Foreground = Brush.Parse("#64748B"),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            Grid.SetColumn(ipText, 1);

            var sizeText = new TextBlock
            {
                Text = item.FormattedSize,
                FontSize = 13,
                Foreground = Brush.Parse("#334155"),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            Grid.SetColumn(sizeText, 2);

            var statusBadge = new Border
            {
                Background = item.Status == "Completed" ? Brush.Parse("#DCFCE7") : Brush.Parse("#E0F2FE"),
                CornerRadius = new Avalonia.CornerRadius(3),
                Padding = new Avalonia.Thickness(8, 2),
                Width = 80,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };

            var statusText = new TextBlock
            {
                Text = item.Status,
                FontSize = 12,
                Foreground = item.Status == "Completed" ? Brush.Parse("#15803D") : Brush.Parse("#0369A1"),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            statusBadge.Child = statusText;
            Grid.SetColumn(statusBadge, 3);

            rowGrid.Children.Add(nameText);
            rowGrid.Children.Add(ipText);
            rowGrid.Children.Add(sizeText);
            rowGrid.Children.Add(statusBadge);

            rowBorder.Child = rowGrid;
            IncomingTransfersList.Children.Add(rowBorder);
        }
    }
}
