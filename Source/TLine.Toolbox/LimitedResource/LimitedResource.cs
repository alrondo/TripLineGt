using System;

namespace TripLine.Toolbox.LimitedResource
{
    public class LimitedResource<T>
    {
        private readonly T _obj;
        private readonly Action<T> _releaseAction;

        internal LimitedResource(T obj, Action<T> releaseAction)
        {
            _obj = obj;
            _releaseAction = releaseAction;
        }

        public T Get()
        {
            return _obj;
        }

        public void Release()
        {
            _releaseAction(_obj);
        }
    }
}