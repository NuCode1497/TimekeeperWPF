﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;

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
            if (!IsMimicking) PropagateMimicry();
        }
        private void _Timer_Tick(object sender, EventArgs e)
        {
            //determine the state of this object based on the current time
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

            //InvalidateArrange();
        }
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
                nameof(End), typeof(DateTime), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(DateTime.Now.Date.AddHours(2).AddMinutes(17)));
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
                nameof(Start), typeof(DateTime), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(DateTime.Now.Date.AddHours(1).AddMinutes(33)));
        #endregion Start
        public bool Intersects(DateTime dt) { return Start <= dt && dt <= End; }
        public bool Intersects(Note N) { return Intersects(N.DateTime); }
        public bool Intersects(CalendarNoteObject C) { return Intersects(C.DateTime); }
        public bool Intersects(CheckIn CI) { return Intersects(CI.DateTime); }
        public bool Intersects(CalendarCheckIn CI) { return Intersects(CI.DateTime); }
        public bool Intersects(DateTime start, DateTime end) { return start < End && Start < end; }
        public bool Intersects(IZone Z) { return Intersects(Z.Start, Z.End); }
        public bool IsInside(DateTime start, DateTime end) { return start < Start && End < end; }
        public bool IsInside(IZone Z) { return Z.Start < Start && End < Z.End; }
        #endregion Zone
        #region ParentMap
        public CalendarTimeTaskMap ParentMap
        {
            get { return (CalendarTimeTaskMap)GetValue(ParentMapProperty); }
            set { SetValue(ParentMapProperty, value); }
        }
        public static readonly DependencyProperty ParentMapProperty =
            DependencyProperty.Register(
                nameof(ParentMap), typeof(CalendarTimeTaskMap), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(null,
                    null,
                    new CoerceValueCallback(OnCoerceParentMap)));
        private static CalendarTimeTaskMap OnCoerceParentMap(DependencyObject d, object value)
        {
            CalendarTaskObject CalObj = d as CalendarTaskObject;
            CalendarTimeTaskMap NewValue = (CalendarTimeTaskMap)value;
            if (CalObj.ParentMap != null)
                CalObj.ParentMap.TimeTask.PropertyChanged -= CalObj.OnParentEntityPropertyChanged;
            NewValue.TimeTask.PropertyChanged += CalObj.OnParentEntityPropertyChanged;
            CalObj.TaskType = NewValue.TimeTask.TaskType;
            return NewValue;
        }
        #endregion
        #region ParentPerZone
        public PerZone ParentPerZone
        {
            get { return (PerZone)GetValue(ParentPerZoneProperty); }
            set { SetValue(ParentPerZoneProperty, value); }
        }
        public static readonly DependencyProperty ParentPerZoneProperty =
            DependencyProperty.Register(
                nameof(ParentPerZone), typeof(PerZone), typeof(CalendarTaskObject));
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
            CalObj.SetValue(TaskTypeNamePropertyKey, value.Name);
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
            AutoCompleted,   //MediumAquamarine
            AutoConfirm,    //Aquamarine
            CheckIn,        //DodgerBlue
            Completed,      //LimeGreen
            Confirmed,      //SpringGreen
            Conflict,       //Pink
            Current,        //Azure
            Cancel,     //Crimson
            Insufficient,   //Orange
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
                new FrameworkPropertyMetadata(States.Unconfirmed,
                    new PropertyChangedCallback(OnStateChanged)));
        public static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarTaskObject CalObj = d as CalendarTaskObject;
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
                case States.Cancel:
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
                case States.AutoCompleted:
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
            private set { SetValue(StateColorPropertyKey, value); }
        }
        private static readonly DependencyPropertyKey StateColorPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(StateColor), typeof(SolidColorBrush), typeof(CalendarTaskObject),
                new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty StateColorProperty =
            StateColorPropertyKey.DependencyProperty;
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
            ToolTip = CalObj.ToolTip;
            ParentMap = CalObj.ParentMap;
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
                    ParentMap.TimeTask.PropertyChanged -= OnParentEntityPropertyChanged;
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
