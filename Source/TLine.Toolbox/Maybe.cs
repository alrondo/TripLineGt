using System.Collections.Generic;

namespace TripLine.Toolbox
{
    public class Maybe<T> where T : class
    {
        public Maybe()
        {
            Value = default(T);
            HasValue = false;
        }

        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public bool HasValue { get; }
        public T Value { get; }

        public IEnumerable<T> GetEnumerable()
        {
            if (HasValue)
                return new[] { Value };

            return new T[0];
        }
    }
}
