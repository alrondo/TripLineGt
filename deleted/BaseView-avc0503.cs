using System.Threading.Tasks;
using Tripline.DesktopApp.ContentUserControl;

namespace Tripline.DesktopLine.ViewModel
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using TLine.WPF.MVVM;
    using TLine.Toolbox.UI;

    public class BaseView : BaseUserControl
    {
        public INavigator Nav { get; set; }

        public virtual void ViewLoaded()
        {
            // Grid should be the top most object! 
            if (!(Content is Grid))
            {
                throw new BaseViewException("Grid needs to be the top most container");
            }

            // We add a new row on top to manage our header
            var grid = Content as Grid;
            grid.RowDefinitions.Insert(0, new RowDefinition(){Height = new GridLength(70)});

            if (grid.RowDefinitions.Count == 1)
            {
                grid.RowDefinitions.Add(new RowDefinition(){Height = new GridLength(1, GridUnitType.Star)});
            }

            // Move all children down
            foreach (var child in grid.Children.Cast<UIElement>())
            {
                Grid.SetRow(child, Grid.GetRow(child) + 1);
            }

            // Add header
            var label = new Label();
            label.Style = Application.Current.FindResource("LabelHeader") as Style;
            var binding = new Binding("Name");
            binding.Source = DataContext;
            label.SetBinding(Label.ContentProperty, binding);
            
            // Add Grid
            var headerGrid = new Grid();
            headerGrid.CreateColumns("70", "*");
            headerGrid.Cell().Column(1).AddUi(label);

            // Add Back button if need be
            if (Nav.HasBreadCrumb())
            {
                var bButton = new Button();
                bButton.Width = 40;
                bButton.VerticalAlignment = VerticalAlignment.Bottom;
                bButton.Style = Application.Current.Resources["ArrowBack"] as Style;
                bButton.Click += (sender, args) => Nav.GoBack();
                headerGrid.Cell().Column(0).AddUi(bButton);
            }

            grid.Cell().Row(0).AddUi(headerGrid);
        }

        public virtual void ViewUnloaded(){}
    }

    public class BaseViewException : Exception
    {
        public BaseViewException(string message) : base(message)
        {
            
        }
    }
}