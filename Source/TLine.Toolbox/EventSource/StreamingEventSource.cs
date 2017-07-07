using System;
using System.Diagnostics;

namespace TripLine.Toolbox.EventSource
{
    public class StreamingEventSource : System.Diagnostics.Tracing.EventSource
    {
        public static StreamingEventSource Log = new StreamingEventSource();

        protected StreamingEventSource()
        {
            PerformanceStats((1000L * 1000L * 1000L) / Stopwatch.Frequency);
        }

        public void PerformanceStats(long NanoSecondsPerTick)
        {
            WriteEvent(1, NanoSecondsPerTick);
        }

        public void Duration(string Name, double Duration)
        {
            WriteEvent(2, Name, Duration);
        }

        public void BufferCopyDuration(ulong Iteration, double Duration)
        {
            WriteEvent(3, Iteration, Duration);
        }

        public void WaitOnBufferCopyDuration(ulong Iteration, double Duration)
        {
            WriteEvent(4, Iteration, Duration);
        }

        public void DmaProgrammed(ulong Iteration, uint Dma, uint BufferIndex)
        {
            WriteEvent(5, Iteration, Dma, BufferIndex);
        }

        public void StartGrabbing(ulong Iteration, uint BufferSize)
        {
            WriteEvent(6, Iteration, BufferSize);
        }

        public void GrabDuration(ulong Iteration, double Duration)
        {
            WriteEvent(7, Iteration, Duration);
        }

    }
}