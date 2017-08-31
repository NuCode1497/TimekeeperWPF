using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TimekeeperWPF
{
    public class Day : Panel
    {
        public Day() : base()
        {
        }
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

        }
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty = 
            DependencyProperty.Register(
                nameof(Scale), typeof(double), typeof(Day),
                new FrameworkPropertyMetadata(60d,
                    FrameworkPropertyMetadataOptions.AffectsArrange),
                new ValidateValueCallback(IsValidScale));
        private static bool IsValidScale(object value)
        {
            Double scale = (Double)value;
            return scale >= 1
                && !scale.Equals(Double.PositiveInfinity);
        }
        public TimeSpan DayLength => Date.AddDays(1) - Date;
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
                double yMax = DayLength.TotalSeconds / Scale;
                Size size = child.DesiredSize;

                if(child is CalendarObject)
                {
                    CalendarObject CalObj = child as CalendarObject;
                    CalObj.Scale = Scale;
                    //set y relative to object start
                    //y = 0 is 12:00:00 AM of Date
                    y = (CalObj.Start - Date).TotalSeconds / Scale;
                    size.Width = arrangeSize.Width - x;
                    size.Height = (CalObj.End - CalObj.Start).TotalSeconds / Scale;
                }
                child.Arrange(new Rect(x, y, size.Width, size.Height));
            }
            return arrangeSize;
        }
    }
}
