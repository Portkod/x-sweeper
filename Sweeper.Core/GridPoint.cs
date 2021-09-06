namespace Sweeper.Core;

public readonly record struct GridPoint(int Row, int Column)
{
    public static GridPoint operator +(GridPoint a, GridPoint b)
    {
        return new(a.Row + b.Row, a.Column + b.Column);
    }
    public static GridPoint operator +(GridPoint a, (int Row, int Column) b)
    {
        return new(a.Row + b.Row, a.Column + b.Column);
    }
    public static GridPoint operator -(GridPoint a, GridPoint b)
    {
        return new(a.Row - b.Row, a.Column - b.Column);
    }
    public static GridPoint operator -(GridPoint a, (int Row, int Column) b)
    {
        return new(a.Row - b.Row, a.Column - b.Column);
    }
}
