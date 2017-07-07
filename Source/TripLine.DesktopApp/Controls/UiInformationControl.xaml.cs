
using TLine.DpSystem.Ui.Configuration.Core.Converters;
using log4net;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using TLine.DpSystem.Ui.Configuration.Core.Attributes;
    using TLine.PostSharp;
    using TLine.Toolbox;
    using TLine.Toolbox.Extensions;
    using TLine.Toolbox.UI;


    public class UiInformationException : Exception
    {
        public UiInformationException(string message) : base(message)
        {

        }
    }


    /// <summary>
    /// Interaction logic for UiInformationControl.xaml
    /// </summary>
    [NotifyProperty(ApplyToStateMachine = false)]
    public partial class UiInformationControl : UserControl, INotifyPropertyChanged
    {
        private class UiInfo
        {
            public UiInformationAttribute Attribute { get; set; }
            public PropertyInfo Prop { get; set; }
        }

        private static ILog _log = LogManager.GetLogger(typeof (UiInformationControl));

        private List<Editor> _editors = new List<Editor>();

        public static readonly DependencyProperty ItemSizeProperty = DependencyProperty.Register(
    "ItemSize", typeof(int), typeof(UiInformationControl), new PropertyMetadata(25, OnItemSizeCallback));

        private static void OnItemSizeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }

        public int ItemSize
        {
            get
            {
                return (int)GetValue(ItemSizeProperty);
            }
            set
            {
                SetValue(ItemSizeProperty, value);
            }
        }

        public static readonly DependencyProperty ItemToShowProperty = DependencyProperty.Register(
    "ItemToShow", typeof(object), typeof(UiInformationControl), new PropertyMetadata(default(object), OnItemToShowCallback));

        private static void OnItemToShowCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (UiInformationControl)d;
            ctrl.Disect();
        }

        public object ItemToShow
        {
            get { return GetValue(ItemToShowProperty); }
            set { SetValue(ItemToShowProperty, value); }
        }

        public UiInformationControl()
        {
            InitializeComponent();
        }


        private void Disect()
        {
            stackVals.Children.Clear();
            stackKeys.Children.Clear();

            List<UiInfo> infos = new List<UiInfo>();
            ItemToShow.GetType().GetProperties().ForEach(x =>
            {
                try
                {
                    var ui = x.GetCustomAttribute(typeof(UiInformationAttribute), true);
                    if (ui != null)
                    {
                        var item = ui as UiInformationAttribute;
                        bool isVisible = true;
                        if (!string.IsNullOrEmpty(item.IsVisible))
                        {
                            // must look for function
                            var method = ItemToShow.GetType().GetMethod(item.IsVisible, BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method == null)
                            {
                                var message =
                                    StringExt.Format("Missing method: {0} for UiInformationAttribute in class {1}", item.IsVisible,
                                        ItemToShow.GetType().Name);
                                _log.Error(message);
                                throw new UiInformationException(message);
                            }
                            if (method.ReturnType != typeof(bool))
                            {
                                var message =
                                    StringExt.Format("UiInformationAttribute.IsVisible: {0} method must return a bool", item.IsVisible);
                                _log.Error(message);
                                throw new UiInformationException(message);
                            }
                            isVisible = (bool)method.Invoke(ItemToShow, null);
                        }

                        if (isVisible)
                        {
                            infos.Add(new UiInfo() { Attribute = item, Prop = x });
                        }
                    }

                }
                catch (Exception pokemon)
                {
                    _log.Error(pokemon.Message, pokemon);
                    throw;
                }
            });

            infos.Sort(
                (info, uiInfo) =>
                    {
                        if (info.Attribute.Position < uiInfo.Attribute.Position) return -1;
                        if (info.Attribute.Position > uiInfo.Attribute.Position) return 1;
                        return 0;
                    });

            foreach (var uiInfo in infos)
            {
                
                var labelKey = new StackPanel() { Height = ItemSize, HorizontalAlignment = HorizontalAlignment.Left };

                string name = uiInfo.Attribute.Name;
                if (name == null)
                {
                    var nameProperty = ItemToShow.GetType().GetProperty(uiInfo.Attribute.NameProperty, BindingFlags.Instance | BindingFlags.NonPublic);
                    name = (string) nameProperty.GetMethod.Invoke(ItemToShow, null);
                }

                labelKey.Children.Add(new Label()
                {
                    Content = name,
                    Style = (FindResource("LabelNamePanelList") as Style)
                }); 
                stackKeys.Children.Add(labelKey);
                var binding = new Binding(uiInfo.Prop.Name);
                binding.Source = ItemToShow;

                if (uiInfo.Prop.PropertyType.IsEnum)
                {
                    binding.Converter = new EnumToStringConverter();
                }

                var child = new Label() { Style = (FindResource("LabelValuePanelList") as Style), HorizontalAlignment = HorizontalAlignment.Left };
                var labelVal = new StackPanel() { Height = ItemSize, HorizontalAlignment = HorizontalAlignment.Left };
                labelVal.Children.Add(child);
                child.SetBinding(Label.ContentProperty, binding);
                stackVals.Children.Add(labelVal);
                if (uiInfo.Attribute.Editor != null)
                {
                    var control = Activator.CreateInstance(uiInfo.Attribute.Editor) as Control;

                    if(control != null)
                    {
                        control.HorizontalAlignment = HorizontalAlignment.Left;
                        control.VerticalAlignment = VerticalAlignment.Center;
                        control.FontSize = 20;
                        control.Height = ItemSize;
                    }

                    if (control is ComboBox && uiInfo.Prop.PropertyType.IsEnum)
                    {
                        var converter = new EnumToStringConverter();
                        foreach (var val in uiInfo.Prop.PropertyType.GetEnumValues())
                        {
                            var convertedVal = converter.Convert(val, null, null, null);
                            (control as ComboBox).Items.Add(convertedVal);
                        }
                    }
                    Editor editor = child.BindEditor().Set(control);
                    editor.OnEditorShow += EditorOnOnEditorShow;
                    _editors.Add(editor);
                }
            }
        }

        private void EditorOnOnEditorShow(Editor editor)
        {
            IEnumerable<Editor> enumerable = _editors.Where(e => e.ShowingEditor).ToList();
            if (enumerable.Count() > 1)
            {
                enumerable.ForEach(
                    e =>
                        {
                            if (e != editor)
                            {
                                e.SwapToOriginal();
                            }
                        });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
