namespace NibblesSharp.Game;

/// <summary>
/// Manages the 50x80 subpixel arena grid and collision detection.
/// </summary>
public class GameArena
{
    public const int Rows = 50;
    public const int Cols = 80;

    // 1-based indexing for exact compatibility with QBasic Nibbles formulas:
    // row 1..50, col 1..80
    private readonly ConsoleColor[,] _grid = new ConsoleColor[Rows + 1, Cols + 1];

    public ConsoleColor this[int row, int col]
    {
        get => (row >= 1 && row <= Rows && col >= 1 && col <= Cols) ? _grid[row, col] : ConsoleColor.Black;
        set
        {
            if (row >= 1 && row <= Rows && col >= 1 && col <= Cols)
            {
                _grid[row, col] = value;
            }
        }
    }

    public void Clear(ConsoleColor backgroundColor)
    {
        for (int r = 1; r <= Rows; r++)
        {
            for (int c = 1; c <= Cols; c++)
            {
                _grid[r, c] = backgroundColor;
            }
        }
    }

    /// <summary>
    /// Builds the standard outer borders of the playing field:
    /// Top border: Arena row 3
    /// Bottom border: Arena row 50
    /// Left border: Col 1, rows 4..49
    /// Right border: Col 80, rows 4..49
    /// </summary>
    public void BuildOuterBorders(ConsoleColor wallColor)
    {
        for (int col = 1; col <= Cols; col++)
        {
            _grid[3, col] = wallColor;
            _grid[50, col] = wallColor;
        }

        for (int row = 4; row <= 49; row++)
        {
            _grid[row, 1] = wallColor;
            _grid[row, 80] = wallColor;
        }
    }

    public void SetPixel(int row, int col, ConsoleColor color)
    {
        if (row >= 1 && row <= Rows && col >= 1 && col <= Cols)
        {
            _grid[row, col] = color;
        }
    }

    /// <summary>
    /// Checks if a non-background pixel is present at (row, col).
    /// Equivalent to QBasic PointIsThere function.
    /// </summary>
    public bool PointIsThere(int row, int col, ConsoleColor backgroundColor)
    {
        if (row < 1 || row > Rows || col < 1 || col > Cols)
            return true; // Out of bounds is a wall collision

        return _grid[row, col] != backgroundColor;
    }

    public static int GetRealRow(int arenaRow) => (arenaRow + 1) / 2;

    public static int GetSisterRow(int arenaRow) => arenaRow + ((arenaRow % 2) * 2 - 1);
}
