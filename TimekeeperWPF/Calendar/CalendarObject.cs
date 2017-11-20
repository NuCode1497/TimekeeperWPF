using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public class CalendarObject : ContentControl, IDisposable
    {
        static CalendarObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(typeof(CalendarObject)));
            BackgroundProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(
                    new SolidColorBrush() { Opacity = 0.5d, Color = Colors.Tomato }));
            BorderBrushProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(Brushes.DarkSlateGray));
            BorderThicknessProperty.OverrideMetadata(typeof(CalendarObject), 
                new FrameworkPropertyMetadata(new Thickness(2)));
        }
        public CalendarObject()
        {
            Day._Timer.Tick += _Timer_Tick;
            if (ParentMap != null)
                ParentMap.TimeTask.PropertyChanged += OnParentEntityPropertyChanged;
        }
        private void OnParentEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TypedLabeledEntity.TaskType))
            {
                TypedLabeledEntity entity = (TypedLabeledEntity)sender;
                TaskType = entity.TaskType;
            }
        }
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropagateMimicry();
        }
        private void _Timer_Tick(object sender, EventArgs e)
        {
            //determine the state of this object based on the current time
            if (DateTime.Now < Start)
            {
            }
            else if (DateTime.Now > End)
            {

            }
            else
            {
                State = States.Current;
            }

            //InvalidateArrange();
        }
        private CalendarTimeTaskMap _ParentMap;
        public CalendarTimeTaskMap ParentMap
        {
            get { return _ParentMap; }
            set
            {
                if (value == _ParentMap) return;
                if (_ParentMap != null)
                    _ParentMap.TimeTask.PropertyChanged -= OnParentEntityPropertyChanged;
                _ParentMap = value;
                _ParentMap.TimeTask.PropertyChanged += OnParentEntityPropertyChanged;
                TaskType = _ParentMap.TimeTask.TaskType;
            }
        }
        public TimeSpan Duration => End - Start;
        public string DurationString()
        {
            string s = "";
            if (Duration.Days > 0) s += Duration.Days + " days ";
            if (Duration.Hours > 0) s += Duration.Hours + " hours ";
            if (Duration.Minutes > 0) s += Duration.Minutes + " minutes ";
            return s;
        }
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
        #endregion End
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
        #endregion Scale
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
        #endregion Start
        #region TaskType
        public TaskType TaskType
        {
            get { return (TaskType)GetValue(TaskTypeProperty); }
            set { SetValue(TaskTypeProperty, value); }
        }
        public static readonly DependencyProperty TaskTypeProperty =
            DependencyProperty.Register(
                nameof(TaskType), typeof(TaskType), typeof(CalendarObject),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnTaskTypeChanged)));
        public static void OnTaskTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarObject CalObj = d as CalendarObject;
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
                nameof(TaskTypeName), typeof(string), typeof(CalendarObject),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty TaskTypeNameProperty =
            TaskTypeNamePropertyKey.DependencyProperty;
        #endregion TaskType
        public bool Intersects(DateTime dt) { return Start <= dt && dt <= End; }
        public bool Intersects(Note N) { return Intersects(N.DateTime); }
        public bool Intersects(DateTime start, DateTime end) { return start < End && Start < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarObject C) { return Intersects(C.Start, C.End); }
        #region States
        public enum States
        {
            Current,        //Azure
            Completed,      //LimeGreen
            Confirmed,      //SpringGreen
            Incomplete,     //Crimson
            Conflict,       //Pink
            Insufficient,   //Orange
            CheckIn,        //DodgerBlue
            Unscheduled,    //Chartreuse
            Unconfirmed,    //SkyBlue
            AutoCheckIn,    //MediumAquamarine
            AutoConfirm,    //Aquamarine
        }
        public States State
        {
            get { return (States)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        public static DependencyProperty StateProperty =
            DependencyProperty.Register(
                nameof(State), typeof(States), typeof(CalendarObject),
                new FrameworkPropertyMetadata(States.Unconfirmed,
                    new PropertyChangedCallback(OnStateChanged)));
        public static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarObject CalObj = d as CalendarObject;
            States value = (States)e.NewValue;
            switch (value)
            {
                case States.Current:
                    CalObj.StateColor = Brushes.Azure;
                    break;
                case States.Completed:
                    CalObj.StateColor = Brushes.LimeGreen;
                    break;
                case States.Confirmed:
                    CalObj.StateColor = Brushes.SpringGreen;
                    break;
                case States.Incomplete:
                    CalObj.StateColor = Brushes.Crimson;
                    break;
                case States.Conflict:
                    CalObj.StateColor = Brushes.Pink;
                    break;
                case States.Insufficient:
                    CalObj.StateColor = Brushes.Orange;
                    break;
                case States.CheckIn:
                    CalObj.StateColor = Brushes.DodgerBlue;
                    break;
                case States.Unscheduled:
                    CalObj.StateColor = Brushes.Chartreuse;
                    break;
                case States.Unconfirmed:
                    CalObj.StateColor = Brushes.SkyBlue;
                    break;
                case States.AutoCheckIn:
                    CalObj.StateColor = Brushes.MediumAquamarine;
                    break;
                case States.AutoConfirm:
                    CalObj.StateColor = Brushes.Aquamarine;
                    break;
            }
        }
        public SolidColorBrush StateColor
        {
            get { return (SolidColorBrush)GetValue(StateColorProperty); }
            set { SetValue(StateColorProperty, value); }
        }
        public static DependencyProperty StateColorProperty =
            DependencyProperty.Register(
                nameof(StateColor), typeof(SolidColorBrush), typeof(CalendarObject));
        #endregion
        #region Shadow Clone
        /// <summary>
        ///This is configured such that CalendarObjects that normally span across more than 
        ///one day shall have additional copies for each day it occupies in this week, up to 7.
        ///DayOffset indicates the copy number and offsets by that number of days.
        /// </summary>
        public int DayOffset { get; set; } = 0;
        public bool IsPropagatingMimicry { get; private set; }
        public CalendarObject OriginalCalObj { get; set; } = null;
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
            ParentMap = CalObj.ParentMap;
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
                    _ParentMap.TimeTask.PropertyChanged -= OnParentEntityPropertyChanged;
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
