using System;

namespace TripLine.Toolbox.LimitedResource
{
    public class LimitedResourceException : Exception
    {
        public LimitedResourceException(string message) : base(message)
        {
        }
    }
}