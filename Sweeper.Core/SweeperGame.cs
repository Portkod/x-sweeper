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
    public Grid<Tile> Grid { get; }
    private GameState _state = GameState.New;
    public GameState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                var previousState = _state;
                _state = value;
                GameStateUpdated?.Invoke(this, new(previousState, _state));
            }
        }
    }
    private int _revealedSafeTilesCount = 0;

    public SweeperGame(int rows, int columns, int totalTraps)
    {
        Grid = new(rows, columns);
        TotalTraps = totalTraps;
        RemainingTraps = TotalTraps;
    }

    private void PlaceTraps(GridPoint safePoint)
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

    private void RevealEntireGrid()
    {
        foreach (var item in Grid)
        {
            var tile = item.Cell;
            if (tile == null)
            {
                tile = CreateSafeTileAtPoint(item.Point);
            }
            tile.State = TileState.Revealed;
            _revealedSafeTilesCount++;
        }
        GridUpdated?.Invoke(this, new());
    }

    private void UpdateGameState(GameState newState)
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

    private Tile CreateTrappedTileAtPoint(GridPoint point, TileState state = TileState.Hidden)
    {
        var tile = new TrappedTile { State = state };
        Grid[point] = tile;

        return tile;
    }
    private Tile CreateSafeTileAtPoint(GridPoint point, TileState state = TileState.Hidden)
    {
        var tile = new SafeTile { State = state, TrappedNeighbors = CountAdjacentTraps(point) };
        Grid[point] = tile;

        return tile;
    }

    private void RevealNeighborsToPoint(GridPoint point)
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
        GridUpdated?.Invoke(this, new());
    }

    public void RevealTile(GridPoint point) => RevealTile(point, true);

    private void RevealTile(GridPoint point, bool invokeUpdate)
    {
        if (Grid.PointIsOutOfBounds(point))
        {
            // TODO Invalid index. Throw?
            return;
        }

        if (State == GameState.New)
        {
            PlaceTraps(point);
            UpdateGameState(GameState.InProgress);
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
            GridUpdated?.Invoke(this, new());
        }
        if (IsVictory())
        {
            UpdateGameState(GameState.Victory);
        }
    }

    private void RevealTrappedTile(TrappedTile tile)
    {
        tile.State = TileState.Revealed;
        UpdateGameState(GameState.Defeat);
    }

    private void RevealSafeTile(SafeTile tile, GridPoint point)
    {
        tile.State = TileState.Revealed;
        _revealedSafeTilesCount++;
        if (tile.TrappedNeighbors == 0)
        {
            RevealNeighborsToPoint(point);
        }
    }

    private bool IsVictory()
    {
        return _revealedSafeTilesCount == Grid.Count - TotalTraps;
        //var safeTilesRevealedCount = Grid.Count(x => x.Cell != null && x.Cell is SafeTile && x.Cell.State == TileState.Revealed);
        //return safeTilesRevealedCount == Grid.Count - TotalTraps;
    }

    private int CountAdjacentTraps(GridPoint point)
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
