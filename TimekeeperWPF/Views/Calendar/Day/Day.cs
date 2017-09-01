using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimekeeperWPF
{
    public class Day : Panel
    {
        public Day() : base()
        {
            Scale = 20d;
        }
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
                new FrameworkPropertyMetadata(60d,
                    FrameworkPropertyMetadataOptions.AffectsArrange));
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
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    new PropertyChangedCallback(OnScaleChanged),
                    new CoerceValueCallback(OnCoerceScale)),
                new ValidateValueCallback(IsValidScale));
        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        private static object OnCoerceScale(DependencyObject d, object value)
        {
            Double newValue = (Double)value;
            Day day = (Day)d;
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
        #endregion
        private double YMax => (Date.AddDays(1) - Date).TotalSeconds / Scale;
        protected override Size MeasureOverride(Size availableSize)
        {
            //Height will be unbound. Width will be bound to UI space.
            availableSize.Height = double.PositiveInfinity;
            Size panelDesiredSize = new Size();
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                panelDesiredSize.Width = child.DesiredSize.Width;
            }
            //Size this panel to the selected day
            panelDesiredSize.Height = YMax;
            return panelDesiredSize;
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                double x = TextMargin;
                double y = 0; //12:00:00 AM
                Size size = child.DesiredSize;

                if (child is CalendarObject)
                {
                    CalendarObject CalObj = child as CalendarObject;
                    CalObj.Scale = Scale;
                    //set y relative to object start
                    //y = 0 is Date = 12:00:00 AM
                    y = (CalObj.Start - Date).TotalSeconds / Scale;
                    size.Width = arrangeSize.Width - x;
                    size.Height = (CalObj.End - CalObj.Start).TotalSeconds / Scale;
                }
                child.Arrange(new Rect(x, y, size.Width, size.Height));
            }
            return arrangeSize;
        }
        protected override void OnRender(DrawingContext dc)
        {
            DrawHorizontalGridLines(dc);
            dc.DrawLine(new Pen(Brushes.SlateGray,2), 
                new Point(TextMargin, 0), new Point(TextMargin, YMax));
        }

        private void DrawHorizontalGridLines(DrawingContext dc)
        {
            if (Scale < 1 || Scale > 900) return;
            //TODO: make pens dep props
            Pen minorPen = new Pen(Brushes.Gray, 1);
            minorPen.DashStyle = DashStyles.Dash;
            Pen regularPen = new Pen(Brushes.Black, 1);
            Pen majorPen = new Pen(Brushes.Black, 2);
            Pen currentPen = regularPen;
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
            }
            else if (Scale < 2)
            {
                minorInterval = 30d; //30s
                regularInterval = 60d; //1m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
            }
            else if (Scale < 5)
            {
                minorInterval = 60d; //1m
                regularInterval = 300d; //5m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
            }
            else if (Scale < 15)
            {
                minorInterval = 300d; //5m
                regularInterval = 900d; //15m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
            }
            else if (Scale < 30)
            {
                minorInterval = 900d; //15m
                regularInterval = 1800d; //30m
                majorInterval = 3600d; //1h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = true;
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
            }
            else if (Scale < 240)
            {
                //if scale < 240s/px, 4m/px, 1h/15px, 24h/360px
                minorInterval = regularInterval = 3600d; //1h
                majorInterval = 21600d; //6h
                minorGridLines = false;
                regularGridLines = true;
                majorGridLines = true;
            }
            else if (Scale < 600)
            {
                minorInterval = 10800d; //3h
                regularInterval = 21600d; //6h
                minorGridLines = true;
                regularGridLines = true;
                majorGridLines = false;
            }
            else if (Scale < 900)
            {
                //if scale < 900s/px, 15m/px, 1h/4px, 24h/96px
                minorInterval = regularInterval = 21600d; //6h
                minorGridLines = false;
                regularGridLines = true;
                majorGridLines = false;
            }
            minorInterval /= Scale;
            regularInterval /= Scale;
            majorInterval /= Scale;
            for (y = 0; y < YMax; y += minorInterval)
            {
                if (majorGridLines && y % majorInterval == 0)
                {
                    currentPen = majorPen;
                }
                else if (regularGridLines && y % regularInterval == 0)
                {
                    currentPen = regularPen;
                }
                else if (minorGridLines)
                {
                    currentPen = minorPen;
                }
                else continue;
                dc.DrawLine(currentPen, new Point(x1, y), new Point(x2, y));
                DrawGridLineText(dc, TextMargin, y);
            }
        }
        private void DrawGridLineText(DrawingContext dc, double x, double y)
        {
            string text = Date.AddSeconds(y).ToShortTimeString();
            FormattedText lineText = new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.RightToLeft,
                new Typeface(
                    new FontFamily("Segoe UI"),
                    FontStyles.Normal,
                    FontWeights.DemiBold,
                    FontStretches.UltraExpanded),
                12,
                Brushes.Black,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(lineText, new Point(x - 4, y - lineText.Height/2));
        }
    }
}
