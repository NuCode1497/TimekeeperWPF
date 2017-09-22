﻿using System;
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
        #region Fields
        #endregion
        #region Constructors
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
        #endregion
        #region Events
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropagateMimicry();
        }
        private void _Timer_Tick(object sender, EventArgs e)
        {
            InvalidateArrange();
        }
        #endregion Events
        #region Properties
        public bool IsPropagatingMimicry { get; private set; }
        public CalendarObject OriginalCalObj { get; set; } = null;
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
        #region Kage Bunshin No Jutsu
        private List<CalendarObject> _Clones = null;
        public CalendarObject ShadowClone()
        {
            //A ShadowClone mimics the original. If either the original or the ShadowClone has
            //a property changed, the change needs to be propagated to the original and other clones.
            CalendarObject KageBunshin = new CalendarObject();
            KageBunshin.Mimic(this);
            KageBunshin.OriginalCalObj = this;
            if (_Clones == null) _Clones = new List<CalendarObject>();
            _Clones.Add(KageBunshin);
            return KageBunshin;
        }
        public void Mimic(CalendarObject CalObj)
        {
            Scale = CalObj.Scale;
            End = CalObj.End;
            Start = CalObj.Start;
            ToolTip = CalObj.ToolTip;
            PropagateMimicry();
        }
        public void PropagateMimicry()
        {
            if (IsPropagatingMimicry) return;
            IsPropagatingMimicry = true;
            if (OriginalCalObj != null && !OriginalCalObj.IsPropagatingMimicry)
            {
                OriginalCalObj.Mimic(this);
            }
            if (_Clones != null)
            {
                foreach (var clone in _Clones)
                {
                    if (!clone.IsPropagatingMimicry)
                    {
                        clone.Mimic(this);
                    }
                }
            }
            IsPropagatingMimicry = false;
        }
        #endregion
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
