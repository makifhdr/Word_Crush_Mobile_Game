using System.Diagnostics;
using System.Text.Json;
using MauiApp5.Models;

namespace MauiApp5.Services;

public class DataService
{
    private readonly string _filePath = Path.Combine(FileSystem.AppDataDirectory, "appdata.json");

    public async Task<AppData> LoadAsync()
    {
        if (IsFirstLaunch())
            return new AppData();

        string json = await File.ReadAllTextAsync(_filePath);
        
        return JsonSerializer.Deserialize<AppData>(json) ?? throw new InvalidOperationException();
    }

    public async Task SaveAsync(AppData data)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_filePath, json);
    }
    
    public bool IsFirstLaunch()
    {
        return !File.Exists(_filePath);
    }
}