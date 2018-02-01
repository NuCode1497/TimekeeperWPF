using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;
using static System.Math;

namespace TimekeeperWPF.Calendar
{
    public class CalendarTaskObject : ContentControl, IDisposable, IZone
    {
        static CalendarTaskObject()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarTaskObject), 
                new FrameworkPropertyMetadata(typeof(CalendarTaskObject)));
            BackgroundProperty.OverrideMetadata(typeof(CalendarTaskObject), 
                new FrameworkPropertyMetadata(
                    new SolidColorBrush() { Opacity = 0.5d, Color = Colors.Tomato }));
            BorderBrushProperty.OverrideMetadata(typeof(CalendarTaskObject), 
                new FrameworkPropertyMetadata(Brushes.DarkSlateGray));
            BorderThicknessProperty.OverrideMetadata(typeof(CalendarTaskObject), 
                new FrameworkPropertyMetadata(new Thickness(2)));
        }
        public CalendarTaskObject()
        {
            Day._Timer.Tick += _Timer_Tick;
            if (ParentPerZone?.ParentMap != null)
                ParentPerZone.ParentMap.TimeTask.PropertyChanged += OnParentEntityPropertyChanged;
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
            if (!IsMimicking) PropagateMimicry();
        }
        private void _Timer_Tick(object sender, EventArgs e)
        {
            DetermineCurrentState();
            BuildToolTip();
        }
        private void DetermineCurrentState()
        {
            if (DateTime.Now < Start)
            {
            }
            else
            if (DateTime.Now > End)
            {
                switch (State)
                {
                    case States.Current:
                        State = _State;
                        break;
                    case States.Confirmed:
                        State = States.Completed;
                        break;
                    case States.AutoConfirm:
                        State = States.AutoCompleted;
                        break;
                    case States.Unconfirmed:
                        State = States.CheckIn;
                        break;
                }
            }
            else
            {
                if (State != States.Current)
                {
                    _State = State;
                    State = States.Current;
                }
            }
        }
        private void BuildToolTip()
        {
            string s = String.Format("D[{0}] {1} - {2}\nP[{3}] {4}\nDur: {5:g}" +
                "\nS  [{7}]{6}\nE  [{9}]{8}\nPS [{10}]\nPE [{11}]\nZS [{12}]\nZE [{13}]\nAs: {14}\nFs: {15}\nLs: {16}",
                ParentPerZone.ParentMap.TimeTask.Dimension,
                ParentPerZone.ParentMap.TimeTask.TaskType,
                ParentPerZone.ParentMap.TimeTask,
                ParentPerZone.ParentMap.TimeTask.Priority,
                State == States.Current ? State + $" ({_State})" : State.ToString(),
                Duration.ShortGoodString(),
                StartLock ? "🔒" : "",
                Start.ToString(),
                EndLock ? "🔒" : "",
                End.ToString(),
                ParentPerZone.Start.ToString(),
                ParentPerZone.End.ToString(),
                ParentInclusionZone?.Start.ToString(),
                ParentInclusionZone?.End.ToString(),
                ParentPerZone.ParentMap.TimeTask.AllocationsToString,
                ParentPerZone.ParentMap.TimeTask.FiltersToString,
                ParentPerZone.ParentMap.TimeTask.LabelsToString
                );
            if (ParentPerZone.TimeConsumption != null)
            {
                s += String.Format("\nAlloc: {0} {2}\nRem: {1}",
                ParentPerZone.TimeConsumption.Allocation.AmountAsTimeSpan.ShortGoodString(),
                ParentPerZone.TimeConsumption.RemainingAsTimeSpan.ShortGoodString(),
                ParentPerZone.ParentMap.TimeTask.AllocationMethod);
            }
            if (ParentPerZone.ParentMap.TimeTask.CanFill) s += "\nCanFill";
            if (ParentPerZone.ParentMap.TimeTask.AutoCheckIn) s += "\nAutoCheckIn";
            ToolTip = s;
        }
        public override string ToString()
        {
            string s = String.Format("{0} P{1} {2} S{3} E{4} ZS{5} ZE{6}",
                ParentPerZone.ParentMap.TimeTask.ToString(),
                ParentPerZone.ParentMap.TimeTask.Priority,
                Start.ToString("y-M-d"),
                (StartLock ? "🔒" : "") +
                Start.ToShortTimeString(),
                (EndLock ? "🔒" : "") +
                End.ToShortTimeString(),
                ParentInclusionZone?.Start.ToShortTimeString(),
                ParentInclusionZone?.End.ToShortTimeString());
            if (ParentPerZone.TimeConsumption != null)
            {
                s += String.Format(" Alloc: {0} {2} Rem: {1}",
                ParentPerZone.TimeConsumption.Allocation.AmountAsTimeSpan.ShortGoodString(),
                ParentPerZone.TimeConsumption.RemainingAsTimeSpan.ShortGoodString(),
                ParentPerZone.ParentMap.TimeTask.AllocationMethod);
            }
            return s;
        }
        internal bool ReDistFlag = true;
        internal bool Step1IgnoreFlag = false;
        public TimeTask TimeTask => ParentPerZone.ParentMap.TimeTask;
        public int Dimension => TimeTask.Dimension;
        public int DimensionCount { get; set; }
        public double Priority => TimeTask.Priority;
        public bool CanReDist => TimeTask.CanReDist && !StartLock && !EndLock;
        public bool CanFill => TimeTask.CanFill;
        public bool CanSplit => TimeTask.CanSplit && !StartLock && !EndLock;
        #region Zone
        public CalendarTaskObject LeftTangent { get; set; }
        public CalendarTaskObject RightTangent { get; set; }
        public TimeSpan Duration => End - Start;
        public string DurationString()
        {
            string s = "";
            if (Duration.Days > 0) s += Duration.Days + " days ";
            if (Duration.Hours > 0) s += Duration.Hours + " hours ";
            if (Duration.Minutes > 0) s += Duration.Minutes + " minutes ";
            return s;
        }
        internal bool HasIntersections(IEnumerable<CalendarTaskObject> calObjs)
        {
            foreach (var C in calObjs)
                if (C != this && C.Intersects(this))
                    return true;
            return false;
        }
        #region End
        public bool EndLock
        {
            get { return (bool)GetValue(EndLockProperty); }
            set { SetValue(EndLockProperty, value); }
        }
        public static readonly DependencyProperty EndLockProperty =
            DependencyProperty.Register(
                nameof(EndLock), typeof(bool), typeof(CalendarTaskObject));
        public DateTime End
        {
            get { return (DateTime)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }
        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register(
                nameof(End), typeof(DateTime), typeof(CalendarTaskObject));
        #endregion End
        #region Start
        public bool StartLock
        {
            get { return (bool)GetValue(StartLockProperty); }
            set { SetValue(StartLockProperty, value); }
        }
        public static readonly DependencyProperty StartLockProperty =
            DependencyProperty.Register(
                nameof(StartLock), typeof(bool), typeof(CalendarTaskObject));
        public DateTime Start
        {
            get { return (DateTime)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register(
                nameof(Start), typeof(DateTime), typeof(CalendarTaskObject));
        #endregion Start
        #endregion Zone
        #region ParentMap
        //public CalendarTimeTaskMap ParentMap
        //{
        //    get { return (CalendarTimeTaskMap)GetValue(ParentMapProperty); }
        //    set { SetValue(ParentMapProperty, value); }
        //}
        //public static readonly DependencyProperty ParentMapProperty =
        //    DependencyProperty.Register(
        //        nameof(ParentMap), typeof(CalendarTimeTaskMap), typeof(CalendarTaskObject),
        //        new FrameworkPropertyMetadata(null,
        //            null,
        //            new CoerceValueCallback(OnCoerceParentMap)));
        //private static CalendarTimeTaskMap OnCoerceParentMap(DependencyObject d, object value)
        //{
        //    CalendarTaskObject CalObj = d as CalendarTaskObject;
        //    CalendarTimeTaskMap NewValue = (CalendarTimeTaskMap)value;
        //    if (CalObj.ParentMap != null)
        //        CalObj.ParentMap.TimeTask.PropertyChanged -= CalObj.OnParentEntityPropertyChanged;
        //    NewValue.TimeTask.PropertyChanged += CalObj.OnParentEntityPropertyChanged;
        //    CalObj.TaskType = NewValue.TimeTask.TaskType;
        //    return NewValue;
        //}
        #endregion
        #region ParentPerZone
        public PerZone ParentPerZone
        {
            get { return (PerZone)GetValue(ParentPerZoneProperty); }
            set { SetValue(ParentPerZoneProperty, value); }
        }
        public static readonly DependencyProperty ParentPerZoneProperty =
            DependencyProperty.Register(
                nameof(ParentPerZone), typeof(PerZone), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(null,
                    null,
                    new CoerceValueCallback(OnCoerceParentPerZone)));
        private static PerZone OnCoerceParentPerZone(DependencyObject d, object value)
        {
            CalendarTaskObject CalObj = d as CalendarTaskObject;
            PerZone NewValue = (PerZone)value;
            if (CalObj.ParentPerZone != null)
                CalObj.ParentPerZone.ParentMap.TimeTask.PropertyChanged -= CalObj.OnParentEntityPropertyChanged;
            NewValue.ParentMap.TimeTask.PropertyChanged += CalObj.OnParentEntityPropertyChanged;
            CalObj.TaskType = NewValue.ParentMap.TimeTask.TaskType;
            return NewValue;
        }
        #endregion
        #region ParentInclusionZone
        public InclusionZone ParentInclusionZone
        {
            get { return (InclusionZone)GetValue(ParentInclusionZoneProperty); }
            set { SetValue(ParentInclusionZoneProperty, value); }
        }
        public static readonly DependencyProperty ParentInclusionZoneProperty =
            DependencyProperty.Register(
                nameof(ParentInclusionZone), typeof(InclusionZone), typeof(CalendarTaskObject));
        #endregion
        #region TaskType
        public TaskType TaskType
        {
            get { return (TaskType)GetValue(TaskTypeProperty); }
            set { SetValue(TaskTypeProperty, value); }
        }
        public static readonly DependencyProperty TaskTypeProperty =
            DependencyProperty.Register(
                nameof(TaskType), typeof(TaskType), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnTaskTypeChanged)));
        public static void OnTaskTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarTaskObject CalObj = d as CalendarTaskObject;
            TaskType value = (TaskType)e.NewValue;
            CalObj.SetValue(TaskTypeNamePropertyKey, value?.Name);
        }
        public string TaskTypeName
        {
            get { return (string)GetValue(TaskTypeNameProperty); }
            private set { SetValue(TaskTypeNamePropertyKey, value); }
        }
        private static readonly DependencyPropertyKey TaskTypeNamePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TaskTypeName), typeof(string), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty TaskTypeNameProperty =
            TaskTypeNamePropertyKey.DependencyProperty;
        #endregion TaskType
        #region States
        public bool Affirmed =>
            State == States.AutoCompleted ||
            State == States.AutoConfirm ||
            State == States.Completed ||
            State == States.Confirmed ||
            State == States.Cancel ||
            State == States.Insufficient ||
            State == States.Unscheduled;
        public enum States
        {
            AutoCompleted,  //MediumAquamarine
            AutoConfirm,    //Aquamarine
            CheckIn,        //DodgerBlue
            Completed,      //LimeGreen
            Confirmed,      //SpringGreen
            Conflict,       //Crimson
            Current,        //Azure
            Cancel,         //Pink
            Insufficient,   //Orange
            OverTime,       //DarkOrange
            Unconfirmed,    //SkyBlue
            Unscheduled,    //Chartreuse
        }
        public bool StateLock
        {
            get { return (bool)GetValue(StateLockProperty); }
            set { SetValue(StateLockProperty, value); }
        }
        public static readonly DependencyProperty StateLockProperty =
            DependencyProperty.Register(
                nameof(StateLock), typeof(bool), typeof(CalendarTaskObject));
        public States State
        {
            get { return (States)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        private States _State;
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                nameof(State), typeof(States), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(States.Unconfirmed));
        #endregion
        #region Shadow Clone
        // ShadowClones are used in WeekViewModel when a CalObj intersects the bounds of a day.
        // The ShadowClone is used to simulate the perception that the CalObj logically continues
        // into the next day even though the days are visually split.

        /// <summary>
        ///This is configured such that CalendarObjects that normally span across more than 
        ///one day shall have additional copies for each day it occupies in this week, up to 7.
        ///DayOffset indicates the copy number and offsets by that number of days.
        /// </summary>
        public int DayOffset { get; set; } = 0;
        public bool IsPropagatingMimicry { get; private set; }
        public bool IsMimicking { get; private set; }
        public CalendarTaskObject OriginalCalObj { get; set; } = null;
        private List<CalendarTaskObject> _Clones = null;
        public CalendarTaskObject ShadowClone()
        {
            //A ShadowClone mimics the original. If either the original or the ShadowClone has
            //a property changed, the change needs to be propagated to the original and other clones.
            CalendarTaskObject KageBunshin = new CalendarTaskObject();
            KageBunshin.OriginalCalObj = this;
            if (_Clones == null) _Clones = new List<CalendarTaskObject>();
            _Clones.Add(KageBunshin);
            PropagateMimicry();
            return KageBunshin;
        }
        public void Mimic(CalendarTaskObject CalObj)
        {
            IsMimicking = true;
            End = CalObj.End;
            EndLock = CalObj.EndLock;
            RightTangent = CalObj.RightTangent;
            Start = CalObj.Start;
            StartLock = CalObj.StartLock;
            LeftTangent = CalObj.LeftTangent;
            State = CalObj.State;
            StateLock = CalObj.StateLock;
            ParentPerZone = CalObj.ParentPerZone;
            ParentInclusionZone = CalObj.ParentInclusionZone;
            IsMimicking = false;
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
                    ParentPerZone.ParentMap.TimeTask.PropertyChanged -= OnParentEntityPropertyChanged;
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
