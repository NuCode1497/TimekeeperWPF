using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class CalendarObject : Control
    {
        static CalendarObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarObject), new FrameworkPropertyMetadata(typeof(CalendarObject)));
        }
        public CalendarObject()
        {
            Start = DateTime.Now.Date.AddMinutes(30);
            End = DateTime.Now.Date.AddHours(2);
            Scale = 60d;
            Background = Brushes.Tomato;
        }
        #region Properties
        #region Start
        public DateTime Start
        {
            get { return (DateTime)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register(
                nameof(Start), typeof(DateTime), typeof(CalendarObject),
                new FrameworkPropertyMetadata(DateTime.Now.Date));
        #endregion
        #region End
        public DateTime End
        {
            get { return (DateTime)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register(
                nameof(End), typeof(DateTime), typeof(CalendarObject),
                new FrameworkPropertyMetadata(DateTime.Now.Date.AddHours(1)));
        #endregion
        #region Scale
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(
                nameof(Scale), typeof(double), typeof(CalendarObject),
                new FrameworkPropertyMetadata(60d),
                new ValidateValueCallback(IsValidScale));
        private static bool IsValidScale(object value)
        {
            Double scale = (Double)value;
            return scale >= 1
                && !scale.Equals(Double.PositiveInfinity);
        }
        #endregion
        #endregion
    }
}
