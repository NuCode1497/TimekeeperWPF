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
        }
        #region Properties
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
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    OnDateChanged));
        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //update gridlines
        }
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
                    null,
                    new CoerceValueCallback(OnCoerceScale)),
                new ValidateValueCallback(IsValidScale));
        private static object OnCoerceScale(DependencyObject d, object value)
        {
            Double newValue = (Double)value;
            Day day = (Day)d;
            return newValue;
        }
        private static bool IsValidScale(object value)
        {
            Double scale = (Double)value;
            return scale >= 1
                && !scale.Equals(Double.PositiveInfinity);
        }
        #endregion
        #endregion
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
            TimeSpan DayLength = Date.AddDays(1) - Date;
            panelDesiredSize.Height = DayLength.TotalSeconds / Scale;
            return panelDesiredSize;
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                double x = 50;
                double y = 0; //12:00:00 AM
                Size size = child.DesiredSize;

                if(child is CalendarObject)
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
            //TODO: make pens dep props
            //3 line types: thick major, regular, dashed minor
            Pen major = new Pen(Brushes.Black, 2);
            Pen minor = new Pen(Brushes.Gray, 1);
            minor.DashStyle = DashStyles.Dash; 
            Pen regular = new Pen(Brushes.Black, 1);

            //left of each grid line display time
            //hour: 4 PM
            //minute: 4:45 PM
            //depending on scale, draw more or less grid lines
            //if scale = 1s/px, 1m/60px, 1h/3600px
            //line for each minute
            //dashed line for each 30s of minute
            //thick line for each hour

            //if scale = 60s/px, 1m/px, 1h/60px, 24h/1440px
            //line for each hour
            //dashed line for each 30m

            //if scale = 900s/px, 15m/px, 1h/4px, 24h/96px
            //no lines
        }
    }
}
