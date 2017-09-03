// Copyright 2012 (C) Cody Neuburger  All rights reserved.
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

namespace TimekeeperWPF.Views.Calendar.Day
{
    public class Day : Panel, IScrollInfo
    {
        #region Fields
        private TimeSpan _AnimationLength = TimeSpan.FromMilliseconds(500);
        private double ScaleFactor = 0.2d;
        #region IScrollInfo Fields
        private const double LineSize = 12;
        private const double WheelSize = 3 * LineSize;
        private bool _CanHorizontallyScroll = false;
        private bool _CanVerticallyScroll = false;
        private ScrollViewer _ScrollOwner;
        private Size _Extent = new Size(0, 0);
        private Size _Viewport = new Size(0, 0);
        #endregion IScrollInfo Fields
        #endregion Fields
        public Day() : base()
        {
            BackgroundProperty.OverrideMetadata(typeof(Day),
                new FrameworkPropertyMetadata(Brushes.MintCream,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
            Scale = 25d;
        }
        #region Events
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                //Scale
                int delta = -e.Delta / Math.Abs(e.Delta);
                _Scale = Scale * (1d + ScaleFactor * delta);
                _Offset = (Offset * Scale) / _Scale;
                AnimateScale();
                AnimateOffset();
                e.Handled = true;
            }
            base.OnPreviewMouseWheel(e);
        }
        #endregion Events
        #region Properties
        #region TextMargin
        public double TextMargin
        {
            get { return (double)GetValue(TextMarginProperty); }
            private set { SetValue(TextMarginPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey TextMarginPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TextMargin), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(80d,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    new CoerceValueCallback(OnCoerceTextMargin)));
        public static readonly DependencyProperty TextMarginProperty =
            TextMarginPropertyKey.DependencyProperty;
        public static object OnCoerceTextMargin(DependencyObject d, object value)
        {
            Day day = (Day)d;
            string format = "";
            if (day.Scale < 2) format = "00:00:00 AM";
            else if (day.Scale < 240) format = "00:00 AM";
            else format = "00 AM";
            FormattedText lineText = new FormattedText(format,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.RightToLeft,
                new Typeface(day.FontFamily, day.FontStyle, day.FontWeight, day.FontStretch),
                day.FontSize, day.Foreground,
                VisualTreeHelper.GetDpi(day).PixelsPerDip);
            return lineText.Width + 8;
        }
        #endregion
        #region Date
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
                    FrameworkPropertyMetadataOptions.AffectsArrange));
        #endregion
        #region Offset
        private Vector _Offset = new Vector();
        private Vector Offset
        {
            get { return (Vector)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }
        private static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register(
                nameof(Offset), typeof(Vector), typeof(Day),
                new FrameworkPropertyMetadata(new Vector(),
                    FrameworkPropertyMetadataOptions.AffectsRender));
        private void AnimateOffset()
        {
            VectorAnimation anime = new VectorAnimation();
            anime.Duration = _AnimationLength;
            anime.To = new Vector(HorizontalOffset, VerticalOffset);
            BeginAnimation(OffsetProperty, anime, HandoffBehavior.Compose);
        }
        #endregion Offset
        #region Scale
        // Scale is in Seconds per Pixel s/px
        //final non-animated scale
        private double _Scale = 60d;
        //animated scale
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
            day.CoerceValue(TextMarginProperty);
        }
        private static object OnCoerceScale(DependencyObject d, object value)
        {
            Day day = d as Day;
            Double newValue = (Double)value;
            if (newValue < 1)
                return 1d;
            if (Double.IsNaN(newValue) || Double.IsPositiveInfinity(newValue)) return DependencyProperty.UnsetValue;
            return newValue;
        }
        internal static bool IsValidScale(object value)
        {
            if((double)value <= 1)
            { }
            Double scale = (Double)value;
            bool result = scale > 0 && !Double.IsNaN(scale) && !Double.IsInfinity(scale);
            return result;
        }
        private void AnimateScale()
        {
            DoubleAnimation anime = new DoubleAnimation();
            anime.Duration = _AnimationLength;
            anime.To = _Scale;
            BeginAnimation(ScaleProperty, anime, HandoffBehavior.Compose);
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
                new FrameworkPropertyMetadata(
                    SystemColors.ControlTextBrush,
                    FrameworkPropertyMetadataOptions.Inherits));
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
                new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily,
                    FrameworkPropertyMetadataOptions.Inherits,
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
                new FrameworkPropertyMetadata(SystemFonts.MessageFontSize,
                    FrameworkPropertyMetadataOptions.Inherits,
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
                new FrameworkPropertyMetadata(
                    TextElement.FontStretchProperty.DefaultMetadata.DefaultValue,
                    FrameworkPropertyMetadataOptions.Inherits,
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
                new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle,
                    FrameworkPropertyMetadataOptions.Inherits,
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
                new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight,
                    FrameworkPropertyMetadataOptions.Inherits,
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
        private double DaySeconds => (Date.AddDays(1) - Date).TotalSeconds;
        private double DaySize => DaySeconds / Scale;
        #endregion Properties
        #region Layout
        protected override Size MeasureOverride(Size availableSize)
        {
            //Height will be unbound. Width will be bound to UI space.
            Size extent = new Size(0, 0);
            double biggestChildWidth = TextMargin;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                double x = 0;
                Size childSize = new Size(availableSize.Width, double.PositiveInfinity);
                if (child is CalendarObject)
                {
                    x = TextMargin;
                    childSize.Width -= x;
                    biggestChildWidth = Math.Max(biggestChildWidth, child.DesiredSize.Width + x);
                }
                else
                {
                    biggestChildWidth = Math.Max(biggestChildWidth, child.DesiredSize.Width);
                }
                child.Measure(childSize);
            }
            extent.Width = biggestChildWidth;
            extent.Height = DaySize;
            VerifyScrollData(availableSize, extent);
            return _Viewport;
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size extent = new Size(0, 0);
            double biggestChildWidth = TextMargin;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                double x = 0;
                double y = 0; //12:00:00 AM
                Size childSize = child.DesiredSize;
                if (child is CalendarObject)
                {
                    CalendarObject CalObj = child as CalendarObject;
                    CalObj.Scale = Scale;
                    //set y relative to object start
                    //y = 0 is Date = 12:00:00 AM
                    x = TextMargin;
                    y = (CalObj.Start - Date).TotalSeconds / Scale;
                    childSize.Width = arrangeSize.Width - x;
                    childSize.Height = (CalObj.End - CalObj.Start).TotalSeconds / Scale;
                    biggestChildWidth = Math.Max(biggestChildWidth, childSize.Width + x);
                }
                else
                {
                    biggestChildWidth = Math.Max(biggestChildWidth, childSize.Width);
                }
                child.Arrange(new Rect(new Point(x - Offset.X, y - Offset.Y), childSize));
            }
            extent.Width = biggestChildWidth;
            extent.Height = DaySize;
            VerifyScrollData(arrangeSize, extent);
            return arrangeSize;
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            //Draw Grid lines and margin text
            DrawHorizontalGridLines(dc);
            //Draw Margin separator line
            dc.DrawLine(GridRegularPen, new Point(TextMargin, 0), new Point(TextMargin, DaySize));
        }
        private void DrawHorizontalGridLines(DrawingContext dc)
        {
            if (!(Scale >= 1 && Scale <= 900)) return;
            //area to work with, only draw within a margin of this area
            Rect area = new Rect(new Point(Offset.X, Offset.Y), _Viewport);
            Pen currentPen = GridRegularPen;
            string timeFormat = "";
            string minorFormat = "";
            string regularFormat = "";
            string majorFormat = "";
            double y = 0;
            bool minorGridLines = false;
            bool regularGridLines = false;
            bool majorGridLines = false;
            double secondsInterval = 21600d;
            double screenInterval;
            int regularSkip = 1;
            int majorSkip = 1;
            int maxIntervals;
            int iStart;
            int iEnd;
            int buffer = 2;

            //left of each grid line display time
            //hour: 4 PM
            //minute: 4:45 PM
            //depending on scale, draw more or less grid lines
            if (Scale == 1)
            {
                //if scale = 1s/px, 1m/60px, 1h/3600px
                secondsInterval = 15d; //15s
                regularSkip = 4; //1m
                majorSkip = 240; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "ss";
                regularFormat = "h:mm:ss tt";
                majorFormat = "h:mm:ss tt";
            }
            else if (Scale < 2)
            {
                secondsInterval = 30d; //30s
                regularSkip = 2; //1m
                majorSkip = 120; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "ss";
                regularFormat = "h:mm:ss tt";
                majorFormat = "h:mm:ss tt";
            }
            else if (Scale < 4)
            {
                secondsInterval = 60d; //1m
                regularSkip = 5; //5m
                majorSkip = 60; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "mm";
                regularFormat = "mm";
                majorFormat = "h:mm tt";
            }
            else if (Scale < 15)
            {
                secondsInterval = 300d; //5m
                regularSkip = 3; //15m
                majorSkip = 12; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "mm";
                regularFormat = "mm";
                majorFormat = "h:mm tt";
            }
            else if (Scale < 30)
            {
                secondsInterval = 900d; //15m
                regularSkip = 2; //30m
                majorSkip = 4; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "mm";
                regularFormat = "mm";
                majorFormat = "h:mm tt";
            }
            else if (Scale < 60)
            {
                //if scale < 60s/px, 1m/px, 1h/60px, 24h/1440px
                secondsInterval = 1800d; //30m
                regularSkip = 2; //1h
                majorSkip = 12; //6h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "mm";
                regularFormat = "h:mm tt";
                majorFormat = "h:mm tt";
            }
            else if (Scale < 240)
            {
                //if scale < 240s/px, 4m/px, 1h/15px, 24h/360px
                secondsInterval = 3600d; //1h
                regularSkip = 1; //1h
                majorSkip = 6; //6h
                minorGridLines = false;
                regularGridLines = true;
                majorGridLines = true;
                regularFormat = "h tt";
                majorFormat = "h tt";
            }
            else if (Scale < 600)
            {
                secondsInterval = 10800d; //3h
                regularSkip = 2; //6h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = false;
                minorFormat = "h tt";
                regularFormat = "h tt";
            }
            else if (Scale < 900)
            {
                //if scale < 900s/px, 15m/px, 1h/4px, 24h/96px
                secondsInterval = 21600d; //6h
                regularSkip = 1;
                minorGridLines = false;
                regularGridLines = true;
                majorGridLines = false;
                regularFormat = "h tt";
            }
            screenInterval = secondsInterval / Scale;
            maxIntervals = (int)(DaySeconds / secondsInterval);
            //restrict number of draws to within one interval of area 
            iStart = (int)(area.Y / screenInterval - buffer).Within(0, maxIntervals);
            iEnd = (int)((area.Y + area.Height) / screenInterval + buffer).Within(0, maxIntervals);
            for (int i = iStart; i < iEnd; i++)
            {
                if (majorGridLines && i % majorSkip == 0)
                {
                    currentPen = GridMajorPen;
                    timeFormat = majorFormat;
                }
                else if (regularGridLines && i % regularSkip == 0)
                {
                    currentPen = GridRegularPen;
                    timeFormat = regularFormat;
                }
                else if (minorGridLines)
                {
                    currentPen = GridMinorPen;
                    timeFormat = minorFormat;
                }
                else continue;
                y = i * screenInterval;
                double finalY = y - Offset.Y;
                double finalX1 = TextMargin - Offset.X;
                double finalX2 = ActualWidth - Offset.X;
                dc.DrawLine(currentPen, new Point(finalX1, finalY), new Point(finalX2, finalY));
                string text = Date.AddSeconds(y * Scale).ToString(timeFormat);
                FormattedText lineText = new FormattedText(text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.RightToLeft,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    FontSize, Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                dc.DrawText(lineText, new Point(finalX1 - 4, finalY - lineText.Height / 2));
            }
        }
        #endregion Layout
        #region IScrollInfo
        public ScrollViewer ScrollOwner
        {
            get { return _ScrollOwner; }
            set { _ScrollOwner = value; }
        }
        public bool CanVerticallyScroll
        {
            get { return _CanVerticallyScroll; }
            set { _CanVerticallyScroll = value; }
        }
        public bool CanHorizontallyScroll
        {
            get { return _CanHorizontallyScroll; }
            set { _CanHorizontallyScroll = value; }
        }
        public double ExtentWidth => _Extent.Width;
        public double ExtentHeight => _Extent.Height;
        public double ViewportWidth => _Viewport.Width;
        public double ViewportHeight => _Viewport.Height;
        public double HorizontalOffset => _Offset.X;
        public double VerticalOffset => _Offset.Y;
        public void LineUp() { SetVerticalOffset(VerticalOffset - LineSize); }
        public void LineDown() { SetVerticalOffset(VerticalOffset + LineSize); }
        public void LineLeft() { SetHorizontalOffset(HorizontalOffset - LineSize); }
        public void LineRight() { SetHorizontalOffset(HorizontalOffset + LineSize); }
        public void PageUp() { SetVerticalOffset(VerticalOffset - ViewportHeight); }
        public void PageDown() { SetVerticalOffset(VerticalOffset + ViewportHeight); }
        public void PageLeft() { SetHorizontalOffset(HorizontalOffset - ViewportWidth); }
        public void PageRight() { SetHorizontalOffset(HorizontalOffset + ViewportWidth); }
        public void MouseWheelUp() { SetVerticalOffset(VerticalOffset - WheelSize); }
        public void MouseWheelDown() { SetVerticalOffset(VerticalOffset + WheelSize); }
        public void MouseWheelLeft() { SetHorizontalOffset(HorizontalOffset - WheelSize); }
        public void MouseWheelRight() { SetHorizontalOffset(HorizontalOffset + WheelSize); }
        public void SetHorizontalOffset(double offset)
        {
            offset = offset.Within(0, ExtentWidth - ViewportWidth);
            if (offset != _Offset.Y)
            {
                _Offset.X = offset;
                InvalidateArrange();
                InvalidateVisual();
                AnimateOffset();
            }
        }
        public void SetVerticalOffset(double offset)
        {
            offset = offset.Within(0, ExtentHeight - ViewportHeight);
            if (offset != _Offset.Y)
            {
                _Offset.Y = offset;
                InvalidateArrange();
                InvalidateVisual();
                AnimateOffset();
            }
        }
        protected void VerifyScrollData(Size viewport, Size extent)
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
        private static double CalculateNewScrollOffset(double topView, double bottomView, double topChild, double bottomChild)
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
        #endregion IScrollInfo
    }
}
