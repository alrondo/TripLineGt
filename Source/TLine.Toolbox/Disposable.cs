using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox
{
    public class Disposable : IDisposable
    {
        private readonly Action _action;

        private Disposable(Action action)
        {
            _action = action;
        }

        public static IDisposable Create(Action action)
        {
            return new Disposable(action);
        }

        public void Dispose()
        {
            _action();
        }
    }
}
