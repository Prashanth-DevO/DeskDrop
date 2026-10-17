using System;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Styling;
using AvaloniaFileTransfer.Models;

namespace AvaloniaFileTransfer.Services;

public class SettingsService
{
    private static SettingsService? _instance;
    public static SettingsService Instance => _instance ??= new SettingsService();

    private readonly string _settingsPath;
    public AppSettings Settings { get; private set; }

    private SettingsService()
    {
        string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "DeskDrop");

        try
        {
            Directory.CreateDirectory(configDir);
        }
        catch
        {
            // Fallback to local app directory if config cannot be created
            configDir = AppContext.BaseDirectory;
        }

        _settingsPath = Path.Combine(configDir, "settings.json");
        Settings = LoadSettings();
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                string json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return new AppSettings();
    }

    public void SaveSettings(AppSettings newSettings)
    {
        Settings = newSettings;
        try
        {
            string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }

        ApplyTheme(Settings.Theme);
    }

    public void ApplyTheme(string theme)
    {
        if (Application.Current is null) return;

        Application.Current.RequestedThemeVariant = theme switch
        {
            "Dark" => ThemeVariant.Dark,
            "Light" => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }
}
