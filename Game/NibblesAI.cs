using NibblesSharp.Graphics;

namespace NibblesSharp.Game;

/// <summary>
/// Autonomous AI pilot for demonstration mode and Player-vs-AI mode
/// using BFS pathfinding, trap lookahead, tail-following heuristics, and flood-fill survival.
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

        int requiredSpace = Math.Min(snake.Length + 4, 20);

        // 1. Try to find path to number that does NOT trap the snake
        var pathDir = FindPath(
            startRow, 
            startCol, 
            snake.Direction, 
            (r, c) => GameArena.GetRealRow(r) == targetRealRow && c == targetCol,
            arena, 
            colors, 
            otherSnake);

        if (pathDir != Direction.None)
        {
            var (dRow, dCol) = GetDelta(pathDir);
            int nextR = startRow + dRow;
            int nextC = startCol + dCol;

            // Verify the path doesn't immediately trap us in a confined space
            int space = CountAccessibleSpaces(nextR, nextC, arena, colors, otherSnake, maxDepth: 45);
            if (space >= requiredSpace)
            {
                return pathDir;
            }
        }

        // 2. Tail-following heuristic: if target is unreachable or trapped, steer towards tail
        int tail = (snake.Head + Snake.MaxSnakeLength - snake.Length) % Snake.MaxSnakeLength;
        var tailSeg = snake.Body[tail];
        if (tailSeg.Row != 0)
        {
            var tailDir = FindPath(
                startRow, 
                startCol, 
                snake.Direction, 
                (r, c) => Math.Abs(r - tailSeg.Row) + Math.Abs(c - tailSeg.Col) <= 1,
                arena, 
                colors, 
                otherSnake);

            if (tailDir != Direction.None)
            {
                var (dRow, dCol) = GetDelta(tailDir);
                int nextR = startRow + dRow;
                int nextC = startCol + dCol;
                int space = CountAccessibleSpaces(nextR, nextC, arena, colors, otherSnake, maxDepth: 35);
                if (space >= 8)
                {
                    return tailDir;
                }
            }
        }

        // 3. Fallback: Choose the safe move with the largest accessible open space (flood fill)
        Direction bestFallbackDir = Direction.None;
        int maxSpace = -1;

        foreach (var (dRow, dCol, dir) in Moves)
        {
            if (Snake.IsOpposite(snake.Direction, dir)) continue;

            int nRow = startRow + dRow;
            int nCol = startCol + dCol;

            if (IsSafe(nRow, nCol, arena, colors, otherSnake))
            {
                int space = CountAccessibleSpaces(nRow, nCol, arena, colors, otherSnake, maxDepth: 50);
                if (space > maxSpace)
                {
                    maxSpace = space;
                    bestFallbackDir = dir;
                }
            }
        }

        return (bestFallbackDir != Direction.None) ? bestFallbackDir : snake.Direction;
    }

    private static Direction FindPath(
        int startRow, 
        int startCol, 
        Direction currentDir,
        Func<int, int, bool> isTarget,
        GameArena arena, 
        ColorPalette colors, 
        Snake? otherSnake)
    {
        var queue = new Queue<(int row, int col, Direction firstDir)>();
        var visited = new bool[GameArena.Rows + 1, GameArena.Cols + 1];
        visited[startRow, startCol] = true;

        foreach (var (dRow, dCol, dir) in Moves)
        {
            if (Snake.IsOpposite(currentDir, dir)) continue;

            int nRow = startRow + dRow;
            int nCol = startCol + dCol;

            if (IsSafe(nRow, nCol, arena, colors, otherSnake))
            {
                visited[nRow, nCol] = true;
                if (isTarget(nRow, nCol))
                {
                    return dir;
                }
                queue.Enqueue((nRow, nCol, dir));
            }
        }

        while (queue.Count > 0)
        {
            var (curRow, curCol, firstDir) = queue.Dequeue();

            if (isTarget(curRow, curCol))
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

        return Direction.None;
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

    private static (int dRow, int dCol) GetDelta(Direction dir) => dir switch
    {
        Direction.Up => (-1, 0),
        Direction.Down => (1, 0),
        Direction.Left => (0, -1),
        Direction.Right => (0, 1),
        _ => (0, 0)
    };

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
