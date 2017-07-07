using System;
using System.Collections.Generic;

namespace TripLine.Toolbox.Extensions
{
    public static class DisposableExtensions
    {
        public static IDisposable ToDisposable(this IEnumerable<IDisposable> disposables)
        {
            return Disposable.Create(() => disposables.ForEach(d => d.Dispose()));
        }
    }
}
