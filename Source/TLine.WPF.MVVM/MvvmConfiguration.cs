namespace TripLine.WPF.MVVM
{
    using System;

    using TinyIoC;

    public class MvvmConfiguration
    {
        public event Action<IView, IViewModel> OnNavigation;

        public TinyIoCContainer IoC { get; private set; }

        public MvvmConfiguration()
        {
            IoC = TinyIoCContainer.Current;
        }

        public void SetOnNavigation(IView view, IViewModel model)
        {
            if (OnNavigation != null)
            {
                OnNavigation(view, model);
            }
        }
    }
}