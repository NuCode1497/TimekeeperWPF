﻿// Copyright 2017 (C) Cody Neuburger  All rights reserved.
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
            _RelativeColumns = _DefaultColumns;
            _RelativeRows = _DefaultRows;
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
            if (TimeOrientation == Orientation.Vertical)
            {
                var pos = e.GetPosition(this);
                var date = Date;
                var seconds = (int)((pos.Y + Offset.Y) * Scale).Within(0, _CellRange);
                var time = new TimeSpan(0, 0, seconds);
                MousePosition = date + time;
            }
            else
            {
                var pos = e.GetPosition(this);
                var date = Date;
                var seconds = (int)((pos.X + Offset.X) * Scale).Within(0, _CellRange);
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
                if (TimeOrientation == Orientation.Vertical)
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
            d.CoerceValue(TimeTextMarginProperty);
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
            d.CoerceValue(TimeTextMarginProperty);
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
            d.CoerceValue(TimeTextMarginProperty);
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
            d.CoerceValue(TimeTextMarginProperty);
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
            d.CoerceValue(TimeTextMarginProperty);
        }
        #endregion
        #region GridMargin
        private Thickness _GridMargin = new Thickness();
        private bool _ShowTimeTextMarginPrevious = true;
        /// <summary>
        /// Show or hide the grid time text.
        /// </summary>
        [Bindable(true)]
        public bool ShowTimeTextMargin
        {
            get { return (bool)GetValue(ShowTimeTextMarginProperty); }
            set { SetValue(ShowTimeTextMarginProperty, value); }
        }
        public static readonly DependencyProperty ShowTimeTextMarginProperty =
            DependencyProperty.Register(
                nameof(ShowTimeTextMargin), typeof(bool), typeof(Day),
                new FrameworkPropertyMetadata(true,
                    new PropertyChangedCallback(OnShowTimeTextMarginChanged)));
        public static void OnShowTimeTextMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Day day = d as Day;
            day.UpdateTimeTextMargin();
        }
        private double _TimeTextMargin = 0;
        public double TimeTextMargin
        {
            get { return (double)GetValue(TimeTextMarginProperty); }
            set { SetValue(TimeTextMarginProperty, value); }
        }
        public static readonly DependencyProperty TimeTextMarginProperty =
            DependencyProperty.Register(
                nameof(TimeTextMargin), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(0d,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnTimeTextMarginChanged)));
        public static void OnTimeTextMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        private void UpdateTimeTextMargin()
        {
            if (ShowTimeTextMargin)
            {
                string format = "";
                if (_GridData.GridTextFormat == GridTextFormat.Long) format = "00:00:00 AM";
                else if (_GridData.GridTextFormat == GridTextFormat.Medium) format = "00:00 AM";
                else if (_GridData.GridTextFormat == GridTextFormat.Short) format = "00 AM";
                else
                {
                    _TimeTextMargin = 0;
                    AnimateTimeTextMargin();
                    return;
                }
                FormattedText lineText = new FormattedText(format,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.RightToLeft,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    FontSize, Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                _TimeTextMargin = lineText.Width - TextOffset.X - TextOffset.X;
                AnimateTimeTextMargin();
            }
            else
            {
                _TimeTextMargin = 0;
                AnimateTimeTextMargin();
            }
        }
        private void AnimateTimeTextMargin()
        {
            DoubleAnimation anime = new DoubleAnimation();
            anime.Duration = _AnimationLength;
            anime.To = _TimeTextMargin;
            anime.AccelerationRatio = _AccelerationRatio;
            anime.DecelerationRatio = _DecelerationRatio;
            anime.Completed += OnAnimateTimeTextMarginCompleted;
            BeginAnimation(TimeTextMarginProperty, anime, HandoffBehavior.Compose);
        }
        private void OnAnimateTimeTextMarginCompleted(object sender, EventArgs e)
        {
            _ShowTimeTextMarginPrevious = ShowTimeTextMargin;
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
            if (day.TimeOrientation == Orientation.Vertical)
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
        private enum DateTimeOrientation { HDHT, HDVT, VDHT, VDVT }
        private DateTimeOrientation DTO = DateTimeOrientation.HDVT;
        public Orientation TimeOrientation
        {
            get { return (Orientation)GetValue(TimeOrientationProperty); }
            set { SetValue(TimeOrientationProperty, value); }
        }
        public static readonly DependencyProperty TimeOrientationProperty =
            DependencyProperty.Register(
                nameof(TimeOrientation), typeof(Orientation), typeof(Day),
                new FrameworkPropertyMetadata(Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnTimeOrientationChanged)),
                new ValidateValueCallback(IsValidOrientation));
        private static void OnTimeOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Day day = d as Day;
            day.SetRange();
            day.ResetScrolling();
            day.CoerceValue(OffsetProperty);
            day.CoerceValue(ScaleProperty);
        }
        protected override bool HasLogicalOrientation => true;
        protected override Orientation LogicalOrientation => TimeOrientation;
        internal static bool IsValidOrientation(object o)
        {
            Orientation value = (Orientation)o;
            return value == Orientation.Horizontal
                || value == Orientation.Vertical;
        }
        public Orientation DateOrientation
        {
            get { return (Orientation)GetValue(DateOrientationProperty); }
            set { SetValue(DateOrientationProperty, value); }
        }
        public static readonly DependencyProperty DateOrientationProperty =
            DependencyProperty.Register(
                nameof(DateOrientation), typeof(Orientation), typeof(Day),
                new FrameworkPropertyMetadata(Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnDateOrientationChanged)),
                new ValidateValueCallback(IsValidOrientation));
        public static void OnDateOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Day day = d as Day;
            day.SetRange();
            day.ResetScrolling();
            day.CoerceValue(OffsetProperty);
            day.CoerceValue(ScaleProperty);
        }
        private void SetRange()
        {
            if (DateOrientation == Orientation.Horizontal)
            {
                if (TimeOrientation == Orientation.Horizontal)
                {
                    DTO = DateTimeOrientation.HDHT;
                }
                else
                {
                    DTO = DateTimeOrientation.HDVT;
                }
                _RelativeColumns = _DefaultColumns;
                _DaysPerCell = 1;
            }
            else
            {
                if (TimeOrientation == Orientation.Horizontal)
                {
                    DTO = DateTimeOrientation.VDHT;
                }
                else
                {
                    DTO = DateTimeOrientation.VDVT;
                }
                _RelativeColumns = 1;
                _DaysPerCell = _DefaultColumns;
                ForceMaxScale = true;
            }
            switch (DTO)
            {
                case DateTimeOrientation.HDHT:
                    break;
                case DateTimeOrientation.HDVT:
                    break;
                case DateTimeOrientation.VDHT:
                    break;
                case DateTimeOrientation.VDVT:
                    break;
            }
            _CellRange = TimeSpan.FromDays(_DaysPerCell).TotalSeconds;
        }
        #endregion
        #region Mouse
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
        public ObservableCollection<CalendarObject> ItemsUnderMouse
        {
            get { return (ObservableCollection<CalendarObject>)GetValue(ItemsUnderMouseProperty); }
            set { SetValue(ItemsUnderMouseProperty, value); }
        }
        public static readonly DependencyProperty ItemsUnderMouseProperty =
            DependencyProperty.Register(
                nameof(ItemsUnderMouse), typeof(ObservableCollection<CalendarObject>), typeof(Day),
                new FrameworkPropertyMetadata(new ObservableCollection<CalendarObject>()));
        #endregion Mouse
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
            day._CellRangeSize = day._CellRange / day.Scale;
        }
        private static object OnCoerceScale(DependencyObject d, object value)
        {
            Day day = d as Day;
            Double newValue = (Double)value;
            if (day.ForceMaxScale || newValue > day.GetMaxScale() || day.DateOrientation == Orientation.Vertical)
            {
                newValue = day.GetMaxScale();
            }
            else
            {
                newValue /= day._RelativeRows;
            }
            if (newValue < day._ScaleLowerLimit) return day._ScaleLowerLimit;
            if (newValue > day._ScaleUpperLimit) return day._ScaleUpperLimit;
            if (Double.IsNaN(newValue)) return DependencyProperty.UnsetValue;
            return newValue;
        }
        public virtual double GetMaxScale()
        {
            if (TimeOrientation == Orientation.Vertical) 
                return _CellRange / RenderSize.Height * _RelativeRows;
            else 
                return _CellRange / RenderSize.Width * _RelativeRows;
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
            if (TimeOrientation == Orientation.Vertical)
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
        #region ShowDateLines
        public bool ShowDateLines
        {
            get { return (bool)GetValue(ShowDateLinesProperty); }
            set { SetValue(ShowDateLinesProperty, value); }
        }
        public static readonly DependencyProperty ShowDateLinesProperty =
            DependencyProperty.Register(
                nameof(ShowDateLines), typeof(bool), typeof(Day),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region ShowTimeLines
        public bool ShowTimeLines
        {
            get { return (bool)GetValue(ShowTimeLinesProperty); }
            set { SetValue(ShowTimeLinesProperty, value); }
        }
        public static readonly DependencyProperty ShowTimeLinesProperty =
            DependencyProperty.Register(
                nameof(ShowTimeLines), typeof(bool), typeof(Day),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region TextOffset
        public Point TextOffset
        {
            get { return (Point)GetValue(TextOffsetProperty); }
            set { SetValue(TextOffsetProperty, value); }
        }
        public static readonly DependencyProperty TextOffsetProperty =
            DependencyProperty.Register(
                nameof(TextOffset), typeof(Point), typeof(Day),
                new FrameworkPropertyMetadata(new Point(-4, 0),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region TextRotation
        public double TextRotation
        {
            get { return (double)GetValue(TextRotationProperty); }
            set { SetValue(TextRotationProperty, value); }
        }
        public static readonly DependencyProperty TextRotationProperty =
            DependencyProperty.Register(
                nameof(TextRotation), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(0d,
                    FrameworkPropertyMetadataOptions.AffectsRender));
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
        #region GridData
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
        private double _ScreenInterval; //Number of pixels between each line
        private int _MaxIntervals => (int)(_CellRange / _GridData.SecondsInterval);
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
                    ScaleCutoff = 24,
                    SecondsInterval = 900, //15m
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
                    ScaleCutoff = 60,
                    //if scale < 60s/px, 1m/px, 1h/60px, 24h/1440px
                    SecondsInterval = 1800, //30m
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
                    ScaleCutoff = 240,
                    //if scale < 240s/px, 4m/px, 1h/15px, 24h/360px
                    SecondsInterval = 3600, //1h
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
                    ScaleCutoff = 600,
                    SecondsInterval = 10800, //3h
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
                    SecondsInterval = 21600, //6h
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
                    ScaleCutoff = 1800d,
                    SecondsInterval = 43200, //12h
                    MajorSkip = 2, //24h
                    MinorGridLines = true,
                    RegularGridLines = false,
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
                    SecondsInterval = 86400, //24h
                    RegularSkip = 1,
                    MinorGridLines = false,
                    RegularGridLines = true,
                    MajorGridLines = false,
                    RegularFormat = "",
                    GridTextFormat = GridTextFormat.Hide,
                    DrawGrid = true,
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
            UpdateTimeTextMargin(); //because format length could've changed
        }
        #endregion GridData
        private delegate Size RelativeSize(double w, double h);
        private delegate Rect RelativeRect(double x, double y, double w, double h);
        private delegate Point RelativePoint(double x, double y);
        private delegate void RelativeRectDrawer(Brush brush, double x, double y, double w, double h);
        private delegate void RelativeLineDrawer(Pen pen, Point p1, Point p2);
        private delegate void RelativeTextDrawer(string text, double size, double x1, double x2, double y);
        public const int _DAY_SECONDS = 86400;
        protected virtual int _DefaultColumns => 1;
        protected virtual int _DefaultRows => 1;
        protected virtual DateTime _FirstVisibleDay => Date;
        protected virtual DateTime _LastVisibleDay => Date;
        protected virtual int _Days => 1;
        protected int _RelativeColumns;
        protected int _RelativeRows;
        protected double _CellRange = _DAY_SECONDS;
        private int _DaysPerCell = 1;
        private int _Cells => _RelativeColumns * _RelativeRows;
        private double _CellRangeSize;
        private int GetCellNumber(DateTime d) => (int)((d - _FirstVisibleDay).TotalSeconds / _CellRange).Within(0, _Cells - 1);
        private Point GetCellPos(int cellNum, Size cellSize)
        {
            var C = (int)Math.Floor((double)cellNum % _RelativeColumns);
            var R = (int)Math.Floor((double)cellNum / _RelativeColumns);
            return new Point(C * cellSize.Width, R * cellSize.Height);
        }
        private DateTime GetCellDateTime(int cellNum) => _FirstVisibleDay + new TimeSpan(0, 0, (int)(_CellRange * cellNum));
        private Size GetCellSize(Size area) => new Size((area.Width - TimeTextMargin) / _RelativeColumns, area.Height / _RelativeRows);
        protected override Size MeasureOverride(Size availableSize)
        {
            if (TimeOrientation == Orientation.Vertical)
            {
                RelativeSize measurer = (w, h) => new Size(w, h);
                Size size = MeasureStuff(measurer, new Size(availableSize.Width, double.PositiveInfinity));
                VerifyVerticalScrollData(availableSize, size);
            }
            else
            {
                RelativeSize measurer = (w, h) => new Size(h, w);
                Size size = MeasureStuff(measurer, new Size(availableSize.Height, double.PositiveInfinity));
                VerifyHorizontalScrollData(availableSize, new Size(size.Height, size.Width));
            }
            return _Viewport;
        }
        private Size MeasureStuff(RelativeSize sizer, Size availableSize)
        {
            double cellWidth = (availableSize.Width - TimeTextMargin) / _RelativeColumns;
            double width = availableSize.Width;
            double height = availableSize.Height;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarker)
                {
                    if (IsDateTimeRelevant(DateTime.Today))
                    {
                        width = cellWidth;
                    }
                    else continue;
                }
                else if (actualChild is CalendarTaskObject)
                {
                    var C = actualChild as CalendarTaskObject;
                    if (IsCalObjRelevant(C))
                    {
                        width = cellWidth / C.DimensionCount;
                    }
                    else continue;
                }
                else if (actualChild is CalendarFlairObject)
                {
                    var C = actualChild as CalendarNoteObject;
                    if (IsDateTimeRelevant(C.DateTime))
                    {
                        width = cellWidth / C.DimensionCount;
                    }
                    else continue;
                }
                child.Measure(sizer(width, height));
            }
            return new Size(availableSize.Width, _CellRangeSize);
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (TimeOrientation == Orientation.Vertical)
            {
                RelativeRect RC = (x, y, w, h) => new Rect(x - Offset.X, y - Offset.Y, w, h);
                RelativeSize sizer = (w, h) => new Size(w, h);
                Size size = ArrangeStuff(sizer, RC, arrangeSize, TimeTextMargin, 0);
                VerifyVerticalScrollData(arrangeSize, size);
            }
            else
            {
                RelativeRect RC = (x, y, w, h) => new Rect(y - Offset.X, x - Offset.Y, h, w);
                RelativeSize sizer = (w, h) => new Size(h, w);
                Size size = ArrangeStuff(sizer, RC, new Size(arrangeSize.Height, arrangeSize.Width), 0, 0);
                VerifyHorizontalScrollData(arrangeSize, new Size(size.Height, size.Width));
            }
            return arrangeSize;
        }
        private Size ArrangeStuff(RelativeSize sizer, RelativeRect recter, Size arrangeSize, double xM, double yM)
        {
            Size cellSize = GetCellSize(arrangeSize);
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                double x = 0;
                double y = 0;
                Size childDesiredSize = sizer(child.DesiredSize.Width, child.DesiredSize.Height);
                double width = childDesiredSize.Width;
                double height = childDesiredSize.Height;
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarker)
                {
                    var C = actualChild as NowMarker;
                    if (IsDateTimeRelevant(DateTime.Now))
                    {
                        width = cellSize.Width;
                        var cellNum = GetCellNumber(DateTime.Now);
                        var cellDate = GetCellDateTime(cellNum);
                        var cellPos = GetCellPos(cellNum, cellSize);
                        var relX = xM;
                        var relY = yM + (DateTime.Now - cellDate).TotalSeconds / Scale - height / 2;
                        x = relX + cellPos.X;
                        y = relY + cellPos.Y;
                        C.Orientation = TimeOrientation;
                        child.Visibility = Visibility.Visible;
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
                        //wrangle within bounds
                        var startDay = (int)(C.Start.Date - Date).TotalDays.Within(0, _Days - 1);
                        var currentDay = startDay;
                        //shadow clone offset
                        if (C.DayOffset > 0)
                        {
                            var startDate = Date.AddDays(startDay);
                            var startCell = GetCellNumber(startDate);
                            var startCellDate = GetCellDateTime(startCell);
                            //only draw this shadowclone if it is the first in a cell
                            if (((startDate - startCellDate).TotalDays + C.DayOffset) % _DaysPerCell != 0)
                            {
                                child.Visibility = Visibility.Collapsed;
                                continue;
                            }
                            currentDay += C.DayOffset;
                        }
                        var currentDate = Date.AddDays(currentDay);
                        var currentCellNum = GetCellNumber(currentDate);
                        var currentCellDate = GetCellDateTime(currentCellNum);
                        var currentCellPos = GetCellPos(currentCellNum, cellSize);

                        width = cellSize.Width / C.DimensionCount;
                        height = (C.End - C.Start).TotalSeconds / Scale;
                        //get relative position within cell
                        var dimensionOffset = width * C.Dimension;
                        var relX = xM + dimensionOffset;
                        var relY = yM + (C.Start - currentCellDate).TotalSeconds / Scale;
                        //Cut off excess
                        var end = relY + height;
                        if (relY < 0)
                        {
                            height = end;
                            relY = 0;
                        }
                        else if (end > _CellRangeSize)
                        {
                            height -= end - _CellRangeSize;
                        }
                        if (height <= 0)
                        {
                            child.Visibility = Visibility.Collapsed;
                            continue;
                        }
                        x = relX + currentCellPos.X;
                        y = relY + currentCellPos.Y;
                        C.Orientation = TimeOrientation;
                        child.Visibility = Visibility.Visible;
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
                        width = cellSize.Width / C.DimensionCount;
                        var cellNum = GetCellNumber(DateTime.Now);
                        var cellPos = GetCellPos(cellNum, cellSize);
                        var dimensionOffset = width * C.Dimension;
                        var relX = xM + dimensionOffset;
                        var relY = yM + C.DateTime.TimeOfDay.TotalSeconds / Scale - height / 2;
                        x = relX + cellPos.X;
                        y = relY + cellPos.Y;
                        C.Orientation = TimeOrientation;
                        child.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                child.Arrange(recter(x, y, width, height));
            }
            return new Size(arrangeSize.Width, _CellRangeSize);
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawGrid(dc);
        }
        private void DrawGrid(DrawingContext dc)
        {
            if (_GridData == null) FindGridData();
            Rect VrenderArea = new Rect(new Point(Offset.X, Offset.Y), RenderSize);
            RelativeRectDrawer VRD = (b, x, y, w, h) => dc.DrawRectangle(b, null, new Rect(x, y, w, h));
            RelativeLineDrawer VLD = (pen, p1, p2) => dc.DrawLine(pen, p1, p2);
            RelativeTextDrawer VTDmargin = (t, s, x1, x2, y) => DrawMarginText(dc, t, 0, x1, y);
            RelativeTextDrawer VTDwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, WatermarkBrush, t, s, x1, y);
            RelativeTextDrawer VTDOOBwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, MonthBoundsWatermarkBrush, t, s, x1, y);
            double Vx1M = TimeTextMargin;
            double Vx2M = 0;
            double VyM = 0;
            Rect HrenderArea = new Rect(new Point(Offset.Y, Offset.X), new Size(RenderSize.Height, RenderSize.Width));
            RelativeRectDrawer HRD = (b, x, y, w, h) => dc.DrawRectangle(b, null, new Rect(y, x, h, w));
            RelativeLineDrawer HLD = (pen, p1, p2) => dc.DrawLine(pen, new Point(p1.Y, p1.X), new Point(p2.Y, p2.X));
            RelativeTextDrawer HTDmargin = (t, s, x1, x2, y) => DrawMarginText(dc, t, -90d, y, x2);
            RelativeTextDrawer HTDwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, WatermarkBrush, t, s, y, x1);
            RelativeTextDrawer HTDOOBwatermark = (t, s, x1, x2, y) => DrawWatermarkText(dc, MonthBoundsWatermarkBrush, t, s, y, x1);
            double Hx1M = 0;
            double Hx2M = TimeTextMargin;
            double HyM = 0;
            if (TimeOrientation == Orientation.Vertical)
            {
                Size VcellSize = new Size((VrenderArea.Width - TimeTextMargin) / _RelativeColumns, VrenderArea.Height / _RelativeRows);
                if (DateOrientation == Orientation.Horizontal)
                {
                    DrawHighlight(Vx1M, VyM, VRD, VcellSize);
                    DrawWatermark(VTDwatermark, VTDOOBwatermark, Vx1M, VyM, VcellSize);
                }
                else
                {
                    Size HcellSize = new Size((VrenderArea.Height) / _DefaultColumns, (HrenderArea.Height + TimeTextMargin) / _RelativeRows);
                    DrawHighlight(Hx1M, Vx1M, HRD, HcellSize);
                    DrawWatermark(HTDwatermark, HTDOOBwatermark, Hx1M, HyM, HcellSize);
                }
                DrawTimeLines(VrenderArea, Vx1M, Vx2M, VLD, VTDmargin, VcellSize);
                DrawDateLines(VrenderArea, Vx1M, Vx2M, VyM, VLD, VcellSize);

                dc.DrawLine(GridRegularPen,
                    new Point(TimeTextMargin, 0),
                    new Point(TimeTextMargin, RenderSize.Height));
            }
            else
            {
                Size HcellSize = new Size((HrenderArea.Width - TimeTextMargin) / _RelativeColumns, HrenderArea.Height / _RelativeRows);
                if (DateOrientation == Orientation.Horizontal)
                {
                    DrawHighlight(Hx1M, HyM, HRD, HcellSize);
                    DrawWatermark(HTDwatermark, HTDOOBwatermark, Hx1M, HyM, HcellSize);
                }
                else
                {
                    Size VcellSize = new Size((VrenderArea.Width) / _DefaultColumns, (VrenderArea.Height - TimeTextMargin) / _RelativeRows);
                    DrawHighlight(HyM, Hx1M, VRD, VcellSize);
                    DrawWatermark(VTDwatermark, VTDOOBwatermark, HyM, Hx1M, VcellSize);
                }
                DrawTimeLines(HrenderArea, Hx1M, Hx2M, HLD, HTDmargin, HcellSize);
                DrawDateLines(HrenderArea, Hx1M, Hx2M, HyM, HLD, HcellSize);

                var mY = RenderSize.Height - TimeTextMargin;
                dc.DrawLine(GridRegularPen,
                    new Point(0, mY),
                    new Point(RenderSize.Width, mY));
            }
        }
        private void DrawTimeLines(Rect renderArea, double x1M, double x2M, RelativeLineDrawer LD, RelativeTextDrawer TDmargin, Size cellSize)
        {
            if (!(ShowTimeLines && _GridData.DrawGrid)) return;
            for (var j = 0; j < _RelativeRows; j++)
            {
                Pen currentPen = GridRegularPen;
                string timeFormat = "";
                //restrict number of draws to within area
                int iStart = (int)(renderArea.Y / _ScreenInterval).Within(0, _MaxIntervals);
                int iEnd = (int)((renderArea.Y + cellSize.Height) / _ScreenInterval + 1).Within(0, _MaxIntervals);
                double finalX1 = x1M;
                double finalX2 = renderArea.Width - x2M;
                for (int i = iStart; i <= iEnd; i++)
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
                    double finalY = y - renderArea.Y + cellSize.Height * j;
                    //Draw a horizontal grid line
                    LD(currentPen, new Point(finalX1, finalY), new Point(finalX2, finalY));
                    if ((ShowTimeTextMargin || _ShowTimeTextMarginPrevious) && timeFormat != "")
                    {
                        string text = Date.AddSeconds(y * Scale).ToString(timeFormat);
                        TDmargin(text, 0, finalX1, finalX2, finalY);
                    }
                }
            }
        }
        private void DrawDateLines(Rect renderArea, double x1M, double x2M, double yM, RelativeLineDrawer LD, Size cellSize)
        {
            if (!ShowDateLines) return;
            for (int i = 0; i <= _RelativeColumns; i++)
            {
                double x = x1M + (i * cellSize.Width);
                double y1 = yM;
                double y2 = renderArea.Height;
                LD(GridMajorPen, new Point(x, y1), new Point(x, y2));
            }
            for (var j = 0; j <= _RelativeRows; j++)
            {
                double y = yM + (j * cellSize.Height);
                double x1 = x1M;
                double x2 = renderArea.Width - x2M;
                LD(GridMajorPen, new Point(x1, y), new Point(x2, y));
            }
        }
        private void DrawHighlight(double x1M, double yM, RelativeRectDrawer RD, Size cellSize)
        {
            if (ShowHighlight && IsDateTimeRelevant(DateTime.Today))
            {
                var cellNum = GetCellNumber(DateTime.Now);
                var cellPos = GetCellPos(cellNum, cellSize);
                RD(Highlight, x1M + cellPos.X, yM + cellPos.Y, cellSize.Width, cellSize.Height);
            }
            if (ShowMonthBoundsHighlight)
            {
                var day = _FirstVisibleDay;
                for (var j = 0; j < _RelativeRows; j++)
                {
                    for (var i = 0; i < _RelativeColumns; i++)
                    {
                        if (!IsDateTimeRelevant(day))
                        {
                            var x = x1M + i * cellSize.Width;
                            var y = yM + j * cellSize.Height;
                            RD(MonthBoundsHighlight, x, y, cellSize.Width, cellSize.Height);
                        }
                        day = day.AddDays(1);
                    }
                }
            }
        }
        private void DrawWatermark(RelativeTextDrawer TDwatermark, RelativeTextDrawer TDOOB, double x1M, double yM, Size cellSize)
        {
            if (!ShowWatermark) return;
            var textSize = Math.Max(12d, Math.Min(cellSize.Width / 4d, cellSize.Height / 4d));
            var day = _FirstVisibleDay;
            var xc = cellSize.Width / 2;
            var yc = cellSize.Height / 2;
            for (var j = 0; j < _DefaultRows; j++)
            {
                for (var i = 0; i < _DefaultColumns; i++)
                {
                    var x = x1M + i * cellSize.Width + xc;
                    var y = yM + j * cellSize.Height + yc;
                    var text = day.ToString(WatermarkFormat);
                    if (!IsDateTimeRelevant(day))
                        TDOOB(text, textSize, x, 0, y);
                    else
                        TDwatermark(text, textSize, x, 0, y);
                    day = day.AddDays(1);
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
            TranslateTransform offset = new TranslateTransform(TextOffset.X, TextOffset.Y - lineText.Height / 2);
            RotateTransform rudeboi = new RotateTransform(TextRotation + r);
            TranslateTransform position = new TranslateTransform(x, y);
            dc.PushTransform(position);
            dc.PushTransform(rudeboi);
            dc.PushTransform(offset);
            dc.DrawText(lineText, TextOffset);
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
