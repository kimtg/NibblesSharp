using NibblesSharp.Graphics;

namespace NibblesSharp.Game;

/// <summary>
/// Autonomous AI pilot for demonstration mode using BFS pathfinding and flood-fill survival heuristics.
/// </summary>
public static class NibblesAI
{
    private static readonly (int dRow, int dCol, Direction dir)[] Moves =
    {
        (-1, 0, Direction.Up),
        (1, 0, Direction.Down),
        (0, -1, Direction.Left),
        (0, 1, Direction.Right)
    };

    public static Direction DetermineNextMove(
        Snake snake, 
        GameArena arena, 
        ColorPalette colors, 
        int targetScreenRow0, 
        int targetCol0,
        Snake? otherSnake = null)
    {
        int startRow = snake.Row;
        int startCol = snake.Col;

        // Target arena coordinates: realRow matches (targetScreenRow0 + 1)
        int targetRealRow = targetScreenRow0 + 1;
        int targetCol = targetCol0 + 1;

        // BFS to find shortest path to target text cell
        var queue = new Queue<(int row, int col, Direction firstDir)>();
        var visited = new bool[GameArena.Rows + 1, GameArena.Cols + 1];
        visited[startRow, startCol] = true;

        foreach (var (dRow, dCol, dir) in Moves)
        {
            if (IsOpposite(snake.Direction, dir)) continue;

            int nRow = startRow + dRow;
            int nCol = startCol + dCol;

            if (IsSafe(nRow, nCol, arena, colors, otherSnake))
            {
                visited[nRow, nCol] = true;
                // If this immediate move hits the number
                if (GameArena.GetRealRow(nRow) == targetRealRow && nCol == targetCol)
                {
                    return dir;
                }
                queue.Enqueue((nRow, nCol, dir));
            }
        }

        while (queue.Count > 0)
        {
            var (curRow, curCol, firstDir) = queue.Dequeue();

            if (GameArena.GetRealRow(curRow) == targetRealRow && curCol == targetCol)
            {
                return firstDir;
            }

            foreach (var (dRow, dCol, _) in Moves)
            {
                int nRow = curRow + dRow;
                int nCol = curCol + dCol;

                if (nRow >= 1 && nRow <= GameArena.Rows && nCol >= 1 && nCol <= GameArena.Cols)
                {
                    if (!visited[nRow, nCol] && IsSafe(nRow, nCol, arena, colors, otherSnake))
                    {
                        visited[nRow, nCol] = true;
                        queue.Enqueue((nRow, nCol, firstDir));
                    }
                }
            }
        }

        // If no path to the target was found, pick the safe move with the most open space (flood fill)
        Direction bestFallbackDir = Direction.None;
        int maxSpace = -1;

        foreach (var (dRow, dCol, dir) in Moves)
        {
            if (IsOpposite(snake.Direction, dir)) continue;

            int nRow = startRow + dRow;
            int nCol = startCol + dCol;

            if (IsSafe(nRow, nCol, arena, colors, otherSnake))
            {
                int space = CountAccessibleSpaces(nRow, nCol, arena, colors, otherSnake, maxDepth: 40);
                if (space > maxSpace)
                {
                    maxSpace = space;
                    bestFallbackDir = dir;
                }
            }
        }

        return (bestFallbackDir != Direction.None) ? bestFallbackDir : snake.Direction;
    }

    private static bool IsSafe(int row, int col, GameArena arena, ColorPalette colors, Snake? otherSnake)
    {
        if (row < 1 || row > GameArena.Rows || col < 1 || col > GameArena.Cols)
            return false;

        if (arena.PointIsThere(row, col, colors.Background))
            return false;

        if (otherSnake != null && otherSnake.Alive && otherSnake.Row == row && otherSnake.Col == col)
            return false;

        return true;
    }

    private static bool IsOpposite(Direction d1, Direction d2)
    {
        return (d1 == Direction.Up && d2 == Direction.Down) ||
               (d1 == Direction.Down && d2 == Direction.Up) ||
               (d1 == Direction.Left && d2 == Direction.Right) ||
               (d1 == Direction.Right && d2 == Direction.Left);
    }

    private static int CountAccessibleSpaces(int startRow, int startCol, GameArena arena, ColorPalette colors, Snake? otherSnake, int maxDepth)
    {
        var visited = new bool[GameArena.Rows + 1, GameArena.Cols + 1];
        var queue = new Queue<(int row, int col)>();
        visited[startRow, startCol] = true;
        queue.Enqueue((startRow, startCol));
        int count = 0;

        while (queue.Count > 0 && count < maxDepth)
        {
            var (r, c) = queue.Dequeue();
            count++;

            foreach (var (dRow, dCol, _) in Moves)
            {
                int nr = r + dRow;
                int nc = c + dCol;

                if (nr >= 1 && nr <= GameArena.Rows && nc >= 1 && nc <= GameArena.Cols)
                {
                    if (!visited[nr, nc] && IsSafe(nr, nc, arena, colors, otherSnake))
                    {
                        visited[nr, nc] = true;
                        queue.Enqueue((nr, nc));
                    }
                }
            }
        }

        return count;
    }
}
