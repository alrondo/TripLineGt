using System;

namespace TripLine.Toolbox
{
    /// <summary>
    /// Encapsulates a frequency value in a unit-safe manner.
    /// </summary>
    /// <remarks>
    /// Internally, the frequency value is always stored in hertz.
    /// </remarks>
    public struct Frequency
    {
        private const int MHz = 1000000;

        private Frequency(int valueInHz) { Hertz = valueInHz; }

        /// <summary>
        /// Factory method to construct a Frequency value from a value in Hz
        /// </summary>
        public static Frequency FromHertz(int value) => new Frequency((int)value);

        /// <summary>
        /// Factory method to construct a Frequency value from a value in MHz
        /// </summary>
        public static Frequency FromMegaHertz(double value) => new Frequency((int)(value * MHz));

        /// <summary>
        /// Gets the current Frequency value in Hz
        /// </summary>
        public int Hertz { get; }

        /// <summary>
        /// Gets the current Frequency value in MHz
        /// </summary>
        public double MegaHertz => (double)Hertz / MHz;

        public static Frequency operator +(Frequency left, Frequency right) => new Frequency(left.Hertz + right.Hertz);
        public static Frequency operator -(Frequency left, Frequency right) => new Frequency(left.Hertz - right.Hertz);
        public static Frequency operator /(Frequency numerator, int denominator)
        {
            if (denominator == 0)
                throw new ArgumentException(nameof(denominator));

            return new Frequency(numerator.Hertz / denominator);
        }

        public static bool operator >(Frequency left, Frequency right) => left.Hertz > right.Hertz;
        public static bool operator <(Frequency left, Frequency right) => left.Hertz < right.Hertz;

        public override string ToString() => $"{MegaHertz:F1} MHz";
    }

    public static class FrequencyFactoryExtensions
    {
        public static Frequency Hertz(this int valueInHertz) => Frequency.FromHertz(valueInHertz);
        public static Frequency MegaHertz(this double valueInMegaHertz) => Frequency.FromMegaHertz(valueInMegaHertz);
        public static Frequency MegaHertz(this int valueInMegaHertz) => Frequency.FromMegaHertz(valueInMegaHertz);
    }
}
