using System;
using System.Collections.Generic;
using System.Linq;

namespace TripLine.Toolbox.Extensions
{
    public static class AppDomainExt
    {
        public static IEnumerable<Type> FindTypes<T>(this AppDomain domain, string ignoreNamespace = null)
        {
            var types = new List<Type>();
            foreach (var assembly in domain.GetAssemblies())
            {
                try
                {
                    types.AddRange(
                        assembly.GetExportedTypes()
                            .Where(
                                t =>
                                typeof(T).IsAssignableFrom(t) && t.IsClass && !t.ContainsGenericParameters
                                && CheckNamespace(t, ignoreNamespace)));

                }
                catch
                {
                    // Ignored because there are some dynamic assemblies which cannot be reflected. 
                }
            }

            return types;

        }

        private static bool CheckNamespace(Type type, string ignore)
        {
            if (ignore != null)
            {
                return !type.Namespace.Contains(ignore);
            }
            return true;
        }
    }
}