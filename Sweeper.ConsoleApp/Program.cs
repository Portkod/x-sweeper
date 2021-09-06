
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
        board.RevealTile(new(int.Parse(foo[1]), int.Parse(foo[2])));
    }
    else if (foo[0] == "m")
    {
        board.MarkTile(new(int.Parse(foo[1]), int.Parse(foo[2])));
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
            var tile = game.Grid[new(row, col)];
            if (tile == null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[-]");
                Console.ForegroundColor = oldColor;
                continue;
            }
            var symbol = tile.State == TileState.Hidden ? "-" : tile is TrappedTile ? "*" : ((SafeTile)tile).TrappedNeighbors.ToString();
            if (tile.State == TileState.Hidden) Console.ForegroundColor = ConsoleColor.White;
            if (tile.State == TileState.Revealed && tile is TrappedTile) Console.ForegroundColor = ConsoleColor.Red;
            if (tile.State == TileState.Revealed && tile is SafeTile) Console.ForegroundColor = ConsoleColor.Green;
            if (tile.State == TileState.Marked) Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{symbol}]");
            Console.ForegroundColor = oldColor;
        }
        Console.WriteLine();
    }
}