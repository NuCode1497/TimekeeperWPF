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
using System.Windows.Threading;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF.Calendar
{
    public class CalendarObject : Control
    {
        static CalendarObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarObject), new FrameworkPropertyMetadata(typeof(CalendarObject)));
            BackgroundProperty.OverrideMetadata(typeof(CalendarObject), new FrameworkPropertyMetadata(Brushes.Tomato));
        }
        public CalendarObject()
        {
            Day._Timer.Tick += _Timer_Tick;
        }
        #region Events
        private void _Timer_Tick(object sender, EventArgs e)
        {
            InvalidateArrange();
        }
        #endregion Events
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
                new FrameworkPropertyMetadata(DateTime.Now.Date.AddHours(1).AddMinutes(33)));
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
                new FrameworkPropertyMetadata(DateTime.Now.Date.AddHours(2).AddMinutes(17)));
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
                new ValidateValueCallback(Day.IsValidScale));
        #endregion
        #endregion
    }
}
