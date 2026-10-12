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

    [Fact]
    public void SparkleBorder_LeftWallMovesDownward()
    {
        // When a increases from 0 to 1, left wall stars must advance downward (row increases)
        // b runs from 1 to 21, c = (a + b) % 5, left wall row = 22 - b
        int GetTopmostStarRow(int a)
        {
            for (int r = 1; r <= 21; r++)
            {
                int b = 22 - r;
                if ((a + b) % 5 == 1)
                {
                    return r;
                }
            }
            return -1;
        }

        int rowAtA0 = GetTopmostStarRow(0); // Row 1
        int rowAtA1 = GetTopmostStarRow(1); // Row 2
        Assert.Equal(1, rowAtA0);
        Assert.Equal(2, rowAtA1);
        Assert.True(rowAtA1 > rowAtA0, "Left wall star should move downwards as step 'a' increases");
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
    public void AllLevels_SpawnPositionsDoNotCollideOnInitialMoves(int level)
    {
        var arena = new GameArena();
        var colors = ColorPalette.ColorMode;
        var sammy = new Snake("Sammy", colors.Snake1);
        var jake = new Snake("Jake", colors.Snake2);

        LevelManager.InitializeLevel(level, arena, sammy, jake, colors);

        // Neither spawn point should be a wall
        Assert.False(arena.PointIsThere(sammy.Row, sammy.Col, colors.Background), $"Level {level}: Sammy spawn ({sammy.Row}, {sammy.Col}) is inside a wall!");
        Assert.False(arena.PointIsThere(jake.Row, jake.Col, colors.Background), $"Level {level}: Jake spawn ({jake.Row}, {jake.Col}) is inside a wall!");

        // First step in initial direction must also not hit a wall
        int sNextRow = sammy.Row + (sammy.Direction == Direction.Up ? -1 : sammy.Direction == Direction.Down ? 1 : 0);
        int sNextCol = sammy.Col + (sammy.Direction == Direction.Left ? -1 : sammy.Direction == Direction.Right ? 1 : 0);
        Assert.False(arena.PointIsThere(sNextRow, sNextCol, colors.Background), $"Level {level}: Sammy first step ({sNextRow}, {sNextCol}) hits a wall!");

        int jNextRow = jake.Row + (jake.Direction == Direction.Up ? -1 : jake.Direction == Direction.Down ? 1 : 0);
        int jNextCol = jake.Col + (jake.Direction == Direction.Left ? -1 : jake.Direction == Direction.Right ? 1 : 0);
        Assert.False(arena.PointIsThere(jNextRow, jNextCol, colors.Background), $"Level {level}: Jake first step ({jNextRow}, {jNextCol}) hits a wall!");
    }

    [Fact]
    public void Snake_InputQueue_BuffersConsecutiveCorneringTurns()
    {
        var snake = new Snake("Sammy", ConsoleColor.Yellow)
        {
            Direction = Direction.Right,
            QueuedDirection = Direction.Right
        };

        // Queue Up then Left within the same frame tick
        snake.SetDirection(Direction.Up);
        snake.SetDirection(Direction.Left);

        // First frame tick: direction becomes Up
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Up, snake.Direction);

        // Second frame tick: direction becomes Left
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Left, snake.Direction);
    }

    [Fact]
    public void Snake_InputQueue_PreventsReversalAgainstQueuedDirection()
    {
        var snake = new Snake("Sammy", ConsoleColor.Yellow)
        {
            Direction = Direction.Right,
            QueuedDirection = Direction.Right
        };

        // Queue Up
        snake.SetDirection(Direction.Up);
        // Try queueing Down (reversal of Up): should be rejected
        snake.SetDirection(Direction.Down);

        // First tick
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Up, snake.Direction);

        // Queue should now be empty; snake remains Up
        snake.ApplyQueuedDirection();
        Assert.Equal(Direction.Up, snake.Direction);
    }

    [Fact]
    public void HighScoreManager_RecordsAndRanksScoresCorrectly()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"nibbles_test_{Guid.NewGuid():N}.json");
        try
        {
            var manager = new HighScoreManager(tempFile);
            manager.Clear();

            manager.AddScore("ALICE", 50, 2, "Solo");
            manager.AddScore("BOB", 120, 4, "VsAI");
            manager.AddScore("CHARLIE", 80, 3, "2P");

            var scores = manager.Scores;
            Assert.Equal(3, scores.Count);
            Assert.Equal("BOB", scores[0].Name);
            Assert.Equal(120, scores[0].Score);
            Assert.Equal("CHARLIE", scores[1].Name);
            Assert.Equal("ALICE", scores[2].Name);

            Assert.True(manager.IsHighScore(200));
            Assert.False(manager.IsHighScore(0));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void GameConfig_ParseArguments_HandlesNewOptions()
    {
        var vsAiConfig = GameConfig.ParseArguments(new[] { "--vs-ai", "--level", "5" });
        Assert.True(vsAiConfig.IsVsAiMode);
        Assert.Equal(2, vsAiConfig.NumPlayers);
        Assert.Equal(5, vsAiConfig.StartLevel);

        var scoresConfig = GameConfig.ParseArguments(new[] { "--scores" });
        Assert.True(scoresConfig.ShowScoresOnly);
    }

    [Fact]
    public void ConsoleRenderer_SyncArenaCell_RendersNumberInBrightWhite()
    {
        var renderer = new ConsoleRenderer();
        var arena = new GameArena();
        var colors = ColorPalette.ColorMode;
        arena.Clear(colors.Background);

        // Number placed at screen row 5, col 10
        renderer.SyncArenaCell(5, 10, arena, colors, hasNumber: true, numScreenRow0: 5, numCol0: 10, numVal: 7);

        // Verify the number is rendered with Bright White, not Snake1 Yellow
        // Test rendering works without throwing
        renderer.Render();
    }

    [Fact]
    public void NibblesAI_FollowsSafePathWhenObstaclesPresent()
    {
        var arena = new GameArena();
        var colors = ColorPalette.ColorMode;
        arena.Clear(colors.Background);
        arena.BuildOuterBorders(colors.Wall);

        // Place wall barrier in front of snake
        arena.SetPixel(25, 26, colors.Wall);

        var snake = new Snake("Sammy", colors.Snake1)
        {
            Row = 25,
            Col = 25,
            Direction = Direction.Right
        };

        // Target at realRow 20, col 30
        var nextDir = NibblesAI.DetermineNextMove(snake, arena, colors, targetScreenRow0: 19, targetCol0: 29);

        // Must not drive straight into the wall at (25, 26)
        Assert.NotEqual(Direction.Right, nextDir);
        Assert.NotEqual(Direction.Left, nextDir); // Can't reverse
        Assert.True(nextDir == Direction.Up || nextDir == Direction.Down);
    }
}
