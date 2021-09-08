namespace Sweeper.Core;

public enum GameState
{
    New,
    InProgress,
    Defeat,
    Victory
}

public readonly record struct GameStateUpdatedArgs(GameState PreviousState, GameState State);
public readonly record struct GridUpdatedArgs();
public class SweeperGame
{
    public event EventHandler<GameStateUpdatedArgs>? GameStateUpdated;
    public event EventHandler<GridUpdatedArgs>? GridUpdated;
    /*
     * 9 x 9, 10 traps
     * 16 x 16, 40 traps
     * 24 x 24, 99 traps
     */
    public int TotalTraps { get; }
    public int RemainingTraps { get; private set; }
    protected Grid<Tile> Grid { get; }
    public int Rows => Grid.Rows;
    public int Columns => Grid.Columns;
    private GameState _state = GameState.New;
    public GameState State
    {
        get => _state;
        protected set
        {
            if (_state != value)
            {
                var previousState = _state;
                _state = value;
                OnGameStateUpdated(new(previousState, _state));
            }
        }
    }

    protected int RevealedSafeTilesCount { get; set; }
    protected bool AllSafeTilesRevealed => RevealedSafeTilesCount == Grid.Count - TotalTraps;

    public SweeperGame(int rows, int columns, int totalTraps)
    {
        Grid = new(rows, columns);
        TotalTraps = totalTraps;
        RemainingTraps = TotalTraps;
    }

    protected virtual void OnGameStateUpdated(GameStateUpdatedArgs e)
    {
        GameStateUpdated?.Invoke(this, e);
    }
    protected virtual void OnGridUpdated(GridUpdatedArgs e)
    {
        GridUpdated?.Invoke(this, e);
    }

    public Tile GetTileAt(GridPoint point) => Grid[point];

    protected void PlaceTraps(GridPoint safePoint)
    {
        var remainingTrapsToPlace = TotalTraps;
        while (remainingTrapsToPlace > 0)
        {
            var randomIndex = Random.Shared.Next(Grid.Count);
            var trapPoint = Grid.IndexToPoint(randomIndex);
            if (trapPoint == safePoint)
            {
                continue;
            }

            if (Grid[trapPoint] == null)
            {
                CreateTrappedTileAtPoint(trapPoint);
                remainingTrapsToPlace--;
            }
        }
    }

    protected void RevealEntireGrid()
    {
        foreach (var item in Grid)
        {
            var tile = item.Cell;
            if (tile == null)
            {
                tile = CreateSafeTileAtPoint(item.Point);
            }
            tile.State = TileState.Revealed;
            RevealedSafeTilesCount++;
        }
        OnGridUpdated(new());
    }

    protected void UpdateGameState(GameState newState)
    {
        switch (newState)
        {
            case GameState.New:
                break;
            case GameState.InProgress:
                break;
            case GameState.Defeat:
                RevealEntireGrid();
                break;
            case GameState.Victory:
                RevealEntireGrid();
                break;
            default:
                break;
        }
        State = newState;
    }

    protected virtual void InitNewGame(GridPoint startPoint)
    {
        PlaceTraps(startPoint);
        UpdateGameState(GameState.InProgress);
    }

    protected Tile CreateTrappedTileAtPoint(GridPoint point, TileState state = TileState.Hidden)
    {
        var tile = new TrappedTile { State = state };
        Grid[point] = tile;

        return tile;
    }
    protected Tile CreateSafeTileAtPoint(GridPoint point, TileState state = TileState.Hidden)
    {
        var tile = new SafeTile { State = state, TrappedNeighbors = CountAdjacentTraps(point) };
        Grid[point] = tile;

        return tile;
    }

    protected void RevealNeighborsToPoint(GridPoint point)
    {
        foreach (var neighborPoint in Grid.EnumerateNeighbors(point))
        {
            RevealTile(neighborPoint, false);
        }
    }

    public void MarkTile(GridPoint point)
    {
        if (State == GameState.New)
        {
            return;
        }

        if (Grid.PointIsOutOfBounds(point))
        {
            // TODO Invalid index. Throw?
            return;
        }

        Tile tile = Grid[point];
        if (tile == null)
        {
            tile = CreateSafeTileAtPoint(point);
        }

        switch (tile.State)
        {
            case TileState.Hidden:
                tile.State = TileState.Marked;
                break;
            case TileState.Marked:
                tile.State = TileState.Hidden;
                break;
        }
        OnGridUpdated(new());
    }

    public void RevealTile(GridPoint point) => RevealTile(point, true);

    protected void RevealTile(GridPoint point, bool invokeUpdate)
    {
        if (Grid.PointIsOutOfBounds(point))
        {
            // TODO Invalid index. Throw?
            return;
        }

        if (State == GameState.New)
        {
            InitNewGame(point);
        }
        else if (State != GameState.InProgress)
        {
            return;
        }

        Tile tile = Grid[point];
        if (tile == null)
        {
            tile = CreateSafeTileAtPoint(point);
        }
        else if (tile.State == TileState.Revealed || tile.State == TileState.Marked)
        {
            return;
        }

        switch (tile)
        {
            case SafeTile safeTile:
                RevealSafeTile(safeTile, point);
                break;
            case TrappedTile trappedTile:
                RevealTrappedTile(trappedTile);
                break;
            default:
                throw new Exception("Unsupported tile type.");
        }

        if (invokeUpdate)
        {
            OnGridUpdated(new());
        }
        if (AllSafeTilesRevealed)
        {
            UpdateGameState(GameState.Victory);
        }
    }

    protected void RevealTrappedTile(TrappedTile tile)
    {
        tile.State = TileState.Revealed;
        UpdateGameState(GameState.Defeat);
    }

    protected void RevealSafeTile(SafeTile tile, GridPoint point)
    {
        tile.State = TileState.Revealed;
        RevealedSafeTilesCount++;
        if (tile.TrappedNeighbors == 0)
        {
            RevealNeighborsToPoint(point);
        }
    }

    protected int CountAdjacentTraps(GridPoint point)
    {
        var count = 0;

        foreach (var neighborPoint in Grid.EnumerateNeighbors(point))
        {
            if (point != neighborPoint && Grid[neighborPoint] is TrappedTile)
            {
                count++;
            }
        }

        return count;
    }
}
