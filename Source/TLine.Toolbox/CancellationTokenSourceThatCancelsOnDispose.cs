using System.Threading;

namespace TripLine.Toolbox
{
    public class CancellationTokenSourceThatCancelsOnDispose : CancellationTokenSource
    {
        protected override void Dispose(bool disposing)
        {
            if (IsCancellationRequested == false)
            {
                Cancel();
            }

            base.Dispose(disposing);
        }
    }
}
