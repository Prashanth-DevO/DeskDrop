using System;
using System.IO;

namespace AvaloniaFileTransfer.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Light";
    public string DefaultDownloadDirectory { get; set; } = GetDefaultDownloadPath();
    public int DefaultPort { get; set; } = 8080;

    public static string GetDefaultDownloadPath()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string downloads = Path.Combine(userProfile, "Downloads");
        return Directory.Exists(downloads) ? downloads : userProfile;
    }
}
