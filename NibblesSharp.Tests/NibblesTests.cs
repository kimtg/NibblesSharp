using NibblesSharp.Audio;
using NibblesSharp.Game;
using NibblesSharp.Graphics;
using Xunit;

namespace NibblesSharp.Tests;

public class NibblesTests
{
    [Fact]
    public void QBasicPlay_ParsesIntroTuneCorrectly()
    {
        string introTune = "MBT160O1L8CDEDCDL4ECC";
        var tones = QBasicPlay.Parse(introTune);

        // CDEDCD (6 notes at L8) + ECC (3 notes at L4) = 9 notes
        Assert.Equal(9, tones.Count);

        // All frequencies must be valid audible range
        foreach (var tone in tones)
        {
            Assert.True(tone.Frequency >= 37, $"Frequency {tone.Frequency} should be >= 37");
            Assert.True(tone.DurationMs > 0, $"Duration {tone.DurationMs} should be > 0");
        }

        // T160 whole note = 240000 / 160 = 1500 ms.
        // L8 = 1500 / 8 = 187 ms.
        // L4 = 1500 / 4 = 375 ms.
        Assert.Equal(187, tones[0].DurationMs);
        Assert.Equal(375, tones[6].DurationMs);
    }

    [Fact]
    public void QBasicPlay_ParsesAllGameTunes()
    {
        var scripts = new[]
        {
            "MBT160O1L8CDEDCDL4ECC",
            "T160O1>L20CDEDCDL10ECC",
            "MBO0L16>CCCE",
            "MBO0L32EFGEFDC"
        };

        foreach (var script in scripts)
        {
            var tones = QBasicPlay.Parse(script);
            Assert.NotEmpty(tones);
            foreach (var t in tones)
            {
                Assert.True(t.Frequency > 0);
                Assert.True(t.DurationMs > 0);
            }
        }
    }

    [Fact]
    public void GameArena_OuterBordersAreBuiltProperly()
    {
        var arena = new GameArena();
        arena.Clear(ConsoleColor.DarkBlue);
        arena.BuildOuterBorders(ConsoleColor.Red);

        // Top border at row 3
        for (int col = 1; col <= 80; col++)
        {
            Assert.Equal(ConsoleColor.Red, arena[3, col]);
            Assert.Equal(ConsoleColor.Red, arena[50, col]);
        }

        // Left and right border
        for (int row = 4; row <= 49; row++)
        {
            Assert.Equal(ConsoleColor.Red, arena[row, 1]);
            Assert.Equal(ConsoleColor.Red, arena[row, 80]);
        }

        // Inside space is background
        Assert.Equal(ConsoleColor.DarkBlue, arena[10, 10]);
    }

    [Fact]
    public void GameArena_CoordinateConversionsMatchQBasicFormulas()
    {
        // In QBasic:
        // arena(row, col).sister = (row MOD 2) * 2 - 1
        // realRow = INT((row + 1) / 2)

        // Odd arena row 3 (top subpixel of screen row 2): sister is below (+1 -> 4)
        Assert.Equal(4, GameArena.GetSisterRow(3));
        Assert.Equal(2, GameArena.GetRealRow(3));

        // Even arena row 4 (bottom subpixel of screen row 2): sister is above (-1 -> 3)
        Assert.Equal(3, GameArena.GetSisterRow(4));
        Assert.Equal(2, GameArena.GetRealRow(4));

        // Bottom border arena row 50 (screen row 25)
        Assert.Equal(25, GameArena.GetRealRow(50));
        Assert.Equal(49, GameArena.GetSisterRow(50));
    }

    [Fact]
    public void Snake_PreventsImmediate180Reversal()
    {
        var snake = new Snake("Sammy", ConsoleColor.Yellow)
        {
            Direction = Direction.Right,
            QueuedDirection = Direction.Right
        };

        // Try reversing left directly
        snake.SetDirection(Direction.Left);
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Right, snake.Direction);

        // Turn Up is allowed
        snake.SetDirection(Direction.Up);
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Up, snake.Direction);

        // Try reversing Down directly
        snake.SetDirection(Direction.Down);
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Up, snake.Direction);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void LevelManager_InitializesAllLevelsWithoutError(int level)
    {
        var arena = new GameArena();
        var sammy = new Snake("Sammy", ConsoleColor.Yellow);
        var jake = new Snake("Jake", ConsoleColor.Magenta);
        var colors = ColorPalette.ColorMode;

        LevelManager.InitializeLevel(level, arena, sammy, jake, colors);

        Assert.True(sammy.Row >= 3 && sammy.Row <= 50);
        Assert.True(sammy.Col >= 1 && sammy.Col <= 80);
        Assert.True(jake.Row >= 3 && jake.Row <= 50);
        Assert.True(jake.Col >= 1 && jake.Col <= 80);
        Assert.NotEqual(Direction.None, sammy.Direction);
        Assert.NotEqual(Direction.None, jake.Direction);
    }

    [Fact]
    public void GameConfig_SkillLevelCalculatesReasonableDelay()
    {
        var config = new GameConfig();

        config.SkillLevel = 1;
        int delay1 = config.CalculateInitialDelayMs();
        Assert.True(delay1 >= 140 && delay1 <= 160);

        config.SkillLevel = 50;
        int delay50 = config.CalculateInitialDelayMs();
        Assert.True(delay50 >= 70 && delay50 <= 100);

        config.SkillLevel = 100;
        int delay100 = config.CalculateInitialDelayMs();
        Assert.True(delay100 <= 30);
    }

    [Fact]
    public void GameConfig_ParseArguments_ParsesCorrectly()
    {
        var config = GameConfig.ParseArguments(new[] { "--players", "2", "--skill", "80", "--mono", "--mute", "--no-accel" });
        Assert.Equal(2, config.NumPlayers);
        Assert.Equal(80, config.SkillLevel);
        Assert.False(config.IsColorMonitor);
        Assert.True(config.StartMuted);
        Assert.False(config.IncreaseSpeed);

        var demoConfig = GameConfig.ParseArguments(new[] { "--demo" });
        Assert.True(demoConfig.IsDemoMode);
        Assert.True(demoConfig.QuickStart);
    }

    [Fact]
    public void NibblesAI_FindsSafeMoveTowardTarget()
    {
        var arena = new GameArena();
        var colors = ColorPalette.ColorMode;
        arena.Clear(colors.Background);
        arena.BuildOuterBorders(colors.Wall);

        var sammy = new Snake("Sammy", colors.Snake1)
        {
            Row = 25,
            Col = 25,
            Direction = Direction.Right
        };

        // Target at realRow 20, col 30 (which is arena row 39 or 40, col 30)
        // targetScreenRow0 = 19, targetCol0 = 29
        int targetScreenRow0 = 19;
        int targetCol0 = 29;

        var move = NibblesAI.DetermineNextMove(sammy, arena, colors, targetScreenRow0, targetCol0);
        Assert.NotEqual(Direction.None, move);
        Assert.NotEqual(Direction.Left, move); // Should not reverse
    }
}
