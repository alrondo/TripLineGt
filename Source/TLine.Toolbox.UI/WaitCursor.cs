using System;
using System.Windows.Input;

namespace TripLine.Toolbox.UI
{
    public class WaitCursor : IDisposable
    {
        private bool _disposed = false;

        public WaitCursor()
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            if (_disposed == false)
            {
                _disposed = true;
                Mouse.OverrideCursor = null;
            }
        }
    }
}
