// Copyright 2017 (C) Cody Neuburger  All rights reserved.
// References:
// http://jigneshon.blogspot.com/2013/11/c-wpf-tutorial-implementing-iscrollinfo.html
// https://blogs.msdn.microsoft.com/bencon/2006/12/09/iscrollinfo-tutorial-part-iv/
// http://jobijoy.blogspot.com/2008/04/simple-radial-panel-for-wpf-and.html
// https://www.codeproject.com/Articles/15705/FishEyePanel-FanPanel-Examples-of-custom-layout-pa
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimekeeperWPF.Tools;
using TimekeeperWPF;
using System.Reflection;
using System.Windows.Threading;
using System.Collections;
using TimekeeperDAL.Tools;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace TimekeeperWPF.Calendar
{
    public class Day : Panel, IScrollInfo
    {
        #region Constructor
        static Day()
        {
            BackgroundProperty.OverrideMetadata(typeof(Day),
                new FrameworkPropertyMetadata(Brushes.MintCream,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
            _Timer = new DispatcherTimer();
            _Timer.Interval = TimeSpan.FromSeconds(1);
            _Timer.Start();
        }
        public Day() : base()
        {
            _Timer.Tick += _Timer_Tick;
        }
        #endregion
        #region Events
        private void _Timer_Tick(object sender, EventArgs e)
        {
            InvalidateArrange();
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            DeterminePosition(e);
        }
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            MouseClickPosition = MousePosition;
            var pt = e.GetPosition(this);
            ItemsUnderMouse.Clear();
            VisualTreeHelper.HitTest(this, null,
                new HitTestResultCallback(DetectCalendarObjects),
                new PointHitTestParameters(pt));
            base.OnPreviewMouseRightButtonDown(e);
        }
        private HitTestResultBehavior DetectCalendarObjects(HitTestResult result)
        {
            if (result.VisualHit is FrameworkElement)
            {
                var FE = result.VisualHit as FrameworkElement;
                var DC = FE.DataContext;
                if (DC is CalendarObject)
                {
                    var CO = DC as CalendarObject;
                    if (!ItemsUnderMouse.Contains(CO))
                        ItemsUnderMouse.Add(CO);
                }
            }
            return HitTestResultBehavior.Continue;
        }
        protected virtual void DeterminePosition(MouseEventArgs e)
        {
            if (Orientation == Orientation.Vertical)
            {
                var pos = e.GetPosition(this);
                var date = Date;
                var seconds = (int)((pos.Y + Offset.Y) * Scale).Within(0, _DAY_SECONDS);
                var time = new TimeSpan(0, 0, seconds);
                MousePosition = date + time;
            }
            else
            {
                var pos = e.GetPosition(this);
                var date = Date;
                var seconds = (int)((pos.X + Offset.X) * Scale).Within(0, _DAY_SECONDS);
                var time = new TimeSpan(0, 0, seconds);
                MousePosition = date + time;
            }
        }
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (e.Handled) return;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;
                if (e.Delta < 0) TryScaleUp();
                else TryScaleDown();
            }
            else
            {
                if (Orientation == Orientation.Vertical)
                {
                    e.Handled = true;
                    if (e.Delta < 0) MouseWheelDown();
                    else MouseWheelUp();
                }
                else
                {
                    e.Handled = true;
                    if (e.Delta > 0) MouseWheelLeft();
                    else MouseWheelRight();
                }
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            CoerceValue(ScaleProperty);
            base.OnRenderSizeChanged(sizeInfo);
        }
        #endregion Events
        #region Features
        #region Animation
        internal static DispatcherTimer _Timer;
        internal static TimeSpan _AnimationLength = TimeSpan.FromMilliseconds(500);
        internal static double _AccelerationRatio = 0.2d;
        internal static double _DecelerationRatio = 0.8d;
        #endregion
        #region Date
        /// <summary>
        /// The selected date.
        /// </summary>
        [Bindable(true)]
        public DateTime Date
        {
            get { return (DateTime)GetValue(DateProperty); }
            set { SetValue(DateProperty, value); }
        }
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(
                nameof(Date), typeof(DateTime), typeof(Day),
                new FrameworkPropertyMetadata(DateTime.Now.Date,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));
        [Bindable(true)]
        public int SelectedMonthOverride
        {
            get { return (int)GetValue(SelectedMonthOverrideProperty); }
            set { SetValue(SelectedMonthOverrideProperty, value); }
        }
        public static readonly DependencyProperty SelectedMonthOverrideProperty =
            DependencyProperty.Register(
                nameof(SelectedMonthOverride), typeof(int), typeof(Week),
                new FrameworkPropertyMetadata(DateTime.Now.Month,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        protected virtual bool IsDateTimeRelevant(DateTime d) { return d.Date == Date; }
        protected bool IsCalObjRelevant(CalendarTaskObject CalObj)
        { return IsDateTimeRelevant(CalObj.Start) || IsDateTimeRelevant(CalObj.End); }
        #endregion
        #region ForceMaxScale
        /// <summary>
        /// Force Day to fit the available space.
        /// </summary>
        [Bindable(true)]
        public bool ForceMaxScale
        {
            get { return (bool)GetValue(ForceMaxScaleProperty); }
            set { SetValue(ForceMaxScaleProperty, value); }
        }
        public static readonly DependencyProperty ForceMaxScaleProperty =
            DependencyProperty.Register(
                nameof(ForceMaxScale), typeof(bool), typeof(Day),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnForceMaxScaleChanged)));
        private static void OnForceMaxScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ScaleProperty);
            d.CoerceValue(OffsetProperty);
        }
        #endregion
        #region Foreground
        [Bindable(true), Category("Appearance")]
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }
        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(
                typeof(Day),
                new FrameworkPropertyMetadata(Brushes.Black,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region FontFamily
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }
        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(
                typeof(Day),
                new FrameworkPropertyMetadata(new FontFamily("Segoe UI"),
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontFamilyChanged)));
        private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(TextMarginProperty);
        }
        #endregion
        #region FontSize
        [TypeConverter(typeof(FontSizeConverter))]
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(
                typeof(Day),
                new FrameworkPropertyMetadata(12d,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontSizeChanged)));
        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(TextMarginProperty);
        }
        #endregion
        #region FontStretch
        [Bindable(true), Category("Appearance")]
        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }
        public static readonly DependencyProperty FontStretchProperty
            = TextElement.FontStretchProperty.AddOwner(
                typeof(Day),
                new FrameworkPropertyMetadata(FontStretches.Normal,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontStretchChanged)));
        private static void OnFontStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(TextMarginProperty);
        }
        #endregion
        #region FontStyle
        [Bindable(true), Category("Appearance")]
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }
        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(
                typeof(Day),
                new FrameworkPropertyMetadata(FontStyles.Normal,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontStyleChanged)));
        private static void OnFontStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(TextMarginProperty);
        }
        #endregion
        #region FontWeight
        [Bindable(true), Category("Appearance")]
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }
        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(
                typeof(Day),
                new FrameworkPropertyMetadata(FontWeights.Normal,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnFontWeightChanged)));
        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(TextMarginProperty);
        }
        #endregion
        #region GridMinorPen
        [Bindable(true)]
        public Pen GridMinorPen
        {
            get { return (Pen)GetValue(GridMinorPenProperty); }
            set { SetValue(GridMinorPenProperty, value); }
        }
        public static readonly DependencyProperty GridMinorPenProperty =
            DependencyProperty.Register(
                nameof(GridMinorPen), typeof(Pen), typeof(Day),
                new FrameworkPropertyMetadata(GetDefaultMinorPen(),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        private static Pen GetDefaultMinorPen()
        {
            Pen p = new Pen(Brushes.Gray, 1);
            p.DashStyle = DashStyles.Dash;
            return p;
        }
        #endregion
        #region GridRegularPen
        [Bindable(true)]
        public Pen GridRegularPen
        {
            get { return (Pen)GetValue(GridRegularPenProperty); }
            set { SetValue(GridRegularPenProperty, value); }
        }
        public static readonly DependencyProperty GridRegularPenProperty =
            DependencyProperty.Register(
                nameof(GridRegularPen), typeof(Pen), typeof(Day),
                new FrameworkPropertyMetadata(new Pen(Brushes.Black, 1),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region GridMajorPen
        [Bindable(true)]
        public Pen GridMajorPen
        {
            get { return (Pen)GetValue(GridMajorPenProperty); }
            set { SetValue(GridMajorPenProperty, value); }
        }
        public static readonly DependencyProperty GridMajorPenProperty =
            DependencyProperty.Register(
                nameof(GridMajorPen), typeof(Pen), typeof(Day),
                new FrameworkPropertyMetadata(new Pen(Brushes.Black, 3),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region Highlight
        [Bindable(true), Category("Appearance")]
        public bool ShowHighlight
        {
            get { return (bool)GetValue(ShowHighlightProperty); }
            set { SetValue(ShowHighlightProperty, value); }
        }
        public static readonly DependencyProperty ShowHighlightProperty =
            DependencyProperty.Register(
                nameof(ShowHighlight), typeof(bool), typeof(Day),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public Brush Highlight
        {
            get { return (Brush)GetValue(HighlightProperty); }
            set { SetValue(HighlightProperty, value); }
        }
        public static readonly DependencyProperty HighlightProperty =
            DependencyProperty.Register(
                nameof(Highlight), typeof(Brush), typeof(Day),
                new FrameworkPropertyMetadata(Brushes.LightCyan,
                    FrameworkPropertyMetadataOptions.Inherits |
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region Offset
        //final non-animated offset
        private Vector _Offset = new Vector();
        protected Vector Offset
        {
            get { return (Vector)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }
        private static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(
                nameof(Offset), typeof(Vector), typeof(Day),
                new FrameworkPropertyMetadata(new Vector(),
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnOffsetChanged),
                    new CoerceValueCallback(OnCoerceOffset)));
        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        private static object OnCoerceOffset(DependencyObject d, object value)
        {
            Day day = d as Day;
            Vector newValue = (Vector)value;
            if (day.ForceMaxScale) return new Vector();
            if (day.Orientation == Orientation.Vertical)
            {
                newValue.X = newValue.X.Within(0, day.ExtentWidth - day.ViewportWidth);
                newValue.Y = newValue.Y.Within(0, day.ExtentHeight - day.ViewportHeight);
            }
            else //(day.Orientation == Orientation.Horizontal)
            {
                newValue.X = newValue.X.Within(0, day.ExtentWidth - day.ViewportWidth);
                newValue.Y = newValue.Y.Within(day.ExtentHeight - day.ViewportHeight, 0);
            }
            return newValue;
        }
        private void AnimateOffset()
        {
            CancelScalingAnimation();
            if (Offset == _Offset) return;
            VectorAnimation anime = new VectorAnimation();
            anime.Duration = _AnimationLength;
            anime.To = new Vector(HorizontalOffset, VerticalOffset);
            anime.AccelerationRatio = _AccelerationRatio;
            anime.DecelerationRatio = _DecelerationRatio;
            BeginAnimation(OffsetProperty, anime, HandoffBehavior.Compose);
        }
        #endregion Offset
        #region Orientation
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation), typeof(Orientation), typeof(Day),
                new FrameworkPropertyMetadata(Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnOrientationChanged)),
                new ValidateValueCallback(IsValidOrientation));
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Day day = d as Day;
            day.ResetScrolling();
            day.CoerceValue(OffsetProperty);
            day.CoerceValue(ScaleProperty);
        }
        protected override bool HasLogicalOrientation => true;
        protected override Orientation LogicalOrientation => Orientation;
        internal static bool IsValidOrientation(object o)
        {
            Orientation value = (Orientation)o;
            return value == Orientation.Horizontal
                || value == Orientation.Vertical;
        }
        #endregion
        #region MousePosition
        public DateTime MousePosition
        {
            get { return (DateTime)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }
        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register(
                nameof(MousePosition), typeof(DateTime), typeof(Day));
        public DateTime MouseClickPosition
        {
            get { return (DateTime)GetValue(MouseClickPositionProperty); }
            set { SetValue(MouseClickPositionProperty, value); }
        }
        public static readonly DependencyProperty MouseClickPositionProperty =
            DependencyProperty.Register(
                nameof(MouseClickPosition), typeof(DateTime), typeof(Day));
        #endregion MousePosition
        #region ItemsUnderMouse
        public ObservableCollection<CalendarObject> ItemsUnderMouse
        {
            get { return (ObservableCollection<CalendarObject>)GetValue(ItemsUnderMouseProperty); }
            set { SetValue(ItemsUnderMouseProperty, value); }
        }
        public static readonly DependencyProperty ItemsUnderMouseProperty =
            DependencyProperty.Register(
                nameof(ItemsUnderMouse), typeof(ObservableCollection<CalendarObject>), typeof(Day),
                new FrameworkPropertyMetadata(new ObservableCollection<CalendarObject>()));
        #endregion ItemsUnderMouse
        #region Scale
        // Scale is in Seconds per Pixel s/px
        protected double ScaleFactor = 0.3d;
        protected double _ScaleLowerLimit = 0.5d;
        protected double _ScaleUpperLimit = 3600d;
        private bool _IsCancellingScaleAnimation = false;
        private Vector PreScaleRelativeOffSetInSeconds = new Vector();
        private Vector RelativeScalingVector = new Vector();
        private ICommand _ScaleUpCommand = null;
        private ICommand _ScaleDownCommand = null;
        //final non-animated scale
        private double _Scale = 60d;
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                nameof(Scale), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(60d,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnScaleChanged),
                    new CoerceValueCallback(OnCoerceScale)),
                new ValidateValueCallback(IsValidScale));
        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Day day = d as Day;
            if (day._IsCancellingScaleAnimation) return;
            Double newValue = (Double)e.NewValue;
            day.FindGridData();
            day.BeginAnimation(OffsetProperty, null);
            day._Offset = day.Offset = (day.PreScaleRelativeOffSetInSeconds / day.Scale) - day.RelativeScalingVector;
            day._DaySize = _DAY_SECONDS / day.Scale;
        }
        private static object OnCoerceScale(DependencyObject d, object value)
        {
            Day day = d as Day;
            Double newValue = (Double)value;
            if (day.ForceMaxScale || newValue > day.GetMaxScale()) newValue = day.GetMaxScale();
            if (newValue < day._ScaleLowerLimit) return day._ScaleLowerLimit;
            if (newValue > day._ScaleUpperLimit) return day._ScaleUpperLimit;
            if (Double.IsNaN(newValue)) return DependencyProperty.UnsetValue;
            return newValue;
        }
        public virtual double GetMaxScale()
        {
            if (Orientation == Orientation.Vertical) 
                return _DAY_SECONDS / RenderSize.Height;
            else 
                return _DAY_SECONDS / RenderSize.Width;
        }
        internal static bool IsValidScale(object value)
        {
            Double scale = (Double)value;
            bool result = scale > 0 && !Double.IsNaN(scale) && !Double.IsInfinity(scale);
            return result;
        }
        public ICommand ScaleUpCommand => _ScaleUpCommand
            ?? (_ScaleUpCommand = new RelayCommand(ap => ScaleUp(), pp => CanScaleUp));
        public ICommand ScaleDownCommand => _ScaleDownCommand
            ?? (_ScaleDownCommand = new RelayCommand(ap => ScaleDown(), pp => CanScaleDown));
        /// <summary>
        /// This property expects negative number to trigger scale down, positive for scale up.
        /// </summary>
        public int ScaleSudoCommand
        {
            get { return (int)GetValue(ScaleSudoCommandProperty); }
            set { SetValue(ScaleSudoCommandProperty, value); }
        }
        public static readonly DependencyProperty ScaleSudoCommandProperty =
            DependencyProperty.Register(
                nameof(ScaleSudoCommand), typeof(int), typeof(Day),
                new FrameworkPropertyMetadata(0, null,
                    new CoerceValueCallback(OnCoerceScaleSudoCommand)));
        public static object OnCoerceScaleSudoCommand(DependencyObject d, object value)
        {
            //This may be a bit unconventional, but it works great.
            //I need to redirect the ScaleUp command sent by a button on a toolbar to the VM to this panel.
            //This is done by sending the information through int properties from VM to this panel.
            Day day = d as Day;
            int i = (int)value;
            if (i < 0) day.TryScaleDown();
            else if (i > 0) day.TryScaleUp();
            return 0;
        }
        private bool CanScaleUp => !ForceMaxScale;
        private bool CanScaleDown => !ForceMaxScale;
        private void ScaleUp() { ScaleUpOrDownBy(ScaleFactor); }
        private void ScaleDown() { ScaleUpOrDownBy(-ScaleFactor); }
        private void ScaleUpOrDownBy(double ScaleFactor)
        {
            if (IsMouseOver) SetRelativeScalingVector(Mouse.GetPosition(this));
            else SetRelativeScalingVector(new Point(_Viewport.Width / 2, _Viewport.Height / 2));
            PreScaleRelativeOffSetInSeconds = (Offset + RelativeScalingVector) * Scale;
            _Scale = Scale * (1d + ScaleFactor);
            AnimateScale();
        }
        private void SetRelativeScalingVector(Point p)
        {
            Vector v = new Vector();
            if (Orientation == Orientation.Vertical)
                v.Y = p.Y;
            else
                v.X = p.X;
            RelativeScalingVector = v;
        }
        public void TryScaleUp()
        {
            if (ScaleUpCommand?.CanExecute(null) ?? false)
                ScaleUpCommand.Execute(null);
        }
        public void TryScaleDown()
        {
            if (ScaleDownCommand?.CanExecute(null) ?? false)
                ScaleDownCommand.Execute(null);
        }
        private void AnimateScale()
        {
            if (Scale == _Scale) return;
            DoubleAnimation anime = new DoubleAnimation();
            anime.Duration = _AnimationLength;
            anime.To = _Scale;
            anime.AccelerationRatio = _AccelerationRatio;
            anime.DecelerationRatio = _DecelerationRatio;
            BeginAnimation(ScaleProperty, anime, HandoffBehavior.Compose);
        }
        private void CancelScalingAnimation()
        {
            //See: https://docs.microsoft.com/en-us/dotnet/framework/wpf/graphics-multimedia/how-to-set-a-property-after-animating-it-with-a-storyboard
            //We want to update the backing value with the last animation value,
            //but we don't want to trigger OnScaleChanged stuff,
            //so we need an IsCancelling flag
            _IsCancellingScaleAnimation = true;
            //update cache with current value (which is the animation value)
            _Scale = Scale;
            //clear the animation so we can change Scale (this has the side effect of reverting Scale to before animatioon)
            BeginAnimation(ScaleProperty, null);
            //set scale to the new value we stored in cache
            Scale = _Scale;
            _IsCancellingScaleAnimation = false;
        }
        #endregion
        #region TextMargin
        private bool _ShowTextMarginPrevious = true;
        /// <summary>
        /// Show or hide the grid time text.
        /// </summary>
        [Bindable(true)]
        public bool ShowTextMargin
        {
            get { return (bool)GetValue(ShowTextMarginProperty); }
            set { SetValue(ShowTextMarginProperty, value); }
        }
        public static readonly DependencyProperty ShowTextMarginProperty =
            DependencyProperty.Register(
                nameof(ShowTextMargin), typeof(bool), typeof(Day),
                new FrameworkPropertyMetadata(true,
                    new PropertyChangedCallback(OnShowTextMarginChanged)));
        public static void OnShowTextMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Day day = d as Day;
            day.UpdateTextMargin();
        }
        private double _TextMargin = 0;
        public double TextMargin
        {
            get { return (double)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }
        public static readonly DependencyProperty TextMarginProperty =
            DependencyProperty.Register(
                nameof(TextMargin), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(0d,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));
        private void UpdateTextMargin()
        {
            if (ShowTextMargin)
            {
                string format = "";
                if (_GridData.GridTextFormat == GridTextFormat.Long) format = "00:00:00 AM";
                else if (_GridData.GridTextFormat == GridTextFormat.Medium) format = "00:00 AM";
                else if (_GridData.GridTextFormat == GridTextFormat.Short) format = "00 AM";
                else
                {
                    _TextMargin = 0;
                    AnimateTextMargin();
                    return;
                }
                FormattedText lineText = new FormattedText(format,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.RightToLeft,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    FontSize, Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                _TextMargin = lineText.Width - _TextOffset.X - _TextOffset.X;
                AnimateTextMargin();
            }
            else
            {
                _TextMargin = 0;
                AnimateTextMargin();
            }
        }
        private void AnimateTextMargin()
        {
            DoubleAnimation anime = new DoubleAnimation();
            anime.Duration = _AnimationLength;
            anime.To = _TextMargin;
            anime.AccelerationRatio = _AccelerationRatio;
            anime.DecelerationRatio = _DecelerationRatio;
            anime.Completed += OnAnimateTextMarginCompleted;
            BeginAnimation(TextMarginProperty, anime, HandoffBehavior.Compose);
        }
        private void OnAnimateTextMarginCompleted(object sender, EventArgs e)
        {
            _ShowTextMarginPrevious = ShowTextMargin;
        }
        #endregion
        #region Watermark
        public bool ShowWatermark { get; set; } = true;
        [Bindable(true)]
        public string WatermarkFormat
        {
            get { return (string)GetValue(WatermarkFormatProperty); }
            set { SetValue(WatermarkFormatProperty, value); }
        }
        public static readonly DependencyProperty WatermarkFormatProperty =
            DependencyProperty.Register(
                nameof(WatermarkFormat), typeof(string), typeof(Day),
                new FrameworkPropertyMetadata("ddd\nMMM d",
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public Brush WatermarkBrush
        {
            get { return (Brush)GetValue(WatermarkBrushProperty); }
            set { SetValue(WatermarkBrushProperty, value); }
        }
        public static readonly DependencyProperty WatermarkBrushProperty =
            DependencyProperty.Register(
                nameof(WatermarkBrush), typeof(Brush), typeof(Day),
                new FrameworkPropertyMetadata(Brushes.DarkGray,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        public FontFamily WatermarkFontFamily
        {
            get { return (FontFamily)GetValue(WatermarkFontFamilyProperty); }
            set { SetValue(WatermarkFontFamilyProperty, value); }
        }
        public static readonly DependencyProperty WatermarkFontFamilyProperty =
            DependencyProperty.Register(
                nameof(WatermarkFontFamily), typeof(FontFamily), typeof(Day),
                new FrameworkPropertyMetadata(new FontFamily("Segoe UI"),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public bool ShowMonthBoundsHighlight
        {
            get { return (bool)GetValue(ShowMonthBoundsHighlightProperty); }
            set { SetValue(ShowMonthBoundsHighlightProperty, value); }
        }
        public static readonly DependencyProperty ShowMonthBoundsHighlightProperty =
            DependencyProperty.Register(
                nameof(ShowMonthBoundsHighlight), typeof(bool), typeof(Week),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public Brush MonthBoundsHighlight
        {
            get { return (Brush)GetValue(MonthBoundsHighlightProperty); }
            set { SetValue(MonthBoundsHighlightProperty, value); }
        }
        public static readonly DependencyProperty MonthBoundsHighlightProperty =
            DependencyProperty.Register(
                nameof(MonthBoundsHighlight), typeof(Brush), typeof(Week),
                new FrameworkPropertyMetadata(Brushes.LightGray,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public Brush MonthBoundsWatermarkBrush
        {
            get { return (Brush)GetValue(MonthBoundsWatermarkBrushProperty); }
            set { SetValue(MonthBoundsWatermarkBrushProperty, value); }
        }
        public static readonly DependencyProperty MonthBoundsWatermarkBrushProperty =
            DependencyProperty.Register(
                nameof(MonthBoundsWatermarkBrush), typeof(Brush), typeof(Week),
                new FrameworkPropertyMetadata(Brushes.MintCream,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #endregion Features
        #region Layout
        public bool ShowGrid { get; set; } = true; //TODO: dep prop
        public Point _TextOffset { get; set; } = new Point(-4, 0); //TODO: dep prop
        public double _TextRotation { get; set; } = 0d; //TODO: dep prop
        public bool ShowDateLines { get; set; } = true; //TODO: dep prop
        protected double _VisibleColumns = 1;
        protected double _VisibleRows = 1;
        protected virtual DateTime _FirstVisibleDay => Date;
        protected virtual int Days => 1;
        protected virtual int GetColumn(DateTime date) { return 0; }
        protected virtual int GetRow(DateTime date) { return 0; }
        internal const int _DAY_SECONDS = 86400;
        private double _DaySize = 86400d / 60d;
        private enum GridTextFormat { Long, Medium, Short, Hide }
        private class GridData : IComparable
        {
            //See: InitializeGridData()
            //This GridData is used below this cutoff
            public double ScaleCutoff { get; set; }
            //Number of seconds between each line
            public double SecondsInterval { get; set; }
            //Draw a regular line each x interval
            public int RegularSkip { get; set; }
            //Draw a major line each x interval
            public int MajorSkip { get; set; }
            //These are toggles for each type of line
            public bool MinorGridLines { get; set; }
            public bool RegularGridLines { get; set; }
            public bool MajorGridLines { get; set; }
            //These are the formats used to write the time text
            public string MinorFormat { get; set; }
            public string RegularFormat { get; set; }
            public string MajorFormat { get; set; }
            //Used to determine size of TextMargin in UpdateTextMargin.
            //Should be representative of Minor/regular/major format
            public GridTextFormat GridTextFormat { get; set; }
            //Should the grid be drawn
            public bool DrawGrid { get; set; }

            public int CompareTo(object obj)
            {
                GridData g = obj as GridData;
                if (ScaleCutoff < g.ScaleCutoff) return -1;
                else if (ScaleCutoff > g.ScaleCutoff) return 1;
                else return 0;
            }
        }
        private GridData _GridData;
        private List<GridData> _ListOfGridDatas;
        //Number of pixels between each line
        private double _ScreenInterval;
        private int _MaxIntervals => (int)(_DAY_SECONDS / _GridData.SecondsInterval);
        private void InitializeGridData()
        {
            _ListOfGridDatas = new List<GridData>()
            {
                new GridData()
                {
                    ScaleCutoff = 1.5d,
                    SecondsInterval = 30d, //30s
                    RegularSkip = 2, //1m
                    MajorSkip = 30, //15m
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "",
                    RegularFormat = "h:mm",
                    MajorFormat = "tt h:mm",
                    GridTextFormat = GridTextFormat.Medium,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 3d,
                    SecondsInterval = 60d, //1m
                    RegularSkip = 5, //5m
                    MajorSkip = 60, //1h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "",
                    RegularFormat = "h:mm",
                    MajorFormat = "tt h:mm",
                    GridTextFormat = GridTextFormat.Medium,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 12d,
                    SecondsInterval = 300d, //5m
                    RegularSkip = 3, //15m
                    MajorSkip = 12, //1h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "mm",
                    RegularFormat = "mm",
                    MajorFormat = "tt h:mm",
                    GridTextFormat = GridTextFormat.Medium,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 24d,
                    SecondsInterval = 900d, //15m
                    RegularSkip = 2, //30m
                    MajorSkip = 4, //1h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "mm",
                    RegularFormat = "mm",
                    MajorFormat = "tt h:mm",
                    GridTextFormat = GridTextFormat.Medium,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 60d,
                    //if scale < 60s/px, 1m/px, 1h/60px, 24h/1440px
                    SecondsInterval = 1800d, //30m
                    RegularSkip = 2, //1h
                    MajorSkip = 12, //6h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "mm",
                    RegularFormat = "tt h:mm",
                    MajorFormat = "tt h:mm",
                    GridTextFormat = GridTextFormat.Medium,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 240d,
                    //if scale < 240s/px, 4m/px, 1h/15px, 24h/360px
                    SecondsInterval = 3600d, //1h
                    RegularSkip = 3, //3h
                    MajorSkip = 12, //12h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "",
                    RegularFormat = "tt h",
                    MajorFormat = "tt h",
                    GridTextFormat = GridTextFormat.Short,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 600d,
                    SecondsInterval = 10800d, //3h
                    RegularSkip = 2, //6h
                    MajorSkip = 4, //24h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "tt h",
                    RegularFormat = "tt h",
                    MajorFormat = "tt h",
                    GridTextFormat = GridTextFormat.Short,
                    DrawGrid = true,
                },
                new GridData()
                {
                    ScaleCutoff = 900d,
                    //if scale < 900s/px, 15m/px, 1h/4px, 24h/96px
                    SecondsInterval = 21600d, //6h
                    RegularSkip = 2, //12h
                    MajorSkip = 4, //24h
                    MinorGridLines = true,
                    RegularGridLines = true,
                    MajorGridLines = true,
                    MinorFormat = "",
                    RegularFormat = "tt h",
                    MajorFormat = "tt h",
                    GridTextFormat = GridTextFormat.Short,
                    DrawGrid = true,
                },
                new GridData()
                {
                    //Last
                    ScaleCutoff = double.PositiveInfinity,
                    SecondsInterval = _DAY_SECONDS, //24h
                    RegularSkip = 1,
                    MinorGridLines = false,
                    RegularGridLines = true,
                    MajorGridLines = false,
                    RegularFormat = "",
                    GridTextFormat = GridTextFormat.Hide,
                    DrawGrid = false,
                },
            };
        }
        private void FindGridData()
        {
            if (_ListOfGridDatas == null)
            {
                InitializeGridData();
                _ListOfGridDatas.Sort((x, y) => x.ScaleCutoff.CompareTo(y.ScaleCutoff));
            }
            _GridData = _ListOfGridDatas.First();
            foreach (GridData gd in _ListOfGridDatas)
            {
                _GridData = gd;
                if (Scale < gd.ScaleCutoff) break;
            }
            _ScreenInterval = _GridData.SecondsInterval / Scale;
            UpdateTextMargin(); //because format length could've changed
        }
        private Point GetCellPos(DateTime d, Size cellSize)
        {
            return new Point(GetColumn(d) * cellSize.Width, GetRow(d) * cellSize.Height);
        }
        private Size GetCellSize(Size area)
        {
            Size cellSize = new Size((area.Width - TextMargin) / _VisibleColumns, area.Height / _VisibleRows);
            return cellSize;
        }
        private delegate Rect RectCustomizer(double x, double y, double w, double h);
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Orientation == Orientation.Vertical)
            {
                RectCustomizer RC = (x, y, w, h) => new Rect(x - Offset.X, y - Offset.Y, w, h);
                Size size = ArrangeStuff(RC, arrangeSize, TextMargin, 0);
                VerifyVerticalScrollData(arrangeSize, size);
            }
            else
            {
                RectCustomizer RC = (x, y, w, h) => new Rect(y - Offset.X, x - Offset.Y, h, w);
                Size size = ArrangeStuff(RC, new Size(arrangeSize.Height, arrangeSize.Width), 0, 0);
                VerifyHorizontalScrollData(arrangeSize, new Size(size.Height, size.Width));
            }
            return arrangeSize;
        }
        private Size ArrangeStuff(RectCustomizer RC, Size arrangeSize, double xM, double yM)
        {
            Size cellSize = new Size();
            cellSize.Width = (arrangeSize.Width - TextMargin) / _VisibleColumns;
            cellSize.Height = arrangeSize.Height / _VisibleRows;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                double x = 0;
                double y = 0;
                double width = child.DesiredSize.Width;
                double height = child.DesiredSize.Height;
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarker)
                {
                    if (IsDateTimeRelevant(DateTime.Today))
                    {
                        child.Visibility = Visibility.Visible;
                        width = cellSize.Width;
                        var cell = GetCellPos(DateTime.Now, cellSize);
                        x = xM + cell.X * cellSize.Width;
                        y = yM + cell.Y + DateTime.Now.TimeOfDay.TotalSeconds / Scale - height / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarTaskObject)
                {
                    var C = actualChild as CalendarTaskObject;
                    if (IsCalObjRelevant(C))
                    {
                        child.Visibility = Visibility.Visible;
                        width = cellSize.Width / C.DimensionCount;
                        height = Math.Max(0, (C.End - C.Start).TotalSeconds / Scale);
                        var startDay = (int)(C.Start.Date - Date).TotalDays.Within(0, Days - 1);
                        var currentDay = startDay + C.DayOffset;
                        var currentDate = Date.AddDays(currentDay);
                        var cell = GetCellPos(currentDate, cellSize);
                        var dimensionOffset = width * C.Dimension;
                        x = xM + cell.X + dimensionOffset;
                        y = yM + cell.Y + (C.Start - currentDate).TotalSeconds / Scale;
                        //Cut off excess
                        var end = y + height;
                        if (y < 0)
                        {
                            height = end;
                            y = 0;
                        }
                        else if (end > _DaySize)
                        {
                            height -= end - _DaySize;
                        }
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarFlairObject)
                {
                    CalendarFlairObject C = actualChild as CalendarFlairObject;
                    if (IsDateTimeRelevant(C.DateTime))
                    {
                        child.Visibility = Visibility.Visible;
                        width = cellSize.Width / C.DimensionCount;
                        var cell = GetCellPos(C.DateTime, cellSize);
                        var dimensionOffset = width * C.Dimension;
                        x = xM + cell.X + dimensionOffset;
                        y = yM + cell.Y + C.DateTime.TimeOfDay.TotalSeconds / Scale - height / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                child.Arrange(RC(x, y, width, height));
            }
            return new Size(arrangeSize.Width, _DaySize);
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawGrid(dc);
        }
        private delegate void RectDrawer(Brush brush, double x, double y, double w, double h);
        private delegate void LineDrawer(Pen pen, Point p1, Point p2);
        private delegate void TextDrawer(string text, double size, double x1, double x2, double y);
        private void DrawGrid(DrawingContext dc)
        {
            Rect area;
            RectDrawer RD;
            LineDrawer LD;
            TextDrawer TDmargin;
            TextDrawer TDwatermark;
            TextDrawer TDOOBwatermark;
            double x1M;
            double x2M;
            double yM;
            if (_GridData == null) FindGridData();
            if (Orientation == Orientation.Vertical)
            {
                area = new Rect(new Point(Offset.X, Offset.Y), RenderSize);
                RD = (b, x, y, w, h) => dc.DrawRectangle(b, null, new Rect(x, y, w, h));
                LD = (pen, p1, p2) => dc.DrawLine(pen, p1, p2);
                TDmargin = (t, s, x1, x2, y) => DrawMarginText(dc, t, 0, x1, y);
                TDwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, WatermarkBrush, t, s, x1, y);
                TDOOBwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, MonthBoundsWatermarkBrush, t, s, x1, y);
                x1M = TextMargin;
                x2M = 0;
                yM = 0;

                dc.DrawLine(GridRegularPen,
                    new Point(TextMargin, 0),
                    new Point(TextMargin, RenderSize.Height));
            }
            else
            {
                area = new Rect(new Point(Offset.Y, Offset.X), new Size(RenderSize.Height, RenderSize.Width));
                RD = (b, x, y, w, h) => dc.DrawRectangle(b, null, new Rect(y, x, h, w));
                LD = (pen, p1, p2) => dc.DrawLine(pen, new Point(p1.Y, p1.X), new Point(p2.Y, p2.X));
                TDmargin = (t, s, x1, x2, y) => DrawMarginText(dc, t, -90d, y, x2);
                TDwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, WatermarkBrush, t, s, y, x1);
                TDOOBwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, MonthBoundsWatermarkBrush, t, s, y, x1);
                x1M = 0;
                x2M = TextMargin;
                yM = 0;

                var mY = RenderSize.Height - TextMargin;
                dc.DrawLine(GridRegularPen,
                    new Point(0, mY),
                    new Point(RenderSize.Width, mY));
            }

            Size cellSize = new Size((area.Width - TextMargin) / _VisibleColumns, area.Height / _VisibleRows);
            DrawHighlight(x1M, yM, RD, cellSize);
            DrawWatermark(TDwatermark, TDOOBwatermark, x1M, yM, cellSize);
            DrawTimeLines(area, x1M, x2M, LD, TDmargin, cellSize);
            DrawDateLines(area, x1M, x2M, yM, LD, cellSize);
        }
        private void DrawTimeLines(Rect area, double x1M, double x2M, LineDrawer LD, TextDrawer TDmargin, Size cellSize)
        {
            if (!(ShowGrid && _GridData.DrawGrid)) return;
            for (var j = 0; j < _VisibleRows; j++)
            {
                Rect r = new Rect(area.X, area.Y + cellSize.Height * j, area.Width, cellSize.Height);
                DrawTimeLinesPart2(r, x1M, x2M, LD, TDmargin);
            }
        }
        private void DrawTimeLinesPart2(Rect area, double x1M, double x2M, LineDrawer LD, TextDrawer TD)
        {

            Pen currentPen = GridRegularPen;
            string timeFormat = "";
            //restrict number of draws to within area
            int iStart = (int)(area.Y / _ScreenInterval).Within(0, _MaxIntervals);
            int iEnd = (int)((area.Y + area.Height) / _ScreenInterval + 2).Within(0, _MaxIntervals);
            double finalX1 = x1M;
            double finalX2 = area.Width - x2M;
            for (int i = iStart; i < iEnd; i++)
            {
                //choose a pen
                if (_GridData.MajorGridLines && i % _GridData.MajorSkip == 0)
                {
                    currentPen = GridMajorPen;
                    timeFormat = _GridData.MajorFormat;
                }
                else if (_GridData.RegularGridLines && i % _GridData.RegularSkip == 0)
                {
                    currentPen = GridRegularPen;
                    timeFormat = _GridData.RegularFormat;
                }
                else if (_GridData.MinorGridLines)
                {
                    currentPen = GridMinorPen;
                    timeFormat = _GridData.MinorFormat;
                }
                else continue;
                double y = i * _ScreenInterval;
                double finalY = y - area.Y;
                //Draw a horizontal grid line
                LD(currentPen, new Point(finalX1, finalY), new Point(finalX2, finalY));
                if ((ShowTextMargin || _ShowTextMarginPrevious) && timeFormat != "")
                {
                    string text = Date.AddSeconds(y * Scale).ToString(timeFormat);
                    TD(text, 0, finalX1, finalX2, finalY);
                }
            }
        }
        private void DrawDateLines(Rect area, double x1M, double x2M, double yM, LineDrawer LD, Size cellSize)
        {
            if (!ShowDateLines) return;
            for (int i = 0; i < _VisibleColumns; i++)
            {
                double x = x1M + (i * cellSize.Width);
                double y1 = yM;
                double y2 = area.Height;
                LD(GridRegularPen, new Point(x, y1), new Point(x, y2));
            }
            for (var j = 0; j < _VisibleRows; j++)
            {
                double y = yM + (j * cellSize.Height);
                double x1 = x1M;
                double x2 = area.Width - x2M;
                LD(GridRegularPen, new Point(x1, y), new Point(x2, y));
            }
        }
        private void DrawHighlight(double x1M, double yM, RectDrawer RD, Size cellSize)
        {
            if (ShowHighlight && IsDateTimeRelevant(DateTime.Today))
            {
                var cell = GetCellPos(DateTime.Now, cellSize);
                RD(Highlight, x1M + cell.X, yM + cell.Y, cellSize.Width, cellSize.Height);
            }
            if (ShowMonthBoundsHighlight)
            {
                var day = _FirstVisibleDay;
                for (var i = 0; i < _VisibleColumns; i++)
                {
                    for (var j = 0; j < _VisibleRows; j++)
                    {
                        day = Date.AddDays(i + j);
                        if (day.Month != SelectedMonthOverride)
                        {
                            var cell = GetCellPos(day, cellSize);
                            RD(MonthBoundsHighlight, x1M + cell.X, yM + cell.Y, cellSize.Width, cellSize.Height);
                        }
                    }
                }
            }
        }
        private void DrawWatermark(TextDrawer TDwatermark, TextDrawer TDOOB, double x1M, double yM, Size cellSize)
        {
            if (!ShowWatermark) return;
            var textSize = Math.Max(12d, Math.Min(cellSize.Width / 4d, cellSize.Height / 4d));
            var day = _FirstVisibleDay;
            var xc = cellSize.Width / 2;
            var yc = cellSize.Height / 2;
            for (var i = 0; i < _VisibleColumns; i++)
            {
                for (var j = 0; j < _VisibleRows; j++)
                {
                    day = Date.AddDays(i + j);
                    var x = x1M + i * cellSize.Width + xc;
                    var y = yM + j * cellSize.Height + yc;
                    var text = day.ToString(WatermarkFormat);
                    if (day.Month != SelectedMonthOverride)
                        TDOOB(text, textSize, x, 0, y);
                    else
                        TDwatermark(text, textSize, x, 0, y);
                }
            }
        }
        private void DrawWatermarkText(DrawingContext dc, Brush brush, string text, double textSize, double x, double y)
        {
            FormattedText lineText = new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(WatermarkFontFamily, FontStyles.Normal,
                FontWeights.Bold, FontStretches.Normal),
                textSize, brush, null,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            lineText.TextAlignment = TextAlignment.Center;
            dc.DrawText(lineText, new Point(x, y - lineText.Height / 2d));
        }
        private void DrawMarginText(DrawingContext dc, string text, double r, double x, double y)
        {
            FormattedText lineText = new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize, Foreground,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            lineText.TextAlignment = TextAlignment.Right;
            TranslateTransform offset = new TranslateTransform(_TextOffset.X, _TextOffset.Y - lineText.Height / 2);
            RotateTransform rudeboi = new RotateTransform(_TextRotation + r);
            TranslateTransform position = new TranslateTransform(x, y);
            dc.PushTransform(position);
            dc.PushTransform(rudeboi);
            dc.PushTransform(offset);
            dc.DrawText(lineText, _TextOffset);
            dc.Pop();
            dc.Pop();
            dc.Pop();
        }
        #endregion Layout
        #region IScrollInfo
        protected const double _scrollLineDelta = 16d;
        protected const double _mouseWheelDelta = 3 * _scrollLineDelta;
        protected ScrollViewer _ScrollOwner;
        public ScrollViewer ScrollOwner
        {
            get { return _ScrollOwner; }
            set
            {
                if (_ScrollOwner == value) return;
                _ScrollOwner = value;
                ResetScrolling();
            }
        }
        protected bool _CanHorizontallyScroll = false;
        public bool CanVerticallyScroll
        {
            get { return _CanVerticallyScroll; }
            set { _CanVerticallyScroll = value; }
        }
        protected bool _CanVerticallyScroll = false;
        public bool CanHorizontallyScroll
        {
            get { return _CanHorizontallyScroll; }
            set { _CanHorizontallyScroll = value; }
        }
        protected Size _Extent = new Size(0, 0);
        public double ExtentWidth => _Extent.Width;
        public double ExtentHeight => _Extent.Height;
        protected Size _Viewport = new Size(0, 0);
        public double ViewportWidth => _Viewport.Width;
        public double ViewportHeight => _Viewport.Height;
        public double HorizontalOffset => _Offset.X;
        public double VerticalOffset => _Offset.Y;
        public void LineUp() { SetVerticalOffset(VerticalOffset - _scrollLineDelta); }
        public void LineDown() { SetVerticalOffset(VerticalOffset + _scrollLineDelta); }
        public void LineLeft() { SetHorizontalOffset(HorizontalOffset - _scrollLineDelta); }
        public void LineRight() { SetHorizontalOffset(HorizontalOffset + _scrollLineDelta); }
        public void PageUp() { SetVerticalOffset(VerticalOffset - ViewportHeight); }
        public void PageDown() { SetVerticalOffset(VerticalOffset + ViewportHeight); }
        public void PageLeft() { SetHorizontalOffset(HorizontalOffset - ViewportWidth); }
        public void PageRight() { SetHorizontalOffset(HorizontalOffset + ViewportWidth); }
        public void MouseWheelUp() { SetVerticalOffset(VerticalOffset - _mouseWheelDelta); }
        public void MouseWheelDown() { SetVerticalOffset(VerticalOffset + _mouseWheelDelta); }
        public void MouseWheelLeft() { SetHorizontalOffset(HorizontalOffset - _mouseWheelDelta); }
        public void MouseWheelRight() { SetHorizontalOffset(HorizontalOffset + _mouseWheelDelta); }
        public void SetHorizontalOffset(double offset)
        {
            offset = offset.Within(0, ExtentWidth - ViewportWidth);
            if (offset != _Offset.Y)
            {
                _Offset.X = offset;
                AnimateOffset();
            }
        }
        public void SetVerticalOffset(double offset)
        {
            offset = offset.Within(0, ExtentHeight - ViewportHeight);
            if (offset != _Offset.Y)
            {
                _Offset.Y = offset;
                AnimateOffset();
            }
        }
        protected void VerifyVerticalScrollData(Size viewport, Size extent)
        {
            if (double.IsInfinity(viewport.Width))
            { viewport.Width = extent.Width; }
            if (double.IsInfinity(viewport.Height))
            { viewport.Height = extent.Height; }
            _Extent = extent;
            _Viewport = viewport;
            _Offset.X = _Offset.X.Within(0, ExtentWidth - ViewportWidth);
            _Offset.Y = _Offset.Y.Within(0, ExtentHeight - ViewportHeight);
            if (ScrollOwner != null)
            { ScrollOwner.InvalidateScrollInfo(); }
        }
        protected void VerifyHorizontalScrollData(Size viewport, Size extent)
        {
            if (double.IsInfinity(viewport.Width))
            { viewport.Width = extent.Width; }
            if (double.IsInfinity(viewport.Height))
            { viewport.Height = extent.Height; }
            _Extent = extent;
            _Viewport = viewport;
            _Offset.X = _Offset.X.Within(0, ExtentWidth - ViewportWidth);
            _Offset.Y = _Offset.Y.Within(ExtentHeight - ViewportHeight, 0);
            if (ScrollOwner != null)
            { ScrollOwner.InvalidateScrollInfo(); }
        }
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            if (rectangle.IsEmpty || visual == null || visual == this || !base.IsAncestorOf(visual))
            { return Rect.Empty; }
            rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);
            Rect viewRect = new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);
            rectangle.X += viewRect.X;
            rectangle.Y += viewRect.Y;
            viewRect.X = CalculateNewScrollOffset(viewRect.Left, viewRect.Right, rectangle.Left, rectangle.Right);
            viewRect.Y = CalculateNewScrollOffset(viewRect.Top, viewRect.Bottom, rectangle.Top, rectangle.Bottom);
            SetHorizontalOffset(viewRect.X);
            SetVerticalOffset(viewRect.Y);
            rectangle.Intersect(viewRect);
            rectangle.X -= viewRect.X;
            rectangle.Y -= viewRect.Y;
            return rectangle;
        }
        protected static double CalculateNewScrollOffset(double topView, double bottomView, double topChild, double bottomChild)
        {
            bool offBottom = topChild < topView && bottomChild < bottomView;
            bool offTop = bottomChild > bottomView && topChild > topView;
            bool tooLarge = (bottomChild - topChild) > (bottomView - topView);
            if (!offBottom && !offTop)
            { return topView; } //Don't do anything, already in view
            if ((offBottom && !tooLarge) || (offTop && tooLarge))
            { return topChild; }
            return (bottomChild - (bottomView - topView));
        }
        protected void ResetScrolling()
        {
            if (_ScrollOwner != null)
            {
                BeginAnimation(OffsetProperty, null);
                Offset = _Offset = new Vector();
                _Viewport = _Extent = new Size();
            }
        }
        #endregion IScrollInfo
    }
}
