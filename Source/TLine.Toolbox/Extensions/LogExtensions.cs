using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using log4net;

namespace TripLine.Toolbox.Extensions
{
    public static class LogExtensions
    {
        // Used to calculate inner scopes indentation
        // Start at -1 so first level is not indented
        private static readonly ThreadLocal<int> ScopeDepth = new ThreadLocal<int>(() => -1);

        public static IDisposable LogDurationOfScope(this ILog log, string message)
        {
            return (log.IsInfoEnabled == false) ? null : LogDurationOfScope(log, _ => message);
        }

        public static IDisposable LogDurationOfScope(this ILog log, string messageFormat, params object[] options)
        {
            if (log.IsInfoEnabled == false) return null;

            var message = string.Format(CultureInfo.InvariantCulture, messageFormat, options);

            return LogDurationOfScope(log, _ => message);
        }

        public static IDisposable LogDurationOfScope(this ILog log, Func<TimeSpan, string> logMessageFunc)
        {
            if (log.IsInfoEnabled == false) return null;

            var indent = new string(' ', ++ScopeDepth.Value * 2);    // indent each level by 2 spaces

            var durationStopwatch = Stopwatch.StartNew();

            return Disposable.Create(
                () =>
                {
                    durationStopwatch.Stop();

                    var fullMessage = string.Format(CultureInfo.InvariantCulture, "{2}{0} took {1} ms", 
                        logMessageFunc(durationStopwatch.Elapsed), durationStopwatch.ElapsedMilliseconds, indent);

                    log.Info(fullMessage);

                    --ScopeDepth.Value;
                });
        }

        public static void Debug(this ILog log, Func<string> debugMessage)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(debugMessage());
            }
        }
    }
}
