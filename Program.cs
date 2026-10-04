using NibblesSharp.Game;

namespace NibblesSharp;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "QBasic Nibbles (C# Edition)";

        try
        {
            var config = GameConfig.ParseArguments(args);
            var game = new NibblesGame(config);
            game.Run();
        }
        finally
        {
            Console.ResetColor();
            try
            {
                Console.CursorVisible = true;
            }
            catch { }
        }
    }
}
