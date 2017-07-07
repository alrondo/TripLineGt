using System;

namespace TripLine.Toolbox.Extensions
{
    public static class RandomExtensions
    {
        public static byte[] NextBytes(this Random random, int numberOfBytes)
        {
            var buffer = new byte[numberOfBytes];
            random.NextBytes(buffer);
            return buffer;
        }
    }
}
