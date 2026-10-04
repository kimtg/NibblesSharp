using System.Text;
using NibblesSharp.Audio;
using NibblesSharp.Graphics;

namespace NibblesSharp.Game;

public class NibblesGame
{
    private readonly ConsoleRenderer _renderer = new();
    private readonly GameArena _arena = new();
    private readonly Snake _sammy;
    private readonly Snake _jake;
    private readonly GameConfig _config;
    private readonly Random _random = new();

    private ColorPalette _colors = ColorPalette.ColorMode;
    private int _curLevel = 1;
    private int _number = 1;
    private bool _hasNumber = false;
    private int _numScreenRow0 = 0;
    private int _numCol0 = 0;
    private int _curDelayMs = 70;
    private bool _quitRequested = false;

    public NibblesGame(GameConfig? config = null)
    {
        _config = config ?? new GameConfig();
        _colors = _config.Colors;
        _sammy = new Snake("SAMMY", _colors.Snake1);
        _jake = new Snake("JAKE", _colors.Snake2);

        if (_config.StartMuted)
        {
            SoundEngine.IsMuted = true;
        }
    }

    public void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        _renderer.EnsureWindowSize();

        try
        {
            Console.CursorVisible = false;
        }
        catch { }

        if (!_config.QuickStart)
        {
            // 1. Classic Intro Screen with Sparkles and Music
            ShowIntro();

            // 2. Options and Settings Screen
            GetInputs();
        }

        // 3. Play Nibbles until player decides to quit
        bool playAgain = true;
        while (playAgain && !_quitRequested)
        {
            PlayNibbles();
            if (!_quitRequested && !_config.IsDemoMode)
            {
                playAgain = StillWantsToPlay();
            }
            else
            {
                playAgain = false;
            }
        }

