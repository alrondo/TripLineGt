using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using TripLine.Toolbox.Extensions;
using log4net;

namespace TripLine.Toolbox.LimitedResource
{
    /// <summary>
    /// Manager of a list of objects which are used as resources. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LimitedResourceManager<T> where T : class
    {
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private List<T> _resources; // This list will be change size as resources are taken and put back. 
        private object _resourceLock = new object();


        public LimitedResourceManager(params T[] objects)
        {
            _resources = objects.ToList();
        }
        public LimitedResourceManager(IEnumerable<T> objects)
        {
            _resources = objects.ToList();
        }

        public LimitedResource<T> GetResource(Func<T, bool> preferredResource = null, bool returnAnyIfPreferredNotThere = true)
        {
            lock (_resourceLock)
            {

                if (_resources.Count == 0)
                {
                    throw new LimitedResourceException("No resource available");
                }


                T resource = default(T);

                if (preferredResource != null)
                {
                    // Try to get the preferred resource
                    resource = _resources.FirstOrDefault(preferredResource);

                    if (resource == null && !returnAnyIfPreferredNotThere)
                    {
                        throw new LimitedResourceException("Preferred resource was not found");
                    }
                }

                if (resource == null)
                {
                    resource = _resources.ElementAt(0);
                }

                _resources.Remove(resource);

                _log.Debug(() =>
                {
                    var callstack = new StackTrace();
                    var str = new StringBuilder();
                    str.AppendLine("Returning a limited resource");
                    foreach (var stackFrame in callstack.GetFrames())
                    {
                        str.AppendLine(stackFrame.ToString());
                    }
                    return str.ToString();
                });


                return new LimitedResource<T>(resource, (lr) =>
                {
                    lock (_resourceLock)
                    {
                        _resources.Add(lr);
                    }
                });
            }
        }

        public int Count { get { return _resources.Count; } }

    
    }
}