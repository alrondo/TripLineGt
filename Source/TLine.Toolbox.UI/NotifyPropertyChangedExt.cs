namespace TripLine.Toolbox.UI
{
    using System;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public class NotifyPropertyAction
    {
        private readonly string _name;

        private Action _action;

        public NotifyPropertyAction(string name, INotifyPropertyChanged changed)
        {
            _name = name;
            changed.PropertyChanged += ChangedOnPropertyChanged;
        }

        private void ChangedOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (_name.Equals(propertyChangedEventArgs.PropertyName))
            {
                if (_action != null)
                {
                    _action();
                }
            }
        }

        public void Call(Action action)
        {
            _action = action;
        }
    }

    public class NotifyPropertyWrap
    {
        private readonly INotifyPropertyChanged _changed;

        public NotifyPropertyWrap(INotifyPropertyChanged changed)
        {
            _changed = changed;
        }

        public NotifyPropertyAction When(string name)
        {
            return new NotifyPropertyAction(name, _changed);
        }
    }

    public static class NotifyPropertyChangedExt
    {
        public static NotifyPropertyWrap OnPropChanged(this INotifyPropertyChanged changed)
        {
            return new NotifyPropertyWrap(changed);
        }
    }
}