using System.Text.Json;

namespace NibblesSharp.Game;

public record HighScoreEntry(string Name, int Score, int Level, string Mode, DateTime Date);

/// <summary>
/// Manages persistent arcade-style leaderboard tracking.
/// </summary>
public class HighScoreManager
{
    private const int MaxEntries = 10;
    private readonly string _filePath;
    private List<HighScoreEntry> _scores = new();

    public HighScoreManager(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _filePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dir = string.IsNullOrWhiteSpace(appData) ? "." : Path.Combine(appData, "NibblesSharp");
            _filePath = Path.Combine(dir, "highscores.json");
        }

        Load();
    }

    public IReadOnlyList<HighScoreEntry> Scores => _scores.AsReadOnly();

    public bool IsHighScore(int score)
    {
        if (score <= 0) return false;
        if (_scores.Count < MaxEntries) return true;
        return score > _scores[^1].Score;
    }

    public void AddScore(string name, int score, int level, string mode)
    {
        string sanitizedName = string.IsNullOrWhiteSpace(name) ? "SAM" : name.Trim().ToUpperInvariant();
        if (sanitizedName.Length > 8) sanitizedName = sanitizedName[..8];

        _scores.Add(new HighScoreEntry(sanitizedName, score, level, mode, DateTime.UtcNow));
        _scores = _scores
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.Level)
            .Take(MaxEntries)
            .ToList();

        Save();
    }

    public void Clear()
    {
        _scores.Clear();
        Save();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<List<HighScoreEntry>>(json);
                if (loaded != null)
                {
                    _scores = loaded
                        .OrderByDescending(s => s.Score)
                        .ThenByDescending(s => s.Level)
                        .Take(MaxEntries)
                        .ToList();
                    return;
                }
            }
        }
        catch
        {
            // Ignore file read/parse errors and start fresh
        }

        // Initialize with default retro records if empty
        _scores = new List<HighScoreEntry>
        {
            new("RICK", 150, 5, "Solo", DateTime.UtcNow),
            new("SAMMY", 100, 4, "Solo", DateTime.UtcNow),
            new("JAKE", 75, 3, "Solo", DateTime.UtcNow),
            new("QBASIC", 50, 2, "Solo", DateTime.UtcNow),
            new("NOVICE", 25, 1, "Solo", DateTime.UtcNow)
        };
    }

    private void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonSerializer.Serialize(_scores, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Do not crash if disk access fails
        }
    }

    public void PrintScoresToConsole()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
       =========================================================
                    Q B A S I C   N I B B L E S
                        H I G H   S C O R E S
       =========================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("       Rank  Player    Score    Level    Mode    Date");
        Console.WriteLine("       ----  ------    -----    -----    ----    ----------");

        for (int i = 0; i < _scores.Count; i++)
        {
            var s = _scores[i];
            Console.ForegroundColor = i switch
            {
                0 => ConsoleColor.Yellow,
                1 => ConsoleColor.Cyan,
                2 => ConsoleColor.Green,
                _ => ConsoleColor.Gray
            };

            Console.WriteLine($"       #{i + 1,-4} {s.Name,-9} {s.Score,-8} {s.Level,-8} {s.Mode,-7} {s.Date:yyyy-MM-dd}");
        }

        Console.ResetColor();
        Console.WriteLine();
    }
}
