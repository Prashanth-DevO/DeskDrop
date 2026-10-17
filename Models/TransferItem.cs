using System;

namespace AvaloniaFileTransfer.Models;

public class TransferItem
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string SenderIp { get; set; } = string.Empty;
    public string Status { get; set; } = "Ready";
    public double Progress { get; set; }

    public string FormattedSize => FormatBytes(FileSize);

    public static string FormatBytes(long bytes)
    {
        if (bytes < 0) return "0 B";
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }
        return $"{size:0.0} {units[unit]}";
    }
}
