namespace TripLine.Toolbox.Extensions
{
    using System;

    public static class EnvironmentExt
    {
        public static string GetPath(this Environment.SpecialFolder folder)
        {
            return Environment.GetFolderPath(folder);
        }
    }
}
