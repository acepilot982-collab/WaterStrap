using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace WaterStrap.Mac;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<RecentGame> _recentGames = new();
    private readonly string _dataDirectory;
    private string GamesFile => Path.Combine(_dataDirectory, "recent-games.json");

    public MainWindow()
    {
        InitializeComponent();
        _dataDirectory = GetDataDirectory();
        Directory.CreateDirectory(_dataDirectory);
        RecentGamesList.ItemsSource = _recentGames;
        LoadRecentGames();
    }

    private void FocusLaunch_Click(object? sender, RoutedEventArgs e)
    {
        PlaceIdBox.Focus();
        StatusText.Text = "Enter a numeric place ID or a Roblox game URL.";
    }

    private void OpenRoblox_Click(object? sender, RoutedEventArgs e) =>
        OpenUrl("https://www.roblox.com/home", "Opening Roblox home in your default browser...");

    private void OpenDownload_Click(object? sender, RoutedEventArgs e) =>
        OpenUrl("https://www.roblox.com/download", "Opening the official Roblox download page...");

    private void OpenRepository_Click(object? sender, RoutedEventArgs e) =>
        OpenUrl("https://github.com/acepilot982-collab/WaterStrap", "Opening the WaterStrap source repository...");

    private void LaunchGame_Click(object? sender, RoutedEventArgs e)
    {
        string input = PlaceIdBox.Text?.Trim() ?? string.Empty;
        string? placeId = ExtractPlaceId(input);

        if (placeId is null)
        {
            StatusText.Text = "Enter a valid numeric place ID or a Roblox game URL.";
            return;
        }

        AddRecentGame(placeId);
        OpenUrl($"https://www.roblox.com/games/{placeId}", $"Opening experience {placeId} in your browser...");
    }

    private void RecentGame_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id } || !Regex.IsMatch(id, @"^\d{1,20}$"))
            return;

        PlaceIdBox.Text = id;
        OpenUrl($"https://www.roblox.com/games/{id}", $"Opening experience {id} in your browser...");
    }

    private void AddRecentGame(string placeId)
    {
        var existing = _recentGames.FirstOrDefault(x => x.PlaceId == placeId);
        if (existing is not null)
            _recentGames.Remove(existing);

        _recentGames.Insert(0, new RecentGame
        {
            Title = $"Roblox experience {placeId}",
            PlaceId = placeId
        });

        while (_recentGames.Count > 8)
            _recentGames.RemoveAt(_recentGames.Count - 1);

        SaveRecentGames();
    }

    private void LoadRecentGames()
    {
        try
        {
            if (!File.Exists(GamesFile)) return;
            var saved = JsonSerializer.Deserialize<List<RecentGame>>(File.ReadAllText(GamesFile)) ?? new();
            foreach (var game in saved.Where(x => Regex.IsMatch(x.PlaceId ?? string.Empty, @"^\d{1,20}$")).Take(8))
                _recentGames.Add(game);
        }
        catch
        {
            // A damaged recent-games file must not prevent the launcher from opening.
        }
    }

    private void SaveRecentGames()
    {
        try
        {
            File.WriteAllText(GamesFile, JsonSerializer.Serialize(_recentGames, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not save recent games: {ex.Message}";
        }
    }

    private static string GetDataDirectory()
    {
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (OperatingSystem.IsMacOS())
            return Path.Combine(home, "Library", "Application Support", "WaterStrap");
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WaterStrap");
        return Path.Combine(home, ".local", "share", "WaterStrap");
    }

    private static string? ExtractPlaceId(string input)
    {
        if (Regex.IsMatch(input, @"^\d{1,20}$")) return input;
        if (!Uri.TryCreate(input, UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme is not ("https" or "http")) return null;

        bool hostMatches = uri.Host.Equals("roblox.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith(".roblox.com", StringComparison.OrdinalIgnoreCase);
        if (!hostMatches) return null;

        var match = Regex.Match(uri.AbsolutePath, @"/games/(\d+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private void OpenUrl(string url, string successMessage)
    {
        try
        {
            ProcessStartInfo startInfo;
            if (OperatingSystem.IsMacOS())
            {
                startInfo = new ProcessStartInfo("open") { UseShellExecute = false };
                startInfo.ArgumentList.Add(url);
            }
            else if (OperatingSystem.IsWindows())
            {
                startInfo = new ProcessStartInfo(url) { UseShellExecute = true };
            }
            else
            {
                startInfo = new ProcessStartInfo("xdg-open") { UseShellExecute = false };
                startInfo.ArgumentList.Add(url);
            }

            Process.Start(startInfo);
            StatusText.Text = successMessage;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not open link: {ex.Message}";
        }
    }

    public sealed class RecentGame
    {
        public string Title { get; set; } = "Roblox experience";
        public string PlaceId { get; set; } = string.Empty;
    }
}
