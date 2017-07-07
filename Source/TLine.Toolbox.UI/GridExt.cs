namespace TripLine.Toolbox.UI
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class GridCell
    {
        private int _Row { get; set; }
        private int _Column { get; set; }
        private int _SpanRow { get; set; }
        private int _SpanCol { get; set; }
        private Grid _ParentGrid { get; set; }
        private Thickness _thickness = new Thickness(0);
        public GridCell(Grid parentGrid)
        {
            _ParentGrid = parentGrid;
            _Row = 0;
            _Column = 0;
            _SpanCol = 1;
            _SpanRow = 1;
        }

        public GridCell AddUi(UIElement ui)
        {
            _ParentGrid.Children.Add(ui);
            Grid.SetColumn(ui, _Column);
            Grid.SetRow(ui, _Row);
            Grid.SetRowSpan(ui, _SpanRow);
            Grid.SetColumnSpan(ui, _SpanCol);

            var element = ui as FrameworkElement;
            if (element != null)
            {
                element.Margin = _thickness;
            }

            return this;
        }

        public GridCell Row(int row)
        {
            _Row = row;
            return this;
        }

        public GridCell Margin(double margin)
        {
            _thickness = new Thickness(margin);
            return this;
        }

        public GridCell Column(int col)
        {
            _Column = col;
            return this;
        }

        public Grid ReturnGrid()
        {
            return _ParentGrid;
        }
    }
    public static class GridExtensions
    {
        public static GridCell Cell(this Grid grid)
        {
            return new GridCell(grid);
        }

        public static Grid CreateColumns(this Grid grid, params string[] columns)
        {
            foreach (var column in columns)
            {
                var gridLenght = new GridLength();
                var width = 0.0;
                if (column.IndexOf('*') > -1)
                {
                    if (column.Count() > 1)
                    {
                        width = double.Parse(column.Substring(0, column.IndexOf('*')));
                    }
                    else
                    {
                        width = 14;
                    }
                    gridLenght = new GridLength(width, GridUnitType.Star);
                }
                else
                {
                    width = double.Parse(column);
                    gridLenght= new GridLength(width);
                }
                grid.ColumnDefinitions.Add(new ColumnDefinition(){Width = gridLenght});
            }

            return grid;
        }

        public static Grid CreateRows(this Grid grid, params string[] rows)
        {
            foreach (var column in rows)
            {
                var gridLenght = new GridLength();
                var width = 0.0;
                if (column.IndexOf('*') > -1)
                {
                    if (column.Count() > 1)
                    {
                        width = double.Parse(column.Substring(0, column.IndexOf('*')));
                    }
                    else
                    {
                        width = 1;
                    }
                    gridLenght = new GridLength(width, GridUnitType.Star);
                }
                else
                {
                    width = double.Parse(column);
                    gridLenght = new GridLength(width);
                }
                grid.RowDefinitions.Add(new RowDefinition() { Height = gridLenght });
            }

            return grid;
        }

    }
}