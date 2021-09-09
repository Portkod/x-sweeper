using Sweeper.Core;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sweeper.DesktopApp
{
    /// <summary>
    /// Interaction logic for GameBoard.xaml
    /// </summary>
    public partial class GameBoard : UserControl
    {
        private Button[,] tileElements;
        private SweeperGame Game;
        public GameBoard()
        {
            InitializeComponent();
            Game = new SweeperGame(16, 16, 40);
            tileElements = new Button[Game.Rows, Game.Columns];
            for (int i = 0; i < Game.Rows; i++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < Game.Columns; i++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int x = 0; x < Game.Rows; x++)
            {
                for (int y = 0; y < Game.Columns; y++)
                {
                    var tile = Game.GetTileAt(new(x, y));
                    var tileElement = CreateTileElement(tile, new(x, y));
                    Grid.SetRow(tileElement, x);
                    Grid.SetColumn(tileElement, y);
                    tileElements[x, y] = tileElement;
                    BoardGrid.Children.Add(tileElement);
                }
            }
            Game.GridUpdated += Game_GridUpdated;
        }

        private void Game_GridUpdated(object? sender, GridUpdatedArgs e)
        {
            UpdateTileElements();
        }

        private void UpdateTileElements()
        {
            for (int x = 0; x < Game.Rows; x++)
            {
                for (int y = 0; y < Game.Columns; y++)
                {
                    var tileElement = tileElements[x, y];
                    var tile = Game.GetTileAt(new(x, y));
                    if (tile == null)
                    {
                        continue;
                    }
                    string symbol = tile.State == TileState.Hidden ? "-" : tile is TrappedTile ? "*" : ((SafeTile)tile).TrappedNeighbors.ToString();
                    if (tile.State == TileState.Hidden) tileElement.Background = Brushes.White;
                    if (tile.State == TileState.Revealed && tile is TrappedTile) tileElement.Background = Brushes.Red;
                    if (tile.State == TileState.Revealed && tile is SafeTile) tileElement.Background = Brushes.Green;
                    if (tile.State == TileState.Marked) tileElement.Background = Brushes.Yellow;
                    tileElement.Content = symbol;

                }
            }
        }

        private Button CreateTileElement(Tile tile, GridPoint point)
        {
            var button = new Button();
            button.Click += (o, e) =>
            {
                Game.RevealTile(point);
            };
            button.MouseRightButtonUp += (o, e) =>
            {
                Game.MarkTile(point);
            };
            return button;

        }
    }
}
