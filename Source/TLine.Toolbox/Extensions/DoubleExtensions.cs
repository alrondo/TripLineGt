using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TripLine.Toolbox.Extensions
{
    public static class DoubleExtensions
    {
        public static double AbsoluteValue(this double actualValue)
        {
            return Math.Abs(actualValue);
        }

        public static bool IsApproximately(this double actualValue, double expectedValue, double acceptableDelta)
        {
            return Math.Abs(actualValue - expectedValue) <= acceptableDelta;
        }
    }
}
