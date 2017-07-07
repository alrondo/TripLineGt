using System;
using System.Windows;
using MahApps.Metro;

public static class TLineStyle
{
    static TLineStyle()
    {
    }

    public static void Load()
    {
        // Load and apply TripLine style
        ThemeManager.AddAccent("TLineAccent", new Uri("pack://application:,,,/TLine.Style;component/TLineAccent.xaml"));
        ThemeManager.AddAppTheme("TLineTheme", new Uri("pack://application:,,,/TLine.Style;component/TLineTheme.xaml"));
        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("TLineAccent"), ThemeManager.GetAppTheme("TLineTheme"));
    }
}
