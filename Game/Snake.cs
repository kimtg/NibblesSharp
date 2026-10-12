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
    private readonly Queue<Direction> _inputQueue = new(2);

    public Direction QueuedDirection
    {
        get => _inputQueue.Count > 0 ? _inputQueue.Peek() : Direction;
        set
        {
            _inputQueue.Clear();
            if (value != Direction.None && value != Direction)
            {
                _inputQueue.Enqueue(value);
            }
        }
    }

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
        _inputQueue.Clear();
        Alive = true;
        Array.Clear(Body, 0, Body.Length);
    }

    public void SetDirection(Direction newDir)
    {
        if (newDir == Direction.None) return;

        // Check reversal against the last intended direction (either in queue or current direction)
        Direction referenceDir = _inputQueue.Count > 0 ? _inputQueue.Last() : Direction;

        if (IsOpposite(referenceDir, newDir) || referenceDir == newDir)
            return;

        // Buffer up to 2 direction changes for responsive cornering
        if (_inputQueue.Count < 2)
        {
            _inputQueue.Enqueue(newDir);
        }
    }

    public void ApplyQueuedDirection()
    {
        if (_inputQueue.TryDequeue(out var nextDir))
        {
            Direction = nextDir;
        }
    }

    public static bool IsOpposite(Direction d1, Direction d2) =>
        (d1 == Direction.Up && d2 == Direction.Down) ||
        (d1 == Direction.Down && d2 == Direction.Up) ||
        (d1 == Direction.Left && d2 == Direction.Right) ||
        (d1 == Direction.Right && d2 == Direction.Left);
}
