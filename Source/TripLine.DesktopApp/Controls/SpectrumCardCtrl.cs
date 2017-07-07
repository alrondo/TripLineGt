using System.Collections.Specialized;
using TLine.DpSystem.Dto;

namespace TLine.DpSystem.Ui.Configuration.Core.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using TLine.DpSystem.Ui.Configuration.Core.Model;
    using TLine.Toolbox.Extensions;
    using TLine.Toolbox.UI;

    public class SpectrumCardCtrl : Grid
    {

        private class CardControl : Border
        {
            private UiCard _uiCard;

            private List<Rectangle> _rects;

            public event Action<UiCard> OnClick;

            public bool IsSelected { get; set; }

            public UiCard Card{get
            {
                return _uiCard;
            }}

            public CardControl(UiCard uiCard)
            {
                _uiCard = uiCard;
                _uiCard.Channels.CollectionChanged += (sender, args) => HookUpChannels(args);
                _uiCard.Channels.ForEach(c=>c.PropertyChanged += ChannelPropChanged);
                _rects = new List<Rectangle>();//[_uiCard.Channels.Count];
                Loaded += OnLoaded;
                
            }

            private void HookUpChannels(NotifyCollectionChangedEventArgs args)
            {
                if (args.OldItems != null)
                {
                    foreach (var oldItem in args.OldItems)
                    {
                        (oldItem as IUiChannel).PropertyChanged -= ChannelPropChanged;
                    }
                }

                if (args.NewItems != null)
                {
                    foreach (var newItem in args.NewItems)
                    {
                        (newItem as IUiChannel).PropertyChanged += ChannelPropChanged;
                    }
                }

                RefreshUi();
            }

            private void ChannelPropChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
            {
                Dispatcher.Invoke(() => RefreshUi());
            }

            protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
            {
                base.OnRenderSizeChanged(sizeInfo);
                RefreshUi();
            }

            protected override void OnMouseEnter(MouseEventArgs e)
            {
                base.OnMouseEnter(e);
                Background = Application.Current.Resources["SpectrumCardUnPressedBrush"] as Brush;
            }

            protected override void OnMouseLeave(MouseEventArgs e)
            {
                base.OnMouseLeave(e);
                if (IsSelected)
                    Background = Application.Current.Resources["SelectedSpectrumCardBackground"] as Brush;
                else
                {
                    Background = Application.Current.Resources["SpectrumCardBackground"] as Brush;
                }

            }

            protected override void OnMouseDown(MouseButtonEventArgs e)
            {
                base.OnMouseDown(e);
                Background = Application.Current.Resources["SpectrumCardPressedBrush"] as Brush;
            }

            protected override void OnMouseUp(MouseButtonEventArgs e)
            {
                base.OnMouseUp(e);
                Background = Application.Current.Resources["SpectrumCardUnPressedBrush"] as Brush;

                if (OnClick != null) OnClick(_uiCard);
            }

            private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
            {
                Loaded -= OnLoaded;

                BorderThickness = new Thickness(1);
                CornerRadius = new CornerRadius(3);
                Background = Application.Current.Resources["SpectrumCardBackground"] as Brush;
                RefreshUi();
            }

            private void RefreshUi()
            {
                lock (_rects)
                {
                    _rects.Clear();
                    var grid = new Grid();
                    grid.CreateRows("50*", "50*");

                    grid.Cell().Row(1)
                        .AddUi(
                            new Label()
                            {
                                Content = _uiCard.CenterFrequency,
                                FontWeight = FontWeights.Black,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Foreground = Application.Current.Resources["SpectrumCardTextBrush"] as Brush 
                            })
                        .AddUi(
                            new Label()
                            {
                                Content = _uiCard.Name,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Foreground = Application.Current.Resources["SpectrumCardNameBrush"] as Brush 
                            })
                        .AddUi(
                            new Label()
                            {   
                                //This is a shortcut to display the stream direction without picture using a webding character
                                FontFamily = new FontFamily("Webdings"),
                                FontSize = 16,
                                Content = (_uiCard.StreamDirection == StreamDirectionEnum.Down) ? "6" : "5", 
                                HorizontalAlignment = HorizontalAlignment.Right ,
                                Foreground = Application.Current.Resources["SpectrumCardNameBrush"] as Brush 
                            }
                        );

                    grid.Cell().Row(0).AddUi(
                        new CustomCanvas(
                            canvas =>
                                {
                                    int i = 0;
                                    Trace.WriteLine(StringExt.Format("Create: {0} - {1}", _uiCard.Name, _uiCard.Channels.Count));

                                    foreach (var channel in _uiCard.Channels)
                                    {
                                        var rect = new Rectangle()
                                                       {
                                                           Fill = ChannelColors.GetChannelColor(channel),
                                                           Margin = new Thickness(0, 3, 0, 3),
                                                           Stroke = Application.Current.Resources["SpectrumCardChannelStateBorderBrush"] as Brush
                                                       };
                                        _rects.Add(rect);
                                        canvas.Children.Add(rect);
                                        i += 1;
                                    }
                                    return true;
                                },
                            (c) =>
                                {
                                    int i = 0;
                                    foreach (var channel in _uiCard.Channels)
                                    {
                                        Trace.WriteLine(
                                            StringExt.Format("Positioning : {0} - {1}", _uiCard.Name, _uiCard.Channels.Count));

                                        var px = c.ActualWidth / 200
                                                 * (channel.CenterFreq - (_uiCard.CenterFrequency - 100)
                                                    - (channel.Bandwidth / 2));
                                        var size = c.ActualWidth / 200 * channel.Bandwidth;

                                        (c.Children[i] as Rectangle).Width = size;
                                        (c.Children[i] as Rectangle).Height = c.ActualHeight;
                                        Canvas.SetLeft(c.Children[i], px);
                                        Canvas.SetTop(c.Children[i], 0);
                                        i += 1;
                                        Trace.WriteLine(StringExt.Format("Positioning : {0} - {1}", _uiCard.Name, px));

                                    }
                                }));
                    Trace.WriteLine(StringExt.Format("{0} - {1}", _uiCard.Name, _uiCard.Channels.Count));
                    Child = grid;
                    if (ActualHeight < 50)
                    {
                        grid.RowDefinitions[0].Height = new GridLength(0);
                    }

                    InvalidateVisual();
                }
            }
        }

        private class CustomCanvas : Canvas
        {

            private readonly Func<CustomCanvas, bool> _create;

            private readonly Action<CustomCanvas> _position;


            private bool _created = false;

            public CustomCanvas(Func<CustomCanvas, bool> create, Action<CustomCanvas> position)
            {
                _create = create;
                _position = position;
                _created = _create(this);
                Loaded += OnLoaded;
            }

            private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                
            }

            private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
            {
                Loaded -= OnLoaded;

                if (!_created)
                {
                    _created = _create(this);

                } CheckPos();

            }

            protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
            {
                base.OnRenderSizeChanged(sizeInfo);
                if (!_created)
                {
                    _created = _create(this);

                }
                CheckPos();
            }

            private void CheckPos()
            {
                _position(this);
            }
        }

        private List<CardControl> _cardControls = new List<CardControl>();

        public static readonly DependencyProperty CardsProperty = DependencyProperty.Register(
                "Cards", typeof(object), typeof(SpectrumCardCtrl), new PropertyMetadata(default(object), CardsCallback));

        public static readonly DependencyProperty SelectedCardProperty = DependencyProperty.Register(
        "SelectedCard", typeof(UiCard), typeof(SpectrumCardCtrl), new PropertyMetadata(default(UiCard), SelectedCardCallback));

        public UiCard SelectedCard
        {
            get
            {
                return (UiCard)GetValue(SelectedCardProperty);
            }
            set
            {
                SetValue(SelectedCardProperty, value);
            }
        }

        private static void SelectedCardCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (SpectrumCardCtrl)d;
            ctrl.ManageSelected(e.OldValue.CastTo<UiCard>(), e.NewValue.CastTo<UiCard>());

        }

        private void ManageSelected(UiCard oldValue, UiCard newValue)
        {
            try
            {
                if (oldValue == newValue || newValue == null) return;
                if (oldValue == null)
                {
                        
                    for (var i = 0; i < RowDefinitions.Count; i++)
                    {
                        RowDefinitions[i].Height = new GridLength(25);
                    }
                }
                else
                {
                    var cardControl = _cardControls.FirstOrDefault(c => c.Card.Name.Equals(oldValue.Name));
                    if (cardControl == null) return;
                    var cardCanvas = Children.Cast<UIElement>().Where(c => c is CustomCanvas).Cast<CustomCanvas>().FirstOrDefault(canvas => canvas.Children.Cast<CardControl>().Any(cc => cc.Card.Name.Equals(cardControl.Card.Name)));
                    if (cardCanvas != null)
                    {
                        var row = GetRow(cardCanvas);
                        RowDefinitions[row].Height = new GridLength(25);
                    }

                }

                if (_cardControls.Count == 0) return;
                var cardcontrol = _cardControls.FirstOrDefault(c => c.Card.Name.Equals(newValue.Name));
                if (cardcontrol == null) return;

                var cCanvas = Children.Cast<UIElement>().Where(c => c is CustomCanvas).Cast<CustomCanvas>().FirstOrDefault(canvas => canvas.Children.Cast<CardControl>().Any(cc => cc.Card.Name.Equals(cardcontrol.Card.Name)));
                if (cCanvas != null)
                {
                    var row = GetRow(cCanvas);
                    RowDefinitions[row].Height = new GridLength(50);
                }
            }
            catch
            {
                
                
            }
        }

        private static void CardsCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var ctrl = (SpectrumCardCtrl)d;
            ctrl.Watch();
            ctrl.SetUI();
        }


        private void Watch()
        {
            InternalCards.CollectionChanged += (sender, args) =>
            {
                SelectedCard = null;
                RefreshUi(null, null);

                InternalCards.ForEach(c => c.PropertyChanged -= RefreshUi);
                InternalCards.ForEach(c => c.PropertyChanged += RefreshUi);

            };

            InternalCards.ForEach(c => c.PropertyChanged -= RefreshUi);
            InternalCards.ForEach(c => c.PropertyChanged += RefreshUi);
        }

        private void RefreshUi(object sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {


                SetUI();
                RefreshSelectedCard();
            });
        }

        private void RefreshSelectedCard()
        {
            if (SelectedCard == null) return;
            var ca = Cards.CastTo<ObservableCollection<UiCard>>().FirstOrDefault(u => u.Name.Equals(SelectedCard.Name));
            if (ca != null) SelectedCard = ca;
        }

        public object Cards
        {
            get
            {
                return GetValue(CardsProperty);
            }
            set
            {
                SetValue(CardsProperty, value);
            }
        }

        private ObservableCollection<UiCard> InternalCards
        {
            get
            {
                return Cards as ObservableCollection<UiCard>;
            }
        }

        public event Action<UiCard> OnCardSelected;

        public void SetCards(ObservableCollection<UiCard> cards)
        {
            Cards = cards;

            SetUI();
        }

        private void SetUI()
        {
            this.Dispatcher.Invoke(() =>
            {


                ColumnDefinitions.Clear();
                RowDefinitions.Clear();
                Children.Clear();
                _cardControls.Clear();

                // Set rows
                ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(2, GridUnitType.Star)});
                ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(40, GridUnitType.Star)});
                ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(40, GridUnitType.Star)});
                ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(2, GridUnitType.Star)});

                int row = 0;

                foreach (var cardIo in InternalCards)
                {
                    RowDefinitions.Add(new RowDefinition() {Height = new GridLength(50)});

                    // Create Background
                    var rect = new Rectangle()
                    {
                        Fill = Application.Current.Resources["SpectrumCardSide"] as Brush,
                        Margin = new Thickness(2)
                    };
                    SetColumn(rect, 0);
                    SetRow(rect, row);
                    Children.Add(rect);

                    rect = new Rectangle()
                    {
                        Fill = Application.Current.Resources["SpectrumCardDarkBackground"] as Brush,
                        Margin = new Thickness(2)
                    };
                    SetColumn(rect, 1);
                    SetRow(rect, row);
                    Children.Add(rect);

                    rect = new Rectangle()
                    {
                        Fill = Application.Current.Resources["SpectrumCardDarkBackground"] as Brush,
                        Margin = new Thickness(2)
                    };
                    SetColumn(rect, 2);
                    SetRow(rect, row);
                    Children.Add(rect);

                    rect = new Rectangle()
                    {
                        Fill = Application.Current.Resources["SpectrumCardSide"] as Brush,
                        Margin = new Thickness(2)
                    };
                    SetColumn(rect, 3);
                    SetRow(rect, row);
                    Children.Add(rect);

                    var control = new CardControl(cardIo);
                    control.OnClick += card =>
                    {
                        if (OnCardSelected != null) OnCardSelected(card);
                        SelectedCard = card;
                    };
                    _cardControls.Add(control);
                    // Create Canvases
                    var canvas = new CustomCanvas(
                        (c) =>
                        {

                            c.Children.Add(control);
                            return true;
                        },
                        (c) =>
                        {
                            var px = c.ActualWidth/2000*(cardIo.CenterFrequency - 100);
                            var size = c.ActualWidth/2000*200;

                            control.Width = size;
                            control.Height = c.ActualHeight;
                            Canvas.SetLeft(control, px);
                            Canvas.SetTop(control, 0);

                        });
                    SetRow(canvas, row);
                    SetColumn(canvas, 1);
                    SetColumnSpan(canvas, 2);
                    Children.Add(canvas);



                    row += 1;

                }
                RowDefinitions.Add(new RowDefinition() {Height = new GridLength(25)});
                var zero = new Label {Content = "0 Mhz", HorizontalAlignment = HorizontalAlignment.Left};
                Children.Add(zero);
                SetColumn(zero, 1);
                SetRow(zero, row);

                var half = new Label
                {
                    Content = "1000 Mhz",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(-25, 0, 0, 0)
                };
                Children.Add(half);
                SetColumn(half, 2);
                SetRow(half, row);

                var full = new Label {Content = "2000 Mhz", HorizontalAlignment = HorizontalAlignment.Right};
                Children.Add(full);
                SetColumn(full, 2);
                SetRow(full, row);


                ManageSelected(null, SelectedCard);
            });
        }
    }
}