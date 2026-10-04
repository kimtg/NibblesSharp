using NibblesSharp.Graphics;

namespace NibblesSharp.Game;

public class GameConfig
{
    public int NumPlayers { get; set; } = 1;
    public int SkillLevel { get; set; } = 50;
    public bool IncreaseSpeed { get; set; } = true;
    public bool IsColorMonitor { get; set; } = true;
    public ColorPalette Colors { get; set; } = ColorPalette.ColorMode;
    public bool IsDemoMode { get; set; } = false;
    public bool QuickStart { get; set; } = false;
    public bool StartMuted { get; set; } = false;

    /// <summary>
    /// Calculates the initial tick delay in milliseconds based on skill level (1 to 100).
    /// Skill 1 = Novice (~150ms)
    /// Skill 90 = Expert (~40ms)
    /// Skill 100 = Twiddle Fingers (~25ms)
    /// </summary>
    public int CalculateInitialDelayMs()
    {
        int clamped = Math.Clamp(SkillLevel, 1, 100);
        double delay = 150.0 - (clamped - 1) * (125.0 / 99.0);
        return Math.Max(20, (int)Math.Round(delay));
    }

    public static GameConfig ParseArguments(string[] args)
    {
        var config = new GameConfig();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLowerInvariant();

            if (arg is "-h" or "--help" or "/?")
            {
                PrintHelp();
                Environment.Exit(0);
            }
            else if (arg is "--demo")
            {
                config.IsDemoMode = true;
                config.QuickStart = true;
                config.SkillLevel = 75; // Smooth viewing speed
            }
            else if (arg is "--quick")
            {
                config.QuickStart = true;
            }
            else if (arg is "--mono")
            {
                config.IsColorMonitor = false;
                config.Colors = ColorPalette.MonochromeMode;
            }
            else if (arg is "--color")
            {
                config.IsColorMonitor = true;
                config.Colors = ColorPalette.ColorMode;
            }
            else if (arg is "--mute")
            {
                config.StartMuted = true;
            }
            else if (arg is "--no-accel")
            {
                config.IncreaseSpeed = false;
            }
            else if (arg is "--players" && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out int p) && (p == 1 || p == 2))
                {
                    config.NumPlayers = p;
                }
            }
            else if (arg is "--skill" && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out int s) && s >= 1 && s <= 100)
                {
                    config.SkillLevel = s;
                }
            }
        }

        return config;
    }

    public static void PrintHelp()
    {
        Console.WriteLine(@"
QBasic Nibbles - C# Edition (Microsoft QBasic 1990 Recreation)
==============================================================

Usage:
  dotnet run [options]

Options:
  --demo            Run autonomous AI demonstration mode (press Esc or key to exit)
  --quick           Skip intro and setup screens, start directly with defaults
  --players <1|2>   Set number of players (1 or 2)
  --skill <1-100>   Set game speed/skill level (1=Novice, 90=Expert, 100=Twiddle Fingers)
  --color           Enable color mode (Default: Blue background, Yellow/Magenta snakes)
  --mono            Enable monochrome monitor mode
  --mute            Start with sound muted
  --no-accel        Disable game speed acceleration across levels
  -h, --help        Show this help message

Controls:
  Player 1 (Sammy): Arrow Keys (Up, Down, Left, Right)
  Player 2 (Jake):  W, S, A, D
  General:          P = Pause/Unpause
                    M = Toggle Sound Mute
                    Esc = Exit Game
");
    }
}
