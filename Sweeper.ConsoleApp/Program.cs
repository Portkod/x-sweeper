
using Sweeper.Core;

var board = new SweeperGame(10, 10, 10);
board.GridUpdated += (sender, args) =>
{
    DrawBoard(board);
};
board.GameStateUpdated += (sender, args) =>
{
    switch (args.State)
    {
        case GameState.Defeat:
            Console.WriteLine("Fail!");
            break;
        case GameState.Victory:
            Console.WriteLine("Win!");
            DrawBoard(board);
            break;
    }
};
DrawBoard(board);

string? line = "";
while ((line = Console.ReadLine()) != "")
{
    var foo = line.Split(',');
    if (foo.Length != 3)
    {
        break;
    }
    if (foo[0] == "r")
    {
        board.RevealCell(new(int.Parse(foo[1]), int.Parse(foo[2])));
    }
    else if (foo[0] == "m")
    {
        board.MarkCell(new(int.Parse(foo[1]), int.Parse(foo[2])));
    }
}


static void DrawBoard(SweeperGame game)
{
    Console.Clear();

    Console.Write($"   ");
    for (int col = 0; col < game.Grid.Columns; col++)
    {
        Console.Write($"{col} ".PadLeft(3));
    }
    Console.WriteLine();

    for (int row = 0; row < game.Grid.Rows; row++)
    {
        Console.Write($"{row} ".PadLeft(3));
        for (int col = 0; col < game.Grid.Columns; col++)
        {
            var oldColor = Console.ForegroundColor;
            var cell = game.Grid[new(row, col)];
            if (cell == null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[-]");
                Console.ForegroundColor = oldColor;
                continue;
            }
            var symbol = cell.State == CellState.Hidden ? "-" : cell is TrappedCell ? "*" : ((SafeCell)cell).TrappedNeighbors.ToString();
            if (cell.State == CellState.Hidden) Console.ForegroundColor = ConsoleColor.White;
            if (cell.State == CellState.Revealed && cell is TrappedCell) Console.ForegroundColor = ConsoleColor.Red;
            if (cell.State == CellState.Revealed && cell is SafeCell) Console.ForegroundColor = ConsoleColor.Green;
            if (cell.State == CellState.Marked) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{symbol}]");
            Console.ForegroundColor = oldColor;
        }
        Console.WriteLine();
    }
}