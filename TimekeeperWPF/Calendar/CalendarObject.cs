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
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF.Calendar
{
    public class CalendarObject : Control, IDisposable
    {
        static CalendarObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(typeof(CalendarObject)));
            BackgroundProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(new SolidColorBrush() { Opacity = 0.5d, Color = Colors.Tomato }));
            BorderBrushProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(Brushes.DarkSlateGray));
            BorderThicknessProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(new Thickness(2)));
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
        public int DayOffset { get; set; } = 0;
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
        #endregion
        public CalendarObject ShadowClone()
        {
            CalendarObject KageBunshin = new CalendarObject();
            KageBunshin.Scale = Scale;
            KageBunshin.End = End;
            KageBunshin.Start = Start;
            return KageBunshin;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Day._Timer.Tick -= _Timer_Tick;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CalendarObject() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
