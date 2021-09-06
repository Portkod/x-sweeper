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
    public Grid<Cell> Grid { get; }
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
    private int _revealedSafeCellCount = 0;

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
            var randomIndex = Random.Shared.Next(Grid.TotalItems);
            var trapPoint = Grid.IndexToPoint(randomIndex);
            if (trapPoint == safePoint)
            {
                continue;
            }

            if (Grid[trapPoint] == null)
            {
                CreateTrappedCellAtPoint(trapPoint);
                remainingTrapsToPlace--;
            }
        }
    }

    private void RevealEntireGrid()
    {
        foreach (var item in Grid)
        {
            var cell = item.Item;
            if (cell == null)
            {
                cell = CreateSafeCellAtPoint(item.Point);
            }
            cell.State = CellState.Revealed;
            _revealedSafeCellCount++;
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

    private Cell CreateTrappedCellAtPoint(GridPoint point, CellState state = CellState.Hidden)
    {
        var cell = new TrappedCell { State = state };
        Grid[point] = cell;

        return cell;
    }
    private Cell CreateSafeCellAtPoint(GridPoint point, CellState state = CellState.Hidden)
    {
        var cell = new SafeCell { State = state, TrappedNeighbors = CountAdjacentTraps(point) };
        Grid[point] = cell;

        return cell;
    }

    private void RevealNeighborsToPoint(GridPoint point)
    {
        foreach (var neighborPoint in Grid.EnumeratePointNeighbors(point))
        {
            RevealCell(neighborPoint, false);
        }
    }

    public void MarkCell(GridPoint point)
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

        Cell cell = Grid[point];
        if (cell == null)
        {
            cell = CreateSafeCellAtPoint(point);
        }

        switch (cell.State)
        {
            case CellState.Hidden:
                cell.State = CellState.Marked;
                break;
            case CellState.Marked:
                cell.State = CellState.Hidden;
                break;
        }
        GridUpdated?.Invoke(this, new());
    }

    public void RevealCell(GridPoint point) => RevealCell(point, true);

    private void RevealCell(GridPoint point, bool invokeUpdate)
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

        Cell cell = Grid[point];
        if (cell == null)
        {
            cell = CreateSafeCellAtPoint(point);
        }
        else if (cell.State == CellState.Revealed || cell.State == CellState.Marked)
        {
            return;
        }

        switch (cell)
        {
            case SafeCell safeCell:
                RevealSafeCell(safeCell, point);
                break;
            case TrappedCell trappedCell:
                RevealTrappedCell(trappedCell);
                break;
            default:
                throw new Exception("Unsupported cell type.");
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

    private void RevealTrappedCell(TrappedCell cell)
    {
        cell.State = CellState.Revealed;
        UpdateGameState(GameState.Defeat);
    }

    private void RevealSafeCell(SafeCell cell, GridPoint point)
    {
        cell.State = CellState.Revealed;
        _revealedSafeCellCount++;
        if (cell.TrappedNeighbors == 0)
        {
            RevealNeighborsToPoint(point);
        }
    }

    private bool IsVictory()
    {
        return _revealedSafeCellCount == Grid.TotalItems - TotalTraps;
        //var safeCellsRevealedCount = Grid.Count(x => x.Item != null && x.Item is SafeCell && x.Item.State == CellState.Revealed);
        //return safeCellsRevealedCount == Grid.TotalItems - TotalTraps;
    }

    private int CountAdjacentTraps(GridPoint point)
    {
        var count = 0;

        foreach (var neighborPoint in Grid.EnumeratePointNeighbors(point))
        {
            if (point != neighborPoint && Grid[neighborPoint] is TrappedCell)
            {
                count++;
            }
        }

        return count;
    }
}
