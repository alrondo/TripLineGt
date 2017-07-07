using System;
using System.Timers;

namespace TripLine.Toolbox.Extensions
{
    public static class TimerExtensions
    {
        public static IDisposable StopAndRestart(this Timer timer)
        {
            timer.Stop();
            return Disposable.Create(timer.Start);
        }
    }
}