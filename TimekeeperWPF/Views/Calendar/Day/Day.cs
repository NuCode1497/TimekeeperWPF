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

namespace TimekeeperWPF
{
    public class Day : Panel, IScrollInfo
    {
        public Day() : base()
        {
            Scale = 25d;
            BackgroundProperty.OverrideMetadata(typeof(Day),
                new FrameworkPropertyMetadata(Brushes.MintCream,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        }
        #region Events
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                int delta = e.Delta;
                //modify scale by 10%
                Scale = Scale * (1d + 0.1d * (e.Delta / Math.Abs(e.Delta)));
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
            set { SetValue(TextMarginProperty, value); }
        }
        public static readonly DependencyProperty TextMarginProperty =
            DependencyProperty.Register(
                nameof(TextMargin), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(80d,
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    null,
                    new CoerceValueCallback(OnCoerceTextMargin)));
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
        #region Scale
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
            Double newValue = (Double)value;
            if (newValue < 1) return 1;
            if (Double.IsNaN(newValue) || Double.IsPositiveInfinity(newValue)) return DependencyProperty.UnsetValue;
            return newValue;
        }
        private static bool IsValidScale(object value)
        {
            Double scale = (Double)value;
            return scale > 0
                && !Double.IsNaN(scale)
                && !Double.IsInfinity(scale);
        }
        #endregion
        #region Control-ish Properties

        /// <summary>
        ///     The DependencyProperty for the Foreground property.
        ///     Flags:              Can be used in style rules
        ///     Default Value:      System Font Color
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(
                typeof(Day), 
                new FrameworkPropertyMetadata(
                    SystemColors.ControlTextBrush, 
                    FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        ///     An brush that describes the foreground color.
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [Bindable(true), Category("Appearance")]
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        ///     The DependencyProperty for the FontFamily property.
        ///     Flags:              Can be used in style rules
        ///     Default Value:      System Dialog Font
        /// </summary>
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

        /// <summary>
        ///     The font family of the desired font.
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        ///     The DependencyProperty for the FontSize property.
        ///     Flags:              Can be used in style rules
        ///     Default Value:      System Dialog Font Size
        /// </summary>
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

        /// <summary>
        ///     The size of the desired font.
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        [Bindable(true), Category("Appearance")]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        ///     The DependencyProperty for the FontStretch property.
        ///     Flags:              Can be used in style rules
        ///     Default Value:      FontStretches.Normal
        /// </summary>
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

        /// <summary>
        ///     The stretch of the desired font.
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [Bindable(true), Category("Appearance")]
        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        /// <summary>
        ///     The DependencyProperty for the FontStyle property.
        ///     Flags:              Can be used in style rules
        ///     Default Value:      System Dialog Font Style
        /// </summary>
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

        /// <summary>
        ///     The style of the desired font.
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [Bindable(true), Category("Appearance")]
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        ///     The DependencyProperty for the FontWeight property.
        ///     Flags:              Can be used in style rules
        ///     Default Value:      System Dialog Font Weight
        /// </summary>
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

        /// <summary>
        ///     The weight or thickness of the desired font.
        ///     This will only affect controls whose template uses the property
        ///     as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [Bindable(true), Category("Appearance")]
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        #endregion
        private double DaySize => (Date.AddDays(1) - Date).TotalSeconds / Scale;
        #endregion Properties
        #region Layout
        protected override Size MeasureOverride(Size availableSize)
        {
            //Height will be unbound. Width will be bound to UI space.
            Size childAvailableSize = new Size(availableSize.Width - TextMargin, double.PositiveInfinity);
            Size extent = new Size(TextMargin, 0);
            double biggestChildWidth = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                child.Measure(childAvailableSize);
                biggestChildWidth = Math.Max(biggestChildWidth, child.DesiredSize.Width);
            }
            extent.Width += biggestChildWidth;
            extent.Height = DaySize;
            VerifyScrollData(availableSize, extent);
            return _Viewport;
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size extent = new Size(TextMargin, 0);
            double biggestChildWidth = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                double x = TextMargin;
                double y = 0; //12:00:00 AM
                Size childSize = child.DesiredSize;
                if (child is CalendarObject)
                {
                    CalendarObject CalObj = child as CalendarObject;
                    CalObj.Scale = Scale;
                    //set y relative to object start
                    //y = 0 is Date = 12:00:00 AM
                    y = (CalObj.Start - Date).TotalSeconds / Scale;
                    childSize.Width = arrangeSize.Width - x;
                    childSize.Height = (CalObj.End - CalObj.Start).TotalSeconds / Scale;
                }
                biggestChildWidth = Math.Max(biggestChildWidth, childSize.Width);
                child.Arrange(new Rect(new Point(x, y), childSize));
                TranslateTransform trans = child.RenderTransform as TranslateTransform;
                if (trans == null)
                {
                    child.RenderTransformOrigin = new Point(0, 0);
                    trans = new TranslateTransform();
                    child.RenderTransform = trans;
                }
                trans.BeginAnimation(TranslateTransform.YProperty,
                    new DoubleAnimation(-VerticalOffset, _ScrollAnimationLength), 
                    HandoffBehavior.SnapshotAndReplace);
                trans.BeginAnimation(TranslateTransform.XProperty,
                    new DoubleAnimation(-HorizontalOffset, _ScrollAnimationLength), 
                    HandoffBehavior.SnapshotAndReplace);
            }
            extent.Width += biggestChildWidth;
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
            dc.DrawLine(new Pen(Brushes.SlateGray, 2),
                new Point(TextMargin, 0), new Point(TextMargin, DaySize));
        }
        private void DrawHorizontalGridLines(DrawingContext dc)
        {
            //area to work with, only draw within a margin of this area
            Size area = _Viewport;

            if (Scale < 1 || Scale > 900) return;

            //TODO: make pens dep props
            Pen minorPen = new Pen(Brushes.Gray, 1);
            minorPen.DashStyle = DashStyles.Dash;
            Pen regularPen = new Pen(Brushes.Black, 1);
            Pen majorPen = new Pen(Brushes.Black, 2);
            Pen currentPen = regularPen;
            string timeFormat = "";
            string minorFormat = "";
            string regularFormat = "";
            string majorFormat = "";
            double x1 = TextMargin;
            double x2 = ActualWidth;
            double y = 0;
            bool minorGridLines = false;
            bool regularGridLines = false;
            bool majorGridLines = false;
            double minorInterval = 15d;
            double regularInterval = 60d;
            double majorInterval = 3600d;

            //left of each grid line display time
            //hour: 4 PM
            //minute: 4:45 PM
            //depending on scale, draw more or less grid lines
            if (Scale == 1)
            {
                //if scale = 1s/px, 1m/60px, 1h/3600px
                minorInterval = 15d; //15s
                regularInterval = 60d; //1m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "ss";
                regularFormat = "h:mm:ss tt";
                majorFormat = "h:mm:ss tt";
            }
            else if (Scale < 2)
            {
                minorInterval = 30d; //30s
                regularInterval = 60d; //1m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "ss";
                regularFormat = "h:mm:ss tt";
                majorFormat = "h:mm:ss tt";
            }
            else if (Scale < 5)
            {
                minorInterval = 60d; //1m
                regularInterval = 300d; //5m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "mm";
                regularFormat = "mm";
                majorFormat = "h:mm tt";
            }
            else if (Scale < 15)
            {
                minorInterval = 300d; //5m
                regularInterval = 900d; //15m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
                minorFormat = "mm";
                regularFormat = "mm";
                majorFormat = "h:mm tt";
            }
            else if (Scale < 30)
            {
                minorInterval = 900d; //15m
                regularInterval = 1800d; //30m
                majorInterval = 3600d; //1h
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
                minorInterval = 1800d; //30m
                regularInterval = 3600d; //1h
                majorInterval = 21600d; //6h
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
                minorInterval = regularInterval = 3600d; //1h
                majorInterval = 21600d; //6h
                minorGridLines = false;
                regularGridLines = true;
                majorGridLines = true;
                regularFormat = "h tt";
                majorFormat = "h tt";
            }
            else if (Scale < 600)
            {
                minorInterval = 10800d; //3h
                regularInterval = 21600d; //6h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = false;
                minorFormat = "h tt";
                regularFormat = "h tt";
            }
            else if (Scale < 900)
            {
                //if scale < 900s/px, 15m/px, 1h/4px, 24h/96px
                minorInterval = regularInterval = 21600d; //6h
                minorGridLines = false;
                regularGridLines = true;
                majorGridLines = false;
                regularFormat = "h tt";
            }
            minorInterval /= Scale;
            regularInterval /= Scale;
            majorInterval /= Scale;
            for (y = 0; y < DaySize; y += minorInterval)
            {
                //do this without %
                if (majorGridLines && y % majorInterval == 0)
                {
                    currentPen = majorPen;
                    timeFormat = majorFormat;
                }
                else if (regularGridLines && y % regularInterval == 0)
                {
                    currentPen = regularPen;
                    timeFormat = regularFormat;
                }
                else if (minorGridLines)
                {
                    currentPen = minorPen;
                    timeFormat = minorFormat;
                }
                else continue;
                dc.DrawLine(currentPen, new Point(x1, y), new Point(x2, y));
                string text = Date.AddSeconds(y * Scale).ToString(timeFormat);
                FormattedText lineText = new FormattedText(text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.RightToLeft,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    FontSize, Foreground,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);
                dc.DrawText(lineText, new Point(TextMargin - 4, y - lineText.Height / 2));
            }
        }
        #endregion Layout
        #region IScrollInfo
        private TimeSpan _ScrollAnimationLength = TimeSpan.FromMilliseconds(200);
        private const double LineSize = 16;
        private const double WheelSize = 3 * LineSize;
        private bool _CanHorizontallyScroll = false;
        private bool _CanVerticallyScroll = false;
        private ScrollViewer _ScrollOwner;
        private Vector _Offset = new Vector(0, 0);
        private Size _Extent = new Size(0, 0);
        private Size _Viewport = new Size(0, 0);
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
            offset = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));
            if (offset != _Offset.Y)
            {
                _Offset.X = offset;
                InvalidateArrange();
                InvalidateVisual();
            }
        }
        public void SetVerticalOffset(double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
            if (offset != _Offset.Y)
            {
                _Offset.Y = offset;
                InvalidateArrange();
                InvalidateVisual();
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
            _Offset.X = Math.Max(0, Math.Min(_Offset.X, ExtentWidth - ViewportWidth));
            _Offset.Y = Math.Max(0, Math.Min(_Offset.Y, ExtentHeight - ViewportHeight));
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
        #endregion
    }
}
