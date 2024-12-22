using System.IO;
using System;
using System.Text.Json;

namespace FFMPEGConverter.Helpers;

public static class SaveSystem
{
    private static UserData userData = new();
    public static UserData UserData { get => userData; set => userData = value; }

    public static void Load()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, "FFMPEGConverter", "FFMPEGConverter.data");
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);
            UserData? ud = JsonSerializer.Deserialize<UserData>(data);
            if (ud != null) { userData = ud; }
        }
    }

    public static void Save()
    {
        string info = JsonSerializer.Serialize(userData);
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        path = Path.Combine(path, "FFMPEGConverter");
        Directory.CreateDirectory(path);
        path = Path.Combine(path, "FFMPEGConverter.data");
        File.WriteAllText(path, info);
    }
}

public class UserData
{
    public string? InputPath { get; set; }
    public string? OutputPath { get; set; }
    public int KBs { get; set; }
    public string? OutputLog { get; set; }
}