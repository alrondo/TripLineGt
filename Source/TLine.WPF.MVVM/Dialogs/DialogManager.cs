
namespace TripLine.WPF.MVVM.Dialogs
{
    using System.Windows.Controls;

    public class DialogManager
    {
        private readonly INavigator _nav;
        private SimpleDialog _dialog;
        
        public DialogManager(INavigator nav)
        {
            _nav = nav;
        }

        public INavigator Navigator { get { return _nav; } }

        /// <summary>
        /// Show the dialog parameter to the user
        /// </summary>
        /// <param name="diag"></param>
        /// <returns>False if there is already a dialog opened, true otherwise.</returns>
        public bool ShowDialog(SimpleDialog diag)
        {
            if (_dialog != null)
            {
                return false;
            }

            _dialog = diag;
            
            var view = _nav.CurrentView as UserControl;
            
            if (view.Content is Grid)
            {
                var grid = view.Content as Grid;
                _dialog.Close += () =>
                {
                    grid.Children.Remove(_dialog);
                    _dialog = null;
                };

                grid.Children.Add(_dialog);
                if (grid.ColumnDefinitions.Count > 0)
                    Grid.SetColumnSpan(_dialog, grid.ColumnDefinitions.Count);
                if (grid.RowDefinitions.Count > 0)
                    Grid.SetRowSpan(_dialog, grid.RowDefinitions.Count);
            }
            return true;
        }

        public void CloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.CloseDialog();
                _dialog = null;
            }
        }
    }
}