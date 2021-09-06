namespace Sweeper.Core;

public enum CellState
{
    Hidden,
    Revealed,
    Marked
}

public abstract class Cell
{
    public CellState State { get; internal set; }
}

public class TrappedCell : Cell { }
public class SafeCell : Cell
{
    public int TrappedNeighbors { get; internal set; }
}

//public class CellGrid : Grid<Cell>
//{
//    public CellGrid(int rows, int columns) : base(rows, columns) { }
//}
