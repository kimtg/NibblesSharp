using System.Text;
using NibblesSharp.Game;

namespace NibblesSharp.Graphics;

public struct ScreenCell : IEquatable<ScreenCell>
{
    public char Character;
    public ConsoleColor Foreground;
    public ConsoleColor Background;

    public ScreenCell(char ch, ConsoleColor fg, ConsoleColor bg)
    {
        Character = ch;
        Foreground = fg;
        Background = bg;
    }

    public bool Equals(ScreenCell other) =>
        Character == other.Character &&
        Foreground == other.Foreground &&
        Background == other.Background;

    public override bool Equals(object? obj) => obj is ScreenCell other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Character, Foreground, Background);
    public static bool operator ==(ScreenCell left, ScreenCell right) => left.Equals(right);
    public static bool operator !=(ScreenCell left, ScreenCell right) => !left.Equals(right);
}

public class ConsoleRenderer
{
    public const int ScreenWidth = 80;
    public const int ScreenHeight = 25;

    private readonly ScreenCell[,] _backBuffer = new ScreenCell[ScreenHeight, ScreenWidth];
    private readonly ScreenCell[,] _frontBuffer = new ScreenCell[ScreenHeight, ScreenWidth];

    public ConsoleRenderer()
    {
        Console.OutputEncoding = Encoding.UTF8;
        try
        {
            Console.CursorVisible = false;
        }
        catch
        {
            // Ignore if cursor visibility can't be toggled in current host
        }

        ClearBackBuffer(' ', ConsoleColor.White, ConsoleColor.Black);
        ClearFrontBuffer();
    }

    public void EnsureWindowSize()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                if (Console.BufferWidth < ScreenWidth || Console.BufferHeight < ScreenHeight)
                {
                    Console.SetBufferSize(Math.Max(Console.BufferWidth, ScreenWidth), Math.Max(Console.BufferHeight, ScreenHeight));
                }
                if (Console.WindowWidth < ScreenWidth || Console.WindowHeight < ScreenHeight)
                {
                    Console.SetWindowSize(Math.Max(Console.WindowWidth, ScreenWidth), Math.Max(Console.WindowHeight, ScreenHeight));
                }
            }
        }
        catch
        {
            // Windows Terminal or third party shells may reject manual window resizing
        }
    }

    public void ClearBackBuffer(char ch, ConsoleColor fg, ConsoleColor bg)
    {
        for (int r = 0; r < ScreenHeight; r++)
        {
            for (int c = 0; c < ScreenWidth; c++)
            {
                _backBuffer[r, c] = new ScreenCell(ch, fg, bg);
            }
        }
    }

    public void ClearFrontBuffer()
    {
        for (int r = 0; r < ScreenHeight; r++)
        {
            for (int c = 0; c < ScreenWidth; c++)
            {
                // Force difference so next render completely repaints
                _frontBuffer[r, c] = new ScreenCell('\0', ConsoleColor.Black, ConsoleColor.Black);
            }
        }
    }

    public void SetCell(int row0, int col0, char ch, ConsoleColor fg, ConsoleColor bg)
    {
        if (row0 >= 0 && row0 < ScreenHeight && col0 >= 0 && col0 < ScreenWidth)
        {
            _backBuffer[row0, col0] = new ScreenCell(ch, fg, bg);
        }
    }

    public void WriteString(int row0, int col0, string text, ConsoleColor fg, ConsoleColor bg)
    {
        if (row0 < 0 || row0 >= ScreenHeight) return;

        for (int i = 0; i < text.Length; i++)
        {
            int c = col0 + i;
            if (c >= 0 && c < ScreenWidth)
            {
                _backBuffer[row0, c] = new ScreenCell(text[i], fg, bg);
            }
        }
    }

    public void WriteCentered(int row0, string text, ConsoleColor fg, ConsoleColor bg)
    {
        int col = Math.Max(0, (ScreenWidth - text.Length) / 2);
        WriteString(row0, col, text, fg, bg);
    }

    /// <summary>
    /// Updates the header line (Screen Row 0, arena rows 1 & 2) with scores and lives.
    /// </summary>
    public void DrawScores(int numPlayers, Snake sammy, Snake jake, ColorPalette colors)
    {
        // Clear row 0 with background color
        for (int c = 0; c < ScreenWidth; c++)
        {
            _backBuffer[0, c] = new ScreenCell(' ', ConsoleColor.White, colors.Background);
        }

        // Sammy on right: "SAMMY-->  Lives: #     #,###,#00" (starts at col 49, 1-based -> col 48, 0-based)
        string sammyScore = sammy.Score.ToString("#,##0").PadLeft(9);
        string sammyText = $"SAMMY-->  Lives: {sammy.Lives}     {sammyScore}";
        WriteString(0, 48, sammyText, ConsoleColor.White, colors.Background);

        // Jake on left: "#,###,#00  Lives: #  <--JAKE" (if 2 players)
        if (numPlayers == 2)
        {
            string jakeScore = jake.Score.ToString("#,##0").PadLeft(9);
            string jakeText = $"{jakeScore}  Lives: {jake.Lives}  <--JAKE";
            WriteString(0, 0, jakeText, ConsoleColor.White, colors.Background);
        }
    }

    /// <summary>
    /// Synchronizes a specific arena column and text row with the backbuffer.
    /// </summary>
    public void SyncArenaCell(int screenRow0, int col0, GameArena arena, ColorPalette colors, bool hasNumber, int numScreenRow0, int numCol0, int numVal)
    {
        if (screenRow0 < 1 || screenRow0 >= ScreenHeight || col0 < 0 || col0 >= ScreenWidth)
            return;

        int arenaCol = col0 + 1; // 1-based arena col
        int topArenaRow = screenRow0 * 2 + 1; // e.g. screenRow0=1 -> arenaRow 3
        int botArenaRow = screenRow0 * 2 + 2; // e.g. screenRow0=1 -> arenaRow 4

        // If number is placed on this screen cell
        if (hasNumber && screenRow0 == numScreenRow0 && col0 == numCol0)
        {
            _backBuffer[screenRow0, col0] = new ScreenCell(
                (char)('0' + numVal),
                colors.Snake1,
                colors.Background
            );
            return;
        }

        ConsoleColor topColor = arena[topArenaRow, arenaCol];
        ConsoleColor botColor = arena[botArenaRow, arenaCol];

        if (topColor == botColor)
        {
            if (topColor == colors.Background)
            {
                _backBuffer[screenRow0, col0] = new ScreenCell(' ', topColor, topColor);
            }
            else
            {
                // Full block
                _backBuffer[screenRow0, col0] = new ScreenCell('█', topColor, topColor);
            }
        }
        else
        {
            // Upper half block: top half is foreground, bottom half is background
            _backBuffer[screenRow0, col0] = new ScreenCell('▀', topColor, botColor);
        }
    }

    /// <summary>
    /// Synchronizes the entire arena (rows 1..24, cols 0..79) into the backbuffer.
    /// </summary>
    public void SyncEntireArena(GameArena arena, ColorPalette colors, bool hasNumber, int numScreenRow0, int numCol0, int numVal)
    {
        for (int screenRow0 = 1; screenRow0 < ScreenHeight; screenRow0++)
        {
            for (int col0 = 0; col0 < ScreenWidth; col0++)
            {
                SyncArenaCell(screenRow0, col0, arena, colors, hasNumber, numScreenRow0, numCol0, numVal);
            }
        }
    }

    /// <summary>
    /// Draws a classic QBasic double-lined red dialog box centered on screen.
    /// </summary>
    public void DrawDialog(int centerRow0, string message, ColorPalette colors)
    {
        string paddedMsg = message.PadRight(29);
        if (paddedMsg.Length > 29) paddedMsg = paddedMsg[..29];

        string top    = "╔═════════════════════════════╗";
        string middle = "║" + paddedMsg + "║";
        string bottom = "╚═════════════════════════════╝";

        WriteCentered(centerRow0 - 1, top, colors.DialogFore, colors.DialogBack);
        WriteCentered(centerRow0, middle, colors.DialogFore, colors.DialogBack);
        WriteCentered(centerRow0 + 1, bottom, colors.DialogFore, colors.DialogBack);
    }

    /// <summary>
    /// Draws the Game Over dialog box.
    /// </summary>
    public void DrawGameOverDialog(int centerRow0, ColorPalette colors)
    {
        string top    = "╔═════════════════════════════════╗";
        string line1  = "║      G A M E   O V E R          ║";
        string line2  = "║                                 ║";
        string line3  = "║     Play Again?   (Y/N)         ║";
        string bottom = "╚═════════════════════════════════╝";

        WriteCentered(centerRow0 - 2, top, colors.DialogFore, colors.DialogBack);
        WriteCentered(centerRow0 - 1, line1, colors.DialogFore, colors.DialogBack);
        WriteCentered(centerRow0, line2, colors.DialogFore, colors.DialogBack);
        WriteCentered(centerRow0 + 1, line3, colors.DialogFore, colors.DialogBack);
        WriteCentered(centerRow0 + 2, bottom, colors.DialogFore, colors.DialogBack);
    }

    /// <summary>
    /// Restores the arena under the dialog box area (rows 10..14).
    /// </summary>
    public void RestoreDialogArea(int startRow0, int endRow0, GameArena arena, ColorPalette colors, bool hasNumber, int numScreenRow0, int numCol0, int numVal)
    {
        for (int r = startRow0; r <= endRow0; r++)
        {
            for (int c = 0; c < ScreenWidth; c++)
            {
                SyncArenaCell(r, c, arena, colors, hasNumber, numScreenRow0, numCol0, numVal);
            }
        }
    }

    /// <summary>
    /// Renders changed cells from backbuffer to the console with minimal draw calls and 0 flicker.
    /// </summary>
    public void Render()
    {
        var sb = new StringBuilder();
        ConsoleColor currentFg = ConsoleColor.White;
        ConsoleColor currentBg = ConsoleColor.Black;

        for (int r = 0; r < ScreenHeight; r++)
        {
            int c = 0;
            while (c < ScreenWidth)
            {
                if (_backBuffer[r, c] != _frontBuffer[r, c])
                {
                    // Find run of changed cells with same colors
                    int startCol = c;
                    var cell = _backBuffer[r, c];
                    currentFg = cell.Foreground;
                    currentBg = cell.Background;

                    sb.Clear();
                    while (c < ScreenWidth && 
                           _backBuffer[r, c] != _frontBuffer[r, c] && 
                           _backBuffer[r, c].Foreground == currentFg && 
                           _backBuffer[r, c].Background == currentBg)
                    {
                        sb.Append(_backBuffer[r, c].Character);
                        _frontBuffer[r, c] = _backBuffer[r, c];
                        c++;
                    }

                    try
                    {
                        Console.SetCursorPosition(startCol, r);
                        Console.ForegroundColor = currentFg;
                        Console.BackgroundColor = currentBg;
                        Console.Write(sb.ToString());
                    }
                    catch
                    {
                        // Ignore buffer bounds edge cases if terminal resized
                    }
                }
                else
                {
                    c++;
                }
            }
        }

        // Park cursor off to the bottom right
        try
        {
            Console.SetCursorPosition(ScreenWidth - 1, ScreenHeight - 1);
        }
        catch { }
    }

    public void FullRepaint()
    {
        ClearFrontBuffer();
        Render();
    }
}
