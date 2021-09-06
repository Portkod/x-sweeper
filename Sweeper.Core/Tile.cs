namespace Sweeper.Core;

public enum TileState
{
    Hidden,
    Marked,
    Revealed
}

public abstract class Tile
{
    public TileState State { get; internal set; }
}

public class TrappedTile : Tile { }
public class SafeTile : Tile
{
    public int TrappedNeighbors { get; internal set; }
}

//public class TileGrid : Grid<Tile>
//{
//    public TileGrid(int rows, int columns) : base(rows, columns) { }
//}
