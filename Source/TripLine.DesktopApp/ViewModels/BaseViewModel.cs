using System.Threading.Tasks;

namespace TripLine.DesktopApp.ViewModels
{
   
    using System.ComponentModel;
    using System.Runtime.CompilerServices;


    using TripLine.WPF.MVVM;

    public class BaseViewModel : IViewModel, INotifyPropertyChanged
    {
        public string Name { get; protected set; }

        public INavigator Navigator { get; set; }

        public BaseViewModel(string name)
        {
            Name = name;
        }

        public virtual void BeforeNavigationTo()
        {
            
        }

#pragma warning disable 1998 // It is ok to have an async method that does not have any await. Derived classes will have await.
        public async virtual Task<bool> NavigatedTo(object param)
        {
            // Set the correct tab. 
            return true;
        }
#pragma warning restore 1998

        public virtual bool BeforeNavigationAway()
        {
            return true;
        }

        public virtual void NavigatedAway()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnOtherPropertyChanged( string propertyName )
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}