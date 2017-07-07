using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace TripLine.DesktopApp
{
    ///«// <summary>
    ///// Interaction logic for App.xaml
    ///// </summary>
    //public partial class App : Application
    //{

    //    // Load and apply Averna style
    //    TLineStyle.Load();

    //}

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            _log.Info("Starting desktop app");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);

            // Load and apply Averna style
            TLineStyle.Load();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                _log.Fatal("Unhandled exception", exception);
            }
            else
            {
                _log.FatalFormat("Unhandled exception: {0}", e.ExceptionObject);
            }
        }
    }
}
