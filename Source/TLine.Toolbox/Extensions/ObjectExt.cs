using System;

namespace TripLine.Toolbox.Extensions
{
    public static class ObjectExt
    {
        public static T CastTo<T>(this object obj)
        {
            return (T) obj;
        }
        public static void Maybe<TInstance>(this TInstance instance, Action action)
        {
            if ((object)instance == null)
                return;
            action();
        }

        public static void Maybe<TInstance>(this TInstance instance, Action<TInstance> action)
        {
            if ((object)instance == null)
                return;
            action(instance);
        }

        public static TResult Maybe<TResult, TInstance>(this TInstance instance, Func<TInstance, TResult> function)
        {
            return ObjectExt.Maybe<TResult, TInstance>(instance, function, default(TResult));
        }

        public static TResult Maybe<TResult, TInstance>(this TInstance instance, Func<TInstance, TResult> function, TResult defaultValue)
        {
            if ((object)instance != null)
                return function(instance);
            return defaultValue;
        }
    }
}