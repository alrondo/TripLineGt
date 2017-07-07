using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using TripLine.Toolbox.EventSource;
using log4net;

namespace TripLine.Toolbox.Extensions
{
    public static class PerformanceEventSourceExtensions
    {
        // Used to calculate inner scopes indentation
        // Start at -1 so first level is not indented
        private static readonly long NanoSecondsPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        

        public static IDisposable LogDurationOfScope(this StreamingEventSource log, string message)
        {
            return LogDurationOfScope(log, _ => message);
        }

        public static IDisposable LogDurationOfScope(this StreamingEventSource log, string messageFormat, params object[] options)
        {
            var message = string.Format(CultureInfo.InvariantCulture, messageFormat, options);

            return LogDurationOfScope(log, _ => message);
        }

        public static IDisposable LogDurationOfScope(this StreamingEventSource log, Func<TimeSpan, string> logMessageFunc)
        {
            var durationStopwatch = Stopwatch.StartNew();

            return Disposable.Create(
                () =>
                {
                    durationStopwatch.Stop();

                    StreamingEventSource.Log.Duration(logMessageFunc(durationStopwatch.Elapsed), durationStopwatch.ElapsedTicks * NanoSecondsPerTick);

                });
        }

        public static IDisposable LogCopyDurationOfScope(this StreamingEventSource log, ulong iteration)
        {
            var durationStopwatch = Stopwatch.StartNew();

            return Disposable.Create(
                () =>
                {
                    durationStopwatch.Stop();

                    StreamingEventSource.Log.BufferCopyDuration(iteration, durationStopwatch.ElapsedTicks * NanoSecondsPerTick);

                });
        }

        public static IDisposable LogWaitCopyDurationOfScope(this StreamingEventSource log, ulong iteration)
        {
            var durationStopwatch = Stopwatch.StartNew();

            return Disposable.Create(
                () =>
                {
                    durationStopwatch.Stop();

                    StreamingEventSource.Log.WaitOnBufferCopyDuration(iteration, durationStopwatch.ElapsedTicks * NanoSecondsPerTick);

                });
        }

        public static IDisposable LogGrabDurationOfScope(this StreamingEventSource log,ulong iteration, uint bufferSize)
        {
            var durationStopwatch = Stopwatch.StartNew();
            StreamingEventSource.Log.StartGrabbing(iteration, bufferSize);

            return Disposable.Create(
                () =>
                {
                    durationStopwatch.Stop();

                    StreamingEventSource.Log.GrabDuration(iteration, durationStopwatch.ElapsedTicks * NanoSecondsPerTick);

                });
        }
    }
}