namespace TripLine.WPF.MVVM
{
    using System;

    public class NavigationException : Exception
    {
        public NavigationException(string frmt):base(frmt)
        {
        }
    }
}