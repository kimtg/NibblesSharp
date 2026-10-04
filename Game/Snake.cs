namespace NibblesSharp.Game;

public enum Direction
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

public struct BodySegment
{
    public int Row;
    public int Col;

    public BodySegment(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

public class Snake
{
    public const int MaxSnakeLength = 1000;

    public string Name { get; set; }
    public int Head { get; set; }
    public int Length { get; set; }
    public int Row { get; set; }
    public int Col { get; set; }
    public Direction Direction { get; set; }
    public Direction QueuedDirection { get; set; }
    public int Lives { get; set; }
    public int Score { get; set; }
    public ConsoleColor Color { get; set; }
    public bool Alive { get; set; }
    public BodySegment[] Body { get; } = new BodySegment[MaxSnakeLength];

    public Snake(string name, ConsoleColor color)
    {
        Name = name;
        Color = color;
        Reset();
    }

    public void Reset()
    {
        Head = 0;
        Length = 2;
        Row = 0;
        Col = 0;
        Direction = Direction.None;
        QueuedDirection = Direction.None;
        Alive = true;
        Array.Clear(Body, 0, Body.Length);
    }

    public void SetDirection(Direction newDir)
    {
        // Prevent 180-degree immediate reversal into own neck
        if (Direction == Direction.Up && newDir == Direction.Down) return;
        if (Direction == Direction.Down && newDir == Direction.Up) return;
        if (Direction == Direction.Left && newDir == Direction.Right) return;
        if (Direction == Direction.Right && newDir == Direction.Left) return;

        QueuedDirection = newDir;
    }

    public void ApplyQueuedDirection()
    {
        if (QueuedDirection != Direction.None)
        {
            Direction = QueuedDirection;
        }
    }
}
