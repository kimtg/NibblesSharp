namespace NibblesSharp.Graphics;

public class ColorPalette
{
    public ConsoleColor Snake1 { get; init; }
    public ConsoleColor Snake2 { get; init; }
    public ConsoleColor Wall { get; init; }
    public ConsoleColor Background { get; init; }
    public ConsoleColor DialogFore { get; init; }
    public ConsoleColor DialogBack { get; init; }

    public static ColorPalette ColorMode => new()
    {
        Snake1 = ConsoleColor.Yellow,       // 14
        Snake2 = ConsoleColor.Magenta,      // 13 (Light Magenta)
        Wall = ConsoleColor.Red,            // 12 (Light Red)
        Background = ConsoleColor.DarkBlue, // 1 (Classic QBasic Blue)
        DialogFore = ConsoleColor.White,    // 15 (Bright White)
        DialogBack = ConsoleColor.DarkRed   // 4 (Red dialog box)
    };

    public static ColorPalette MonochromeMode => new()
    {
        Snake1 = ConsoleColor.White,        // 15
        Snake2 = ConsoleColor.Gray,         // 7
        Wall = ConsoleColor.Gray,           // 7
        Background = ConsoleColor.Black,    // 0
        DialogFore = ConsoleColor.White,    // 15
        DialogBack = ConsoleColor.Black     // 0
    };
}