        // Clean exit
        Console.ResetColor();
        Console.Clear();
        Console.WriteLine("Thank you for playing QBasic Nibbles!");
    }

    #region Intro & Menus

    private void ShowIntro()
    {
        _renderer.ClearBackBuffer(' ', ConsoleColor.White, ConsoleColor.Black);

        _renderer.WriteCentered(3, "Q B a s i c   N i b b l e s", ConsoleColor.White, ConsoleColor.Black);
        _renderer.WriteCentered(5, "Copyright (C) Microsoft Corporation 1990", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(7, "Nibbles is a game for one or two players.  Navigate your snakes", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(8, "around the game board trying to eat up numbers while avoiding", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(9, "running into walls or other snakes.  The more numbers you eat up,", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(10, "the more points you gain and the longer your snake becomes.", ConsoleColor.Gray, ConsoleColor.Black);

        _renderer.WriteCentered(12, "Game Controls", ConsoleColor.White, ConsoleColor.Black);
        _renderer.WriteCentered(14, " General             Player 1               Player 2      ", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(15, "                       (Up)                   (Up)        ", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(16, "P - Pause               ↑                      W          ", ConsoleColor.White, ConsoleColor.Black);
        _renderer.WriteCentered(17, "               (Left) ←   → (Right)   (Left) A   D (Right)", ConsoleColor.White, ConsoleColor.Black);
        _renderer.WriteCentered(18, "                        ↓                      S          ", ConsoleColor.White, ConsoleColor.Black);
        _renderer.WriteCentered(19, "                      (Down)                 (Down)       ", ConsoleColor.Gray, ConsoleColor.Black);
        _renderer.WriteCentered(21, "M - Toggle Sound  |  Esc - Exit", ConsoleColor.DarkGray, ConsoleColor.Black);
        _renderer.WriteCentered(23, "Press any key to continue", ConsoleColor.White, ConsoleColor.Black);

        _renderer.FullRepaint();

        // Play intro tune asynchronously in background
        SoundEngine.PlayAsync("MBT160O1L8CDEDCDL4ECC");

        // Sparkle border animation
        SparklePause();
    }

    private void SparklePause()
    {
        string sparklePattern = "*    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    *    ";
        int a = 0;

        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.M)
                {
                    SoundEngine.ToggleMute();
                }
                else
                {
                    break;
                }
            }

            // Top border (row 0)
            int topOffset = a % 5;
            string topRow = sparklePattern.Substring(topOffset, ConsoleRenderer.ScreenWidth);
            _renderer.WriteString(0, 0, topRow, ConsoleColor.Red, ConsoleColor.Black);

            // Bottom border (row 22)
            int botOffset = (5 - topOffset) % 5;
            string botRow = sparklePattern.Substring(botOffset, ConsoleRenderer.ScreenWidth);
            _renderer.WriteString(22, 0, botRow, ConsoleColor.Red, ConsoleColor.Black);

            // Vertical sparkles (rows 1..21)
            for (int b = 1; b <= 21; b++)
            {
                int c = (a + b) % 5;
                char ch = (c == 1) ? '*' : ' ';
                _renderer.SetCell(b, ConsoleRenderer.ScreenWidth - 1, ch, ConsoleColor.Red, ConsoleColor.Black);
                _renderer.SetCell(22 - b, 0, ch, ConsoleColor.Red, ConsoleColor.Black);
            }

            _renderer.Render();
            a = (a + 1) % 5;
            Thread.Sleep(50);
        }

        // Flush extra keys
        while (Console.KeyAvailable) Console.ReadKey(true);
    }

    private void GetInputs()
    {
        Console.ResetColor();
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("                   Q B a s i c   N i b b l e s   S e t u p");
        Console.WriteLine("                   =======================================");
        Console.WriteLine();

        // 1. Number of players
        Console.Write("                   How many players (1 or 2) [Default: 1]: ");
        string? numInput = Console.ReadLine()?.Trim();
        if (numInput == "2")
            _config.NumPlayers = 2;
        else
            _config.NumPlayers = 1;

        Console.WriteLine();
        Console.WriteLine("                   Skill level (1 to 100) [Default: 50]:");
        Console.WriteLine("                   1   = Novice");
        Console.WriteLine("                   90  = Expert");
        Console.WriteLine("                   100 = Twiddle Fingers");
        Console.WriteLine("                   (Computer speed may affect your skill level)");
        Console.Write("                   Enter skill level: ");
        string? speedInput = Console.ReadLine()?.Trim();
        if (int.TryParse(speedInput, out int sVal) && sVal >= 1 && sVal <= 100)
            _config.SkillLevel = sVal;
        else
            _config.SkillLevel = 50;

        Console.WriteLine();
        Console.Write("                   Increase game speed during play (Y or N) [Default: Y]: ");
        string? diffInput = Console.ReadLine()?.Trim().ToUpperInvariant();
        _config.IncreaseSpeed = (diffInput != "N");

        Console.WriteLine();
        Console.Write("                   Monochrome or color monitor (M or C) [Default: C]: ");
        string? monInput = Console.ReadLine()?.Trim().ToUpperInvariant();
        _config.IsColorMonitor = (monInput != "M");
        _config.Colors = _config.IsColorMonitor ? ColorPalette.ColorMode : ColorPalette.MonochromeMode;
        _colors = _config.Colors;

        _sammy.Color = _colors.Snake1;
        _jake.Color = _colors.Snake2;
    }

    #endregion

    #region Main Gameplay

    private void PlayNibbles()
    {
        _sammy.Lives = 5;
        _sammy.Score = 0;
        _jake.Lives = 5;
        _jake.Score = 0;

        _curLevel = 1;
        _curDelayMs = _config.CalculateInitialDelayMs();

        bool showLevelPause = true;

        // Play rounds until a player runs out of lives
        while (!_quitRequested && _sammy.Lives > 0 && (_config.NumPlayers == 1 || _jake.Lives > 0))
        {
            // Initial setup for current level
            StartLevel(STARTOVER: false);

            if (showLevelPause)
            {
                SpacePause($"     Level {_curLevel},  Push Space");
                if (_quitRequested) return;
            }

            // Play the round until either player dies or level is won
            bool levelWon = PlayRound();

            if (_quitRequested) return;

            if (levelWon)
            {
                _curLevel++;
                if (_config.IncreaseSpeed)
                {
                    _curDelayMs = Math.Max(20, _curDelayMs - 8);
                }
                showLevelPause = true;
            }
            else
            {
                // Player died. They already pressed space on "Sammy Dies! Push Space!",
                // so restart the same level immediately without redundant pause prompt.
                showLevelPause = false;
            }
        }
    }

    private void StartLevel(bool STARTOVER)
    {
        LevelManager.InitializeLevel(_curLevel, _arena, _sammy, _jake, _colors);

        if (_config.NumPlayers == 1)
        {
            _jake.Row = 0;
            _jake.Alive = false;
        }

        // Draw initial heads into arena
        _arena[_sammy.Row, _sammy.Col] = _sammy.Color;
        _sammy.Head = 0;
        _sammy.Body[0] = new BodySegment(_sammy.Row, _sammy.Col);

        if (_config.NumPlayers == 2)
        {
            _arena[_jake.Row, _jake.Col] = _jake.Color;
            _jake.Head = 0;
            _jake.Body[0] = new BodySegment(_jake.Row, _jake.Col);
        }

        _hasNumber = false;
        _renderer.DrawScores(_config.NumPlayers, _sammy, _jake, _colors);

        if (_config.IsDemoMode)
        {
            _renderer.WriteString(0, 31, "[DEMO AUTO-PILOT]", ConsoleColor.Cyan, _colors.Background);
        }

        _renderer.SyncEntireArena(_arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
        _renderer.FullRepaint();
    }

    /// <summary>
    /// Plays one round of the current level until a player dies (returns false) or beats the level (returns true).
    /// </summary>
    private bool PlayRound()
    {
        _number = 1;
        _hasNumber = false;
        bool playerDied = false;
        bool levelCompleted = false;

        _renderer.DrawScores(_config.NumPlayers, _sammy, _jake, _colors);
        if (_config.IsDemoMode)
        {
            _renderer.WriteString(0, 31, "[DEMO AUTO-PILOT]", ConsoleColor.Cyan, _colors.Background);
        }
        _renderer.Render();

        // Round start tune
        SoundEngine.PlayAsync("T160O1>L20CDEDCDL10ECC");

        while (!playerDied && !levelCompleted && !_quitRequested)
        {
            // Spawn number if none exists
            if (!_hasNumber)
            {
                SpawnNumber();
            }

            // Render current frame
            _renderer.Render();

            // Wait frame delay
            Thread.Sleep(_curDelayMs);

            // Handle keyboard input (or AI in demo mode)
            ProcessInput();

            if (_config.IsDemoMode)
            {
                // Autonomous AI movement
                var nextDir = NibblesAI.DetermineNextMove(_sammy, _arena, _colors, _numScreenRow0, _numCol0, _config.NumPlayers == 2 ? _jake : null);
                _sammy.SetDirection(nextDir);

                if (_config.NumPlayers == 2 && _jake.Alive)
                {
                    var jakeDir = NibblesAI.DetermineNextMove(_jake, _arena, _colors, _numScreenRow0, _numCol0, _sammy);
                    _jake.SetDirection(jakeDir);
                }
            }

            if (_quitRequested) return false;

            // Apply queued directions
            _sammy.ApplyQueuedDirection();
            if (_config.NumPlayers == 2 && _jake.Alive)
            {
                _jake.ApplyQueuedDirection();
            }

            // Move Snake 1 (Sammy)
            MoveSnakeHead(_sammy);

            // Move Snake 2 (Jake)
            if (_config.NumPlayers == 2 && _jake.Alive)
            {
                MoveSnakeHead(_jake);
            }

            // Check number eating for both snakes
            CheckNumberEaten(_sammy, ref levelCompleted);
            if (!levelCompleted && _config.NumPlayers == 2 && _jake.Alive)
            {
                CheckNumberEaten(_jake, ref levelCompleted);
            }

            // If level was won, exit the round immediately before collision checks
            if (levelCompleted) break;

            // Check collisions for Sammy
            CheckSnakeCollision(_sammy, _jake, ref playerDied);

            // Check collisions for Jake
            if (_config.NumPlayers == 2 && _jake.Alive)
            {
                CheckSnakeCollision(_jake, _sammy, ref playerDied);
            }
        }

        // When a player died
        if (playerDied && !_quitRequested)
        {
            HandlePlayerDeath();
            return false;
        }

        // When level was won (numbers 1-9 eaten)
        if (levelCompleted && !_quitRequested)
        {
            EraseSnakeWithAnimation(_sammy);
            if (_config.NumPlayers == 2)
            {
                EraseSnakeWithAnimation(_jake);
            }
            return true;
        }

        return false;
    }

    private void SpawnNumber()
    {
        int row, col, sisterRow;
        int attempts = 0;
        do
        {
            row = _random.Next(3, 50);  // 3 to 49
            col = _random.Next(2, 80);  // 2 to 79
            sisterRow = GameArena.GetSisterRow(row);
            attempts++;
        }
        while (attempts < 500 && (_arena.PointIsThere(row, col, _colors.Background) || _arena.PointIsThere(sisterRow, col, _colors.Background)));

        _numScreenRow0 = GameArena.GetRealRow(row) - 1; // 0-based
        _numCol0 = col - 1;                             // 0-based
        _hasNumber = true;

        _renderer.SyncArenaCell(_numScreenRow0, _numCol0, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
    }

    private void MoveSnakeHead(Snake snake)
    {
        switch (snake.Direction)
        {
            case Direction.Up:    snake.Row--; break;
            case Direction.Down:  snake.Row++; break;
            case Direction.Left:  snake.Col--; break;
            case Direction.Right: snake.Col++; break;
        }
    }

    private void CheckNumberEaten(Snake snake, ref bool levelCompleted)
    {
        if (!_hasNumber) return;

        int snakeScreenRow0 = GameArena.GetRealRow(snake.Row) - 1;
        int snakeCol0 = snake.Col - 1;

        if (snakeScreenRow0 == _numScreenRow0 && snakeCol0 == _numCol0)
        {
            // Eat sound
            SoundEngine.PlayAsync("MBO0L16>CCCE");

            if (snake.Length < (Snake.MaxSnakeLength - 30))
            {
                snake.Length += _number * 4;
            }

            snake.Score += _number;
            _renderer.DrawScores(_config.NumPlayers, _sammy, _jake, _colors);
            if (_config.IsDemoMode)
            {
                _renderer.WriteString(0, 31, "[DEMO AUTO-PILOT]", ConsoleColor.Cyan, _colors.Background);
            }

            _number++;
            _hasNumber = false;

            // Clear number text cell from screen
            _renderer.SyncArenaCell(_numScreenRow0, _numCol0, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);

            // Completed all 9 numbers -> Flag level completed to advance cleanly!
            if (_number == 10)
            {
                levelCompleted = true;
            }
        }
    }

    private void CheckSnakeCollision(Snake snake, Snake other, ref bool playerDied)
    {
        // Hit wall or body, or head-to-head collision
        bool headCollision = (_config.NumPlayers == 2 && other.Alive && snake.Row == other.Row && snake.Col == other.Col);
        bool hitObstacle = _arena.PointIsThere(snake.Row, snake.Col, _colors.Background);

        if (hitObstacle || headCollision)
        {
            // Play death crash sound
            SoundEngine.PlayAsync("MBO0L32EFGEFDC");

            // Erase number if present
            if (_hasNumber)
            {
                _hasNumber = false;
                _renderer.SyncArenaCell(_numScreenRow0, _numCol0, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
            }

            playerDied = true;
            snake.Alive = false;
            snake.Lives--;
        }
        else
        {
            // Advance body
            snake.Head = (snake.Head + 1) % Snake.MaxSnakeLength;
            snake.Body[snake.Head] = new BodySegment(snake.Row, snake.Col);

            int tail = (snake.Head + Snake.MaxSnakeLength - snake.Length) % Snake.MaxSnakeLength;
            var tailSeg = snake.Body[tail];

            // Erase tail pixel
            if (tailSeg.Row != 0)
            {
                _arena.SetPixel(tailSeg.Row, tailSeg.Col, _colors.Background);
                _renderer.SyncArenaCell(GameArena.GetRealRow(tailSeg.Row) - 1, tailSeg.Col - 1, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
                snake.Body[tail] = new BodySegment(0, 0);
            }

            // Draw new head pixel
            _arena.SetPixel(snake.Row, snake.Col, snake.Color);
            _renderer.SyncArenaCell(GameArena.GetRealRow(snake.Row) - 1, snake.Col - 1, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
        }
    }

    private void HandlePlayerDeath()
    {
        // Reset delay to initial speed for current level
        _curDelayMs = _config.CalculateInitialDelayMs();
        if (_config.IncreaseSpeed)
        {
            _curDelayMs = Math.Max(20, _curDelayMs - (_curLevel - 1) * 8);
        }

        // Dissolving snake animation
        EraseSnakeWithAnimation(_sammy);
        if (_config.NumPlayers == 2)
        {
            EraseSnakeWithAnimation(_jake);
        }

        // Deduct 10 points and show Death dialog
        if (!_sammy.Alive)
        {
            _sammy.Score = Math.Max(0, _sammy.Score - 10);
            _renderer.DrawScores(_config.NumPlayers, _sammy, _jake, _colors);
            SpacePause(" Sammy Dies! Push Space! --->");
        }
        else if (_config.NumPlayers == 2 && !_jake.Alive)
        {
            _jake.Score = Math.Max(0, _jake.Score - 10);
            _renderer.DrawScores(_config.NumPlayers, _sammy, _jake, _colors);
            SpacePause(" <---- Jake Dies! Push Space ");
        }
    }

    /// <summary>
    /// Classic QBasic 10-pass interlaced snake erase animation.
    /// </summary>
    private void EraseSnakeWithAnimation(Snake snake)
    {
        for (int c = 0; c <= 9; c++)
        {
            for (int b = snake.Length - c; b >= 0; b -= 10)
            {
                int tail = (snake.Head + Snake.MaxSnakeLength - b) % Snake.MaxSnakeLength;
                var seg = snake.Body[tail];
                if (seg.Row != 0)
                {
                    _arena.SetPixel(seg.Row, seg.Col, _colors.Background);
                    _renderer.SyncArenaCell(GameArena.GetRealRow(seg.Row) - 1, seg.Col - 1, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
                }
            }
            _renderer.Render();
            Thread.Sleep(15);
        }
    }

    private void ProcessInput()
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                // Player 1 (Sammy) - Arrow keys (in human mode)
                case ConsoleKey.UpArrow:    if (!_config.IsDemoMode) _sammy.SetDirection(Direction.Up); break;
                case ConsoleKey.DownArrow:  if (!_config.IsDemoMode) _sammy.SetDirection(Direction.Down); break;
                case ConsoleKey.LeftArrow:  if (!_config.IsDemoMode) _sammy.SetDirection(Direction.Left); break;
                case ConsoleKey.RightArrow: if (!_config.IsDemoMode) _sammy.SetDirection(Direction.Right); break;

                // Player 2 (Jake) - W/A/S/D
                case ConsoleKey.W:          if (!_config.IsDemoMode) _jake.SetDirection(Direction.Up); break;
                case ConsoleKey.S:          if (!_config.IsDemoMode) _jake.SetDirection(Direction.Down); break;
                case ConsoleKey.A:          if (!_config.IsDemoMode) _jake.SetDirection(Direction.Left); break;
                case ConsoleKey.D:          if (!_config.IsDemoMode) _jake.SetDirection(Direction.Right); break;

                // Pause
                case ConsoleKey.P:
                    SpacePause(" Game Paused ... Push Space  ");
                    break;

                // Mute toggle
                case ConsoleKey.M:
                    SoundEngine.ToggleMute();
                    break;

                // Quit
                case ConsoleKey.Escape:
                    _quitRequested = true;
                    return;
            }
        }
    }

    private void SpacePause(string text)
    {
        const int centerRow0 = 11;

        _renderer.DrawDialog(centerRow0, text, _colors);
        _renderer.Render();

        // Clear existing keys
        while (Console.KeyAvailable) Console.ReadKey(true);

        DateTime pauseStart = DateTime.UtcNow;

        while (true)
        {
            // In demo mode, automatically proceed after 1.2 seconds if no key is pressed
            if (_config.IsDemoMode && (DateTime.UtcNow - pauseStart).TotalMilliseconds > 1200)
            {
                break;
            }

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Spacebar)
                {
                    break;
                }
                if (key.Key == ConsoleKey.M)
                {
                    SoundEngine.ToggleMute();
                }
                if (key.Key == ConsoleKey.Escape)
                {
                    _quitRequested = true;
                    break;
                }
                if (_config.IsDemoMode)
                {
                    // Any key in demo mode exits or proceeds
                    _quitRequested = true;
                    break;
                }
            }

            Thread.Sleep(20);
        }

        // Restore arena underneath the dialog
        _renderer.RestoreDialogArea(centerRow0 - 1, centerRow0 + 1, _arena, _colors, _hasNumber, _numScreenRow0, _numCol0, _number);
        _renderer.Render();
    }

    private bool StillWantsToPlay()
    {
        const int centerRow0 = 11;

        _renderer.DrawGameOverDialog(centerRow0, _colors);
        _renderer.Render();

        while (Console.KeyAvailable) Console.ReadKey(true);

        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Y)
            {
                return true;
            }
            if (key.Key == ConsoleKey.N || key.Key == ConsoleKey.Escape)
            {
                return false;
            }
            if (key.Key == ConsoleKey.M)
            {
                SoundEngine.ToggleMute();
            }
        }
    }

    #endregion
}
