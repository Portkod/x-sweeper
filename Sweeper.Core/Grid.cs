using System.Collections;

namespace Sweeper.Core;

public class Grid<T> : IEnumerable<(GridPoint Point, T Item)>, IEnumerable
{
    protected T[,] Items { get; }
    public int Rows { get; }
    public int Columns { get; }
    public int TotalItems { get; }

    public Grid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        TotalItems = rows * columns;
        Items = new T[rows, columns];
    }

    public T this[GridPoint point]
    {
        get { return Items[point.Row, point.Column]; }
        set { Items[point.Row, point.Column] = value; }
    }

    public int PointToIndex(GridPoint point) => point.Row * Columns + point.Column;

    public GridPoint IndexToPoint(int index) => new(index / Columns, index % Columns);

    public bool PointIsOutOfBounds(GridPoint point) => point.Row < 0 || point.Row >= Rows || point.Column < 0 || point.Column >= Columns;

    public IEnumerable<GridPoint> EnumeratePointNeighbors(GridPoint point)
    {
        for (int row = -1; row <= 1; row++)
        {
            for (int col = -1; col <= 1; col++)
            {
                var neighborPoint = point + (row, col);
                if (!PointIsOutOfBounds(neighborPoint))
                {
                    yield return neighborPoint;
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<(GridPoint Point, T Item)> GetEnumerator()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                yield return (new(row, col), Items[row, col]);
            }
        }
    }
}
