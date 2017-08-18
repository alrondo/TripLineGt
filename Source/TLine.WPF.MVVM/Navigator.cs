using System.Threading.Tasks;

namespace TripLine.WPF.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;

    public class NavigationStamp
    {
        public Type ViewModel { get; set; }
        public object Param { get; set; }
        public object Tag { get; set; }
    }

    public class Navigator : INavigator
    {
        private Navigator() { }

        private Stack<NavigationStamp> _breadcrumb = new Stack<NavigationStamp>();

        private IViewModel _current = null;

        private IView _currentView = null;

        private NavigationStamp _currentStamp = null;

        private MvvmBootstrap _bootstrap;

        private MvvmConfiguration _mvvmConfiguration;

        public Action<NavigationStamp> OnCreateStamp { get; set; }

        public Action<NavigationStamp> OnPopStamp { get; set; }

        /// <summary>
        /// Event triggerred when navigation is completed. The bool arg represent the result of the operation
        /// </summary>
        public event EventHandler<bool> NavigationFinished;

        public IView CurrentView 
        {
            get
            {
                return _currentView;
            }
        }
        public MvvmConfiguration Configuration
        {
            get
            {
                return _mvvmConfiguration;
            }
        }

        public bool IgnoreView { get; set; }

        public static Navigator Load(MvvmBootstrap bootstrap = null)
        {
            var navigator = new Navigator();

            navigator._bootstrap = bootstrap;
            if (bootstrap == null)
            {
                // Get the bootstrapper
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type =
                        assembly.GetExportedTypes()
                            .FirstOrDefault(
                                t =>
                                typeof(MvvmBootstrap).IsAssignableFrom(t) && t.IsClass && t != typeof(MvvmBootstrap));

                    // We have found a bootstrap overwrite. Execute it. 
                    if (type != null)
                    {
                        navigator._bootstrap = (MvvmBootstrap)Activator.CreateInstance(type);
                    }
                }
            }

            if (navigator._bootstrap != null)
            {
                navigator._mvvmConfiguration = new MvvmConfiguration();
                navigator._bootstrap.Configure(navigator._mvvmConfiguration);

                navigator.Register<IView>();
                navigator.Register<IViewModel>();
                navigator._mvvmConfiguration.IoC.Register<INavigator>(navigator);
            }

            return navigator;
        }

        public bool HasBreadCrumb()
        {
            return _breadcrumb.Count > 0;
        }

        public async Task<bool> NavigateTo(Type type, object param = null)
        {
            return await NavigateTo(type, param, true);
        }


        public IView GetView(Type type)
        {
            return _mvvmConfiguration.IoC.Resolve(type) as IView;
        }

        public IViewModel GetViewModel(Type type)
        {
            var view = GetView(type);
            var viewmodel = GetViewModelForView(view);

            return viewmodel;
        }



        private async Task<bool> NavigateTo(Type type, object param, bool push)
        {

            // Do we already have a ViewModel? If so we check if we can navigate await. 
            // This logic is there to add the possibility of blocking navigation if data is dirty. 
            if (_current != null)
            {
                if (!_current.BeforeNavigationAway())
                {
                    return false;
                }
            }

            IViewModel vm = null;

            var oldView = _currentView;

            // Create an instance of the object type passed in the navigation. 
            //var vobj = _mvvmConfiguration.IoC.Resolve(type); 
            if (typeof(IView).IsAssignableFrom(type) && !IgnoreView)
            {
                // if it's a View, we get the corresponding ViewModel. 
                _currentView = _mvvmConfiguration.IoC.Resolve(type) as IView;
                vm = GetViewModelForView(_currentView);
            }
            else
            {
                // It was a ViewModel, let's get the corresponding View. 
                vm = (IViewModel)_mvvmConfiguration.IoC.Resolve(type);
                if (!IgnoreView)
                    _currentView = GetViewFromViewModel(vm);
            }

            if (vm == null || (_currentView == null && !IgnoreView))
            {
                throw new ArgumentException("Type is not a navigation type for this framework");
            }

        
            // We set the ViewModel as the DataContext of the View for bindings. 
            if (!IgnoreView)
            {
                (_currentView as UserControl).DataContext = vm;
            }

            // Add Navigator to both 
            if (!IgnoreView)
            {
                _currentView.Nav = this;
            }
            vm.Navigator = this;

            // If View has a ModelView property, then set it. 
            if (!IgnoreView)
            {
                var prop = _currentView.GetType().GetProperty("ModelView");
                if (prop != null) prop.SetValue(_currentView, vm);
            }

            vm.BeforeNavigationTo();

            if (_current != null && push)
            {

                _breadcrumb.Push(_currentStamp);
            }


            if (_current != null)
                _current.NavigatedAway();

            if (oldView != null)
                oldView.ViewUnloaded();

            _mvvmConfiguration.SetOnNavigation(_currentView, vm);

            if (!await vm.NavigatedTo(param))
            {
                _currentView = null;
                _current = null;
                await GoBack();
                return false;
            }
            if (!IgnoreView)
            {
                _currentView.ViewLoaded();
            }
            if (OnCreateStamp != null) OnCreateStamp(_currentStamp);
            _currentStamp = new NavigationStamp() { Param = param, ViewModel = vm.GetType() };

            _current = vm;

            //trigger navigation finished event with success value
            if (NavigationFinished != null) NavigationFinished(this, true);

            return true;
        }


        public async Task GoBack()
        {
            if (!_breadcrumb.Any())
                return;

            var stamp = _breadcrumb.Pop();
            if (OnPopStamp != null) OnPopStamp(stamp);

            await NavigateTo(stamp.ViewModel, stamp.Param, false);
        }

        private void Register<T>()
        {
            var types = new List<Type>();
            foreach (var assembly in _bootstrap.LoadAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetExportedTypes().Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.ContainsGenericParameters && !t.Namespace.Contains("TripLine.WPF.MVVM")));

                }
                catch 
                {
                    // Ignore because of exception concerning dynamic assemblies
                }
            }

            foreach (var type in types)
            {
                _mvvmConfiguration.IoC.Register(type);
            }

        }


        private IEnumerable<Type> FindFromAssemblies<T>()
        {
            var types = new List<Type>();
            foreach (var assembly in _bootstrap.LoadAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetExportedTypes().Where(t => typeof(T).IsAssignableFrom(t) && t.IsClass && !t.ContainsGenericParameters && !t.Namespace.Contains("TripLine.WPF.MVVM")));

                }
                catch
                {
                    // Ignored because there are some dynamic assemblies which cannot be reflected. 
                }
            }

            return types;
        }

        private IView GetViewFromViewModel(IViewModel vm)
        {
            var views = FindFromAssemblies<IView>();

            var viewType = views.FirstOrDefault(v => v.GetInterface(typeof(IViewFor<>).Name).GetGenericArguments().First() == vm.GetType());
            var view = (IView)_mvvmConfiguration.IoC.Resolve(viewType);
            return view;
        }

        private IViewModel GetViewModelForView(IView view)
        {
            var modelViewType = view.GetType().GetInterface(typeof(IViewFor<>).Name).GetGenericArguments().First();
            var vm = _mvvmConfiguration.IoC.Resolve(modelViewType);

            return (IViewModel) vm;

        }


        public async void GoHomeAsync()
        {
            if (_breadcrumb.Count == 0) return;
            var home = _breadcrumb.ToArray()[_breadcrumb.Count - 1];

            _breadcrumb.Clear();
            await NavigateTo(home.ViewModel, home.Param, false);
        }
    }
}
