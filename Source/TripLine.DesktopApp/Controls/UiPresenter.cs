using System;
using System.Collections;
using System.Collections.Generic;
using TinyIoC;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using TLine.DpSystem.Ui.Configuration.Core.Model;
    using TLine.DpSystem.Ui.Configuration.Core.Properties;
    using TLine.DpSystem.Ui.Configuration.Core.Views.Sub;
    //using TLine.PostSharp;

    [NotifyProperty(ApplyToStateMachine = false)]
    public class UiPresenter : ContentControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty MyDataContextProperty = 
            DependencyProperty.Register("MyDataContext", typeof(object), typeof(UiPresenter), 
            new FrameworkPropertyMetadata(default(object),OnMyDataContextChanged));

        [Bindable(true)]
        public object MyDataContext
        {
            get { return (object)GetValue(MyDataContextProperty); }

            set { SetValue(MyDataContextProperty, value); }
        }

        private static void OnMyDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            // should get update next when DataContext gets changed
            var ctrl = (UiPresenter)d;
            //ctrl.DataContext = e.NewValue;

            ctrl.SetUi(e.NewValue);
        }

        public void ClearContent()
        {
            if (Content is FrameworkElement)
            {
                if ((Content as FrameworkElement).DataContext is IDisposable)
                {
                    ((Content as FrameworkElement).DataContext as IDisposable).Dispose();
                }
            }
            Content = null;
        }

        private void SetUi(object newValue)
        {
            if (newValue == null)
            {
                SwitchUi(null);
                return;
            }
            if (newValue.GetType() == typeof(UiDownChannel30))
            {
                //Content = Ioc.Current.Get<ChannelDown30Info>() { DataContext = newValue}
                var channelDown30Info = TinyIoC.TinyIoCContainer.Current.Resolve<ChannelDown30Info>();
                channelDown30Info.DataContext = newValue;
                SwitchUi(channelDown30Info);
                
                //new ChannelDown30Info() { DataContext = newValue };
            }
            else if (newValue.GetType() == typeof (UiUpChannel30))
            {
                var channelUp30Info = TinyIoC.TinyIoCContainer.Current.Resolve<ChannelUp30Info>();
                channelUp30Info.DataContext = TinyIoC.TinyIoCContainer.Current.Resolve<UpstreamChannel30ViewModel>(new NamedParameterOverloads(new Dictionary<string, object>
                {
                    {"channel", newValue}
                }));//new UpstreamChannel30ViewModel(newValue as UiUpChannel30);
                SwitchUi(channelUp30Info);
            }
            else if (newValue.GetType() == typeof (UiCard))
            {
                var cardInfoViewModel = TinyIoC.TinyIoCContainer.Current.Resolve<CardInfoViewModel>();
                cardInfoViewModel.Card = (UiCard)newValue;
                cardInfoViewModel.Initialize();
                SwitchUi(new CardInfo() {DataContext = cardInfoViewModel});
            }
            else if (newValue.GetType() == typeof (UiDownChannel31))
            {
                SwitchUi(new ChannelDown31Info() { DataContext = newValue });
            }
            else if (newValue.GetType() == typeof (UiUpChannel31))
            {
                SwitchUi(new ChannelUp31Info() { DataContext = newValue });
            }

        }

        private void SwitchUi(object newDataContent)
        {
            if (Content is FrameworkElement)
            {
                if ((Content as FrameworkElement).DataContext is IDisposable)
                {
                    ((Content as FrameworkElement).DataContext as IDisposable).Dispose();
                }
            }
            Content = newDataContent;
        }

        public UiPresenter()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}