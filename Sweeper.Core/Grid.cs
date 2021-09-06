using System.Collections;

namespace Sweeper.Core;

public class Grid<T> : IReadOnlyCollection<(GridPoint Point, T Cell)>
{
    protected T[,] Cells { get; }
    public int Rows { get; }
    public int Columns { get; }
    public int Count { get; }

    public bool IsReadOnly => true;

    public Grid(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        Count = rows * columns;
        Cells = new T[rows, columns];
    }

    public T this[GridPoint point]
    {
        get => Cells[point.Row, point.Column];
        set => Cells[point.Row, point.Column] = value;
    }

    public int PointToIndex(GridPoint point) => point.Row * Columns + point.Column;

    public GridPoint IndexToPoint(int index) => new(index / Columns, index % Columns);

    public bool PointIsOutOfBounds(GridPoint point) => point.Row < 0 || point.Row >= Rows || point.Column < 0 || point.Column >= Columns;

    public IEnumerable<GridPoint> EnumerateNeighbors(GridPoint point)
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

    public IEnumerator<(GridPoint Point, T Cell)> GetEnumerator()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                yield return (new(row, col), Cells[row, col]);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
