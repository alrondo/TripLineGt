using System;
using System.IO;
using System.Reflection;

namespace TripLine.Toolbox.Extensions
{
    public static class AssemblyExtensions
    {
        public static Stream GetEmbeddedResource(this Assembly assembly, string relativeName)
        {
            var assemblyName = new AssemblyName(assembly.FullName);
            var fullResourceName = assemblyName.Name + "." + relativeName;

            var result = assembly.GetManifestResourceStream(fullResourceName);
            if (result == null)
                throw new ArgumentException(string.Format("Resource '{0}' was not found.", relativeName));

            return result;
        }

        public static string GetAssemblyDirectory(this Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
