using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaFileTransfer.Models;
using AvaloniaFileTransfer.Services;

namespace AvaloniaFileTransfer.Views;

public partial class SenderView : Window
{
    private readonly List<string> _selectedFiles = new();

    public SenderView()
    {
        InitializeComponent();
        InitializeSender();
    }

    private void InitializeSender()
    {
        // Setup Drag & Drop handlers
        FileDropBorder.AddHandler(DragDrop.DragOverEvent, DropArea_DragOver);
        FileDropBorder.AddHandler(DragDrop.DragEnterEvent, DropArea_DragEnter);
        FileDropBorder.AddHandler(DragDrop.DragLeaveEvent, DropArea_DragLeave);
        FileDropBorder.AddHandler(DragDrop.DropEvent, DropArea_Drop);

        // Initialize default port from settings
        PortTextBox.Text = SettingsService.Instance.Settings.DefaultPort.ToString();

        // Refresh initial file list
        RefreshFileList();
    }

    private void Back_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void DropArea_DragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private void DropArea_DragEnter(object? sender, DragEventArgs e)
    {
        FileDropBorder.Background = Brush.Parse("#D8E8F5");
        FileDropBorder.BorderBrush = Brush.Parse("#0875C9");
        DropHintText.Text = "Release to add files or folders";
    }

    private void DropArea_DragLeave(object? sender, DragEventArgs e)
    {
        FileDropBorder.Background = Brush.Parse("#E8EEF2");
        FileDropBorder.BorderBrush = Brush.Parse("#A8B4BE");
        DropHintText.Text = "Drop files or folders here, or browse your computer";
    }

    private void DropArea_Drop(object? sender, DragEventArgs e)
    {
        FileDropBorder.Background = Brush.Parse("#E8EEF2");
        FileDropBorder.BorderBrush = Brush.Parse("#A8B4BE");
        DropHintText.Text = "Drop files or folders here, or browse your computer";

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            foreach (var item in files)
            {
                var path = item.TryGetLocalPath();
                if (string.IsNullOrEmpty(path)) continue;

                if (File.Exists(path))
                {
                    if (!_selectedFiles.Contains(path))
                    {
                        _selectedFiles.Add(path);
                    }
                }
                else if (Directory.Exists(path))
                {
                    AddDirectoryFiles(path);
                }
            }

            RefreshFileList();
        }
    }

    private async void AddFiles_Click(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Select files to send",
                AllowMultiple = true
            });

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (path != null && !_selectedFiles.Contains(path))
            {
                _selectedFiles.Add(path);
            }
        }

        RefreshFileList();
    }

    private async void AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Select a folder to send",
                AllowMultiple = true
            });

        foreach (var folder in folders)
        {
            var path = folder.TryGetLocalPath();
            if (path == null) continue;

            AddDirectoryFiles(path);
        }

        RefreshFileList();
    }

    private void AddDirectoryFiles(string dirPath)
    {
        try
        {
            foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                if (!_selectedFiles.Contains(file))
                {
                    _selectedFiles.Add(file);
                }
            }
        }
        catch (Exception ex)
        {
            TransferStatusText.Text = $"Status: Error reading folder: {ex.Message}";
        }
    }

    private void ClearAll_Click(object? sender, RoutedEventArgs e)
    {
        _selectedFiles.Clear();
        RefreshFileList();
    }

    private void RefreshFileList()
    {
        SelectedFilesList.Children.Clear();

        if (_selectedFiles.Count == 0)
        {
            EmptyFilesPanel.IsVisible = true;
            ClearAllButton.IsVisible = false;
            FileSummary.Text = "0 files · 0 B";
            return;
        }

        EmptyFilesPanel.IsVisible = false;
        ClearAllButton.IsVisible = true;

        long totalBytes = 0;

        foreach (var path in _selectedFiles.ToList())
        {
            long fileSize = 0;
            string fileName = Path.GetFileName(path);

            try
            {
                var info = new FileInfo(path);
                fileSize = info.Length;
                totalBytes += fileSize;
            }
            catch
            {
                // Inaccessible file
            }

            var rowBorder = new Border
            {
                BorderBrush = Brush.Parse("#E2E8F0"),
                BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
                Padding = new Avalonia.Thickness(10, 6)
            };

            var rowGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
                ColumnSpacing = 12
            };

            var nameTextBlock = new TextBlock
            {
                Text = fileName,
                FontSize = 13,
                Foreground = Brush.Parse("#1E293B"),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            ToolTip.SetTip(nameTextBlock, path);

            var sizeTextBlock = new TextBlock
            {
                Text = TransferItem.FormatBytes(fileSize),
                FontSize = 13,
                Foreground = Brush.Parse("#64748B"),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            Grid.SetColumn(sizeTextBlock, 1);

            var removeBtn = new Button
            {
                Content = "[ × ]",
                Tag = path,
                Background = Brush.Parse("#FEE2E2"),
                Foreground = Brush.Parse("#DC2626"),
                BorderBrush = Brush.Parse("#FECACA"),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(3),
                Padding = new Avalonia.Thickness(6, 2),
                FontSize = 12,
                Cursor = new Cursor(StandardCursorType.Hand)
            };

            removeBtn.Click += (_, _) =>
            {
                _selectedFiles.Remove(path);
                RefreshFileList();
            };
            Grid.SetColumn(removeBtn, 2);

            rowGrid.Children.Add(nameTextBlock);
            rowGrid.Children.Add(sizeTextBlock);
            rowGrid.Children.Add(removeBtn);

            rowBorder.Child = rowGrid;
            SelectedFilesList.Children.Add(rowBorder);
        }

        FileSummary.Text = $"{_selectedFiles.Count} file{(_selectedFiles.Count == 1 ? "" : "s")} · {TransferItem.FormatBytes(totalBytes)}";
    }

    private void StartTransfer_Click(object? sender, RoutedEventArgs e)
    {
        if (_selectedFiles.Count == 0)
        {
            TransferStatusText.Text = "Status: Select at least one file first.";
            return;
        }

        string receiverIp = ReceiverIpTextBox.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(receiverIp))
        {
            TransferStatusText.Text = "Status: Enter receiver IP address.";
            return;
        }

        if (!int.TryParse(PortTextBox.Text, out int port) || port < 1 || port > 65535)
        {
            TransferStatusText.Text = "Status: Enter a valid port (1–65535).";
            return;
        }

        long totalBytes = 0;
        foreach (var path in _selectedFiles)
        {
            try { totalBytes += new FileInfo(path).Length; } catch { }
        }

        TransferStatusText.Text = $"Status: Ready to send {_selectedFiles.Count} files ({TransferItem.FormatBytes(totalBytes)}) to http://{receiverIp}:{port}/";
        StartTransferButton.IsEnabled = false;
        CancelTransferButton.IsEnabled = true;
    }

    private void CancelTransfer_Click(object? sender, RoutedEventArgs e)
    {
        TransferStatusText.Text = "Status: Transfer cancelled.";
        TransferProgressBar.Value = 0;
        ProgressPercentageText.Text = "0%";
        StartTransferButton.IsEnabled = true;
        CancelTransferButton.IsEnabled = false;
    }
}