using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TripLine.DesktopApp.ContentUserControl;
using TripLine.DesktopApp.ViewModels;
using TripLine.Service;

namespace TripLine.DesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IWindowCommands
    {

        private readonly MainViewModel _mainViewModel = null;


        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel(ContentControl);
            
            this.DataContext = _mainViewModel;

            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var flyout = this.Flyouts.Items[0] as Flyout;
            var flyoutCCntl = this.MainFlyoutContentControl;

            var main = new Main(this, flyout, flyoutCCntl);
            ContentControl.Content = main;
        }

        private void MainFlyout_ClosingFinished(object sender, RoutedEventArgs e)
        {
        }

       

        public void AddLeft(UIElement element)
        {
            LeftWindowCommands.Items.Add(element);
        }

        public void AddRight(UIElement element)
        {
            RightWindowCommands.Items.Add(element);
        }

        public void RemoveLeft(UIElement element)
        {
            LeftWindowCommands.Items.Remove(element);

        }

        public void RemoveLeft(Func<UIElement, bool> removeAction)
        {
            List<UIElement> toRemove = new List<UIElement>();

            foreach (var item in LeftWindowCommands.Items)
            {
                var el = (UIElement)item;
                if (removeAction(el)) toRemove.Add(el);

            }

            toRemove.ForEach(RemoveLeft);

        }

        public void RemoveRight(UIElement element)
        {
            RightWindowCommands.Items.Remove(element);
        }

        public bool LeftExist(UIElement element)
        {
            return LeftWindowCommands.Items.Contains(element);
        }

        public bool RightExist(UIElement element)
        {
            return RightWindowCommands.Items.Contains(element);
        }

        public async void EnableButtons(bool enable)
        {
            //foreach (var item in RightWindowCommands.Items)
            //{
            //    (item as FrameworkElement).IsEnabled = enable;
            //}

            //foreach (var item in LeftWindowCommands.Items)
            //{
            //    (item as FrameworkElement).IsEnabled = enable;
            //}
            await _mainViewModel.GoBack();

        }


        private void CloseOpenedFlyout(object parameter)
        {
            var openedFlyout = Flyouts.Items
                .OfType<Flyout>()
                .FirstOrDefault(flyout => flyout.IsOpen);

            if (openedFlyout != null)
            {
                openedFlyout.IsOpen = false;
            }
        }

        private async void BackBtn_OnClick(object sender, RoutedEventArgs e)
        {
            await _mainViewModel.GoBack();
        }

        private void RemoveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _mainViewModel.RemoveCommand();
        }
    }
}
