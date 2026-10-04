using NibblesSharp.Graphics;

namespace NibblesSharp.Game;

public static class LevelManager
{
    public static void InitializeLevel(int levelNumber, GameArena arena, Snake sammy, Snake jake, ColorPalette colors)
    {
        arena.Clear(colors.Background);
        arena.BuildOuterBorders(colors.Wall);

        sammy.Reset();
        jake.Reset();

        switch (levelNumber)
        {
            case 1:
                sammy.Row = 25; sammy.Col = 50; sammy.Direction = Direction.Right;
                jake.Row = 25;  jake.Col = 30;  jake.Direction = Direction.Left;
                break;

            case 2:
                for (int i = 20; i <= 60; i++)
                {
                    arena.SetPixel(25, i, colors.Wall);
                }
                sammy.Row = 7;  sammy.Col = 60; sammy.Direction = Direction.Left;
                jake.Row = 43;  jake.Col = 20;  jake.Direction = Direction.Right;
                break;

            case 3:
                for (int i = 10; i <= 40; i++)
                {
                    arena.SetPixel(i, 20, colors.Wall);
                    arena.SetPixel(i, 60, colors.Wall);
                }
                sammy.Row = 25; sammy.Col = 50; sammy.Direction = Direction.Up;
                jake.Row = 25;  jake.Col = 30;  jake.Direction = Direction.Down;
                break;

            case 4:
                for (int i = 4; i <= 30; i++)
                {
                    arena.SetPixel(i, 20, colors.Wall);
                    arena.SetPixel(53 - i, 60, colors.Wall);
                }
                for (int i = 2; i <= 40; i++)
                {
                    arena.SetPixel(38, i, colors.Wall);
                    arena.SetPixel(15, 81 - i, colors.Wall);
                }
                sammy.Row = 7;  sammy.Col = 60; sammy.Direction = Direction.Left;
                jake.Row = 43;  jake.Col = 20;  jake.Direction = Direction.Right;
                break;

            case 5:
                for (int i = 13; i <= 39; i++)
                {
                    arena.SetPixel(i, 21, colors.Wall);
                    arena.SetPixel(i, 59, colors.Wall);
                }
                for (int i = 23; i <= 57; i++)
                {
                    arena.SetPixel(11, i, colors.Wall);
                    arena.SetPixel(41, i, colors.Wall);
                }
                sammy.Row = 25; sammy.Col = 50; sammy.Direction = Direction.Up;
                jake.Row = 25;  jake.Col = 30;  jake.Direction = Direction.Down;
                break;

            case 6:
                for (int i = 4; i <= 49; i++)
                {
                    if (i > 30 || i < 23)
                    {
                        arena.SetPixel(i, 10, colors.Wall);
                        arena.SetPixel(i, 20, colors.Wall);
                        arena.SetPixel(i, 30, colors.Wall);
                        arena.SetPixel(i, 40, colors.Wall);
                        arena.SetPixel(i, 50, colors.Wall);
                        arena.SetPixel(i, 60, colors.Wall);
                        arena.SetPixel(i, 70, colors.Wall);
                    }
                }
                sammy.Row = 7;  sammy.Col = 65; sammy.Direction = Direction.Down;
                jake.Row = 43;  jake.Col = 15;  jake.Direction = Direction.Up;
                break;

            case 7:
                for (int i = 4; i <= 49; i += 2)
                {
                    arena.SetPixel(i, 40, colors.Wall);
                }
                sammy.Row = 7;  sammy.Col = 65; sammy.Direction = Direction.Down;
                jake.Row = 43;  jake.Col = 15;  jake.Direction = Direction.Up;
                break;

            case 8:
                for (int i = 4; i <= 40; i++)
                {
                    arena.SetPixel(i, 10, colors.Wall);
                    arena.SetPixel(53 - i, 20, colors.Wall);
                    arena.SetPixel(i, 30, colors.Wall);
                    arena.SetPixel(53 - i, 40, colors.Wall);
                    arena.SetPixel(i, 50, colors.Wall);
                    arena.SetPixel(53 - i, 60, colors.Wall);
                    arena.SetPixel(i, 70, colors.Wall);
                }
                sammy.Row = 7;  sammy.Col = 65; sammy.Direction = Direction.Down;
                jake.Row = 43;  jake.Col = 15;  jake.Direction = Direction.Up;
                break;

            case 9:
                for (int i = 6; i <= 47; i++)
                {
                    arena.SetPixel(i, i, colors.Wall);
                    arena.SetPixel(i, i + 28, colors.Wall);
                }
                sammy.Row = 40; sammy.Col = 75; sammy.Direction = Direction.Up;
                jake.Row = 15;  jake.Col = 5;   jake.Direction = Direction.Down;
                break;

            default: // CASE ELSE
                for (int i = 4; i <= 49; i += 2)
                {
                    arena.SetPixel(i, 10, colors.Wall);
                    arena.SetPixel(i + 1, 20, colors.Wall);
                    arena.SetPixel(i, 30, colors.Wall);
                    arena.SetPixel(i + 1, 40, colors.Wall);
                    arena.SetPixel(i, 50, colors.Wall);
                    arena.SetPixel(i + 1, 60, colors.Wall);
                    arena.SetPixel(i, 70, colors.Wall);
                }
                sammy.Row = 7;  sammy.Col = 65; sammy.Direction = Direction.Down;
                jake.Row = 43;  jake.Col = 15;  jake.Direction = Direction.Up;
                break;
        }

        sammy.QueuedDirection = sammy.Direction;
        jake.QueuedDirection = jake.Direction;
    }
}
