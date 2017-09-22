using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TripLine.WPF.MVVM;
using TripLine.DesktopApp.ViewModels;

namespace TripLine.DesktopApp.View
{
    /// <summary>
    /// Interaction logic for HighliteView.xaml
    /// </summary>
    public partial class HighliteView : BaseView, IViewFor<HighliteViewModel>
    {
        public HighliteView()
        {
            InitializeComponent();
        }

        //INavigator Nav { get; set; }

        public override void ViewLoaded()
        {
            base.ViewLoaded();

            //if (_commands.LeftExist(_header))
            //    _commands.RemoveLeft(_header);
            //_commands.RemoveLeft(element => element is Viewbox);

        }

        public override void ViewUnloaded()
        {
            base.ViewUnloaded();

            //if (_connection != null)
            //    _commands.AddLeft(new Viewbox() { Child = new Label { Content = _connection.ConnectionStr.ToUpper() } });
        }

        public HighliteViewModel ModelView { get; set; }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - e.Delta / 3);
        }
    }
}
