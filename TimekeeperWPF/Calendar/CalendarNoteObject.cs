using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public class CalendarNoteObject : ContentControl, IDisposable
    {
        static CalendarNoteObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarNoteObject), 
                new FrameworkPropertyMetadata(typeof(CalendarNoteObject)));
        }
        public DateTime DateTime
        {
            get { return (DateTime)GetValue(DateTimeProperty); }
            set { SetValue(DateTimeProperty, value); }
        }
        public static readonly DependencyProperty DateTimeProperty =
            DependencyProperty.Register(
                nameof(DateTime), typeof(DateTime), typeof(CalendarNoteObject),
                new FrameworkPropertyMetadata(DateTime.Now));
        public TaskType TaskType
        {
            get { return (TaskType)GetValue(TaskTypeProperty); }
            set { SetValue(TaskTypeProperty, value); }
        }
        public static readonly DependencyProperty TaskTypeProperty =
            DependencyProperty.Register(
                nameof(TaskType), typeof(TaskType), typeof(CalendarNoteObject),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnTaskTypeChanged)));
        public static void OnTaskTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarNoteObject CalObj = d as CalendarNoteObject;
            TaskType value = (TaskType)e.NewValue;
            CalObj.SetValue(TaskTypeNamePropertyKey, value.Name);
        }
        public string TaskTypeName
        {
            get { return (string)GetValue(TaskTypeNameProperty); }
            private set { SetValue(TaskTypeNamePropertyKey, value); }
        }
        private static readonly DependencyPropertyKey TaskTypeNamePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TaskTypeName), typeof(string), typeof(CalendarNoteObject),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty TaskTypeNameProperty =
            TaskTypeNamePropertyKey.DependencyProperty;
        public bool Intersects(DateTime start, DateTime end) { return start < DateTime && DateTime < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
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
