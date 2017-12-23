// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;
using TimekeeperWPF.Calendar;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using TimekeeperDAL.Tools;
using static System.Math;

namespace TimekeeperWPF
{
    public abstract class CalendarViewModel : TypedLabeledEntitiesViewModel<TimeTask>
    {
        #region Fields
        private NotesViewModel _NotesVM;
        private UIElement _SelectedCalendarObect;
        private DateTime _SelectedDate = DateTime.Now.Date;
        private Orientation _Orientation = Orientation.Vertical;
        private bool _Max = false;
        private bool _TextMargin = true;
        private int _ScaleSudoCommand;
        private ICommand _PreviousCommand;
        private ICommand _NextCommand;
        private ICommand _OrientationCommand;
        private ICommand _ScaleUpCommand;
        private ICommand _ScaleDownCommand;
        private ICommand _SelectWeekCommand;
        private ICommand _SelectDayCommand;
        private ICommand _SelectYearCommand;
        private ICommand _SelectMonthCommand;
        private ICommand _NewNoteCommand;
        #endregion Fields
        #region Events
        public event RequestViewChangeEventHandler RequestViewChange;
        protected virtual void OnRequestViewChange(RequestViewChangeEventArgs e)
        { RequestViewChange?.Invoke(this, e); }
        #endregion Events
        #region Properties
        public NotesViewModel NotesVM
        {
            get { return _NotesVM; }
            set
            {
                _NotesVM = value;
                OnPropertyChanged();
            }
        }
        public List<CalendarTimeTaskMap> TaskMaps;
        public CollectionViewSource CalTaskObjsCollection { get; set; }
        public ObservableCollection<CalendarTaskObject> CalTaskObjsSource => CalTaskObjsCollection?.Source as ObservableCollection<CalendarTaskObject>;
        public ListCollectionView CalTaskObjsView => CalTaskObjsCollection?.View as ListCollectionView;
        public CollectionViewSource CalNoteObjsCollection { get; set; }
        public ObservableCollection<CalendarNoteObject> CalNoteObjsSource => CalNoteObjsCollection?.Source as ObservableCollection<CalendarNoteObject>;
        public ListCollectionView CalNoteObjsView => CalNoteObjsCollection?.View as ListCollectionView;
        public ObservableCollection<CheckIn> CheckIns { get; set; }
        public UIElement SelectedCalendarObject
        {
            get { return _SelectedCalendarObect; }
            set
            {
                if (_SelectedCalendarObect == value) return;
                if (value is NowMarkerHorizontal) return;
                if (value is NowMarkerVertical) return;
                if (value is CalendarNoteObject)
                    NotesVM.SelectedItem = ((CalendarNoteObject)value).Note;
                if (value is CalendarTaskObject)
                    SelectedItem = ((CalendarTaskObject)value).ParentMap.TimeTask;
                _SelectedCalendarObect = value;
                OnPropertyChanged();
            }
        }
        public virtual DateTime SelectedDate
        {
            get { return _SelectedDate; }
            set
            {
                DateTime newValue = value.Date;
                if (_SelectedDate == newValue) return;
                _SelectedDate = newValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedYear));
                OnPropertyChanged(nameof(SelectedMonth));
                OnPropertyChanged(nameof(SelectedDay));
                OnPropertyChanged(nameof(MonthString));
                OnPropertyChanged(nameof(YearString));
                OnPropertyChanged(nameof(DayLongString));
                OnPropertyChanged(nameof(WeekString));
                OnPropertyChanged(nameof(EndDate));
            }
        }
        public int SelectedYear => SelectedDate.Year;
        public int SelectedMonth => SelectedDate.Month;
        public int SelectedDay => SelectedDate.Day;
        public string DayLongString => SelectedDate.ToLongDateString();
        public string YearString => SelectedDate.ToString("yyy");
        public string MonthString => SelectedDate.ToString("MMMM");
        public string WeekString => SelectedDate.ToString("MMMM dd, yyy");
        public abstract DateTime EndDate { get; }
        public virtual Orientation Orientation
        {
            get{ return _Orientation; }
            set
            {
                if (_Orientation == value) return;
                _Orientation = value;
                OnPropertyChanged();
            }
        }
        public virtual bool Max
        {
            get { return _Max; }
            set
            {
                if (_Max == value) return;
                _Max = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanMax));
            }
        }
        public virtual bool TextMargin
        {
            get { return _TextMargin; }
            set
            {
                if (_TextMargin == value) return;
                _TextMargin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanTextMargin));
            }
        }
        public int ScaleSudoCommand
        {
            get { return _ScaleSudoCommand; }
            private set
            {
                _ScaleSudoCommand = value;
                OnPropertyChanged();
            }
        }
        #endregion Properties
        #region Commands
        public ICommand PreviousCommand => _PreviousCommand
            ?? (_PreviousCommand = new RelayCommand(async ap => await PreviousAsync(), pp => CanPrevious));
        public ICommand NextCommand => _NextCommand
            ?? (_NextCommand = new RelayCommand(async ap => await NextAsync(), pp => CanNext));
        public ICommand OrientationCommand => _OrientationCommand
            ?? (_OrientationCommand = new RelayCommand(ap => ToggleOrientation(), pp => CanOrientation));
        public ICommand ScaleUpCommand => _ScaleUpCommand
            ?? (_ScaleUpCommand = new RelayCommand(ap => ScaleUp(), pp => CanScaleUp));
        public ICommand ScaleDownCommand => _ScaleDownCommand
            ?? (_ScaleDownCommand = new RelayCommand(ap => ScaleDown(), pp => CanScaleDown));
        public ICommand SelectWeekCommand => _SelectWeekCommand
            ?? (_SelectWeekCommand = new RelayCommand(ap => SelectWeek(), pp => CanSelectWeek));
        public ICommand SelectDayCommand => _SelectDayCommand
            ?? (_SelectDayCommand = new RelayCommand(ap => SelectDay(), pp => CanSelectDay));
        public ICommand SelectYearCommand => _SelectYearCommand
            ?? (_SelectYearCommand = new RelayCommand(ap => SelectYear(), pp => CanSelectYear));
        public ICommand SelectMonthCommand => _SelectMonthCommand
            ?? (_SelectMonthCommand = new RelayCommand(ap => SelectMonth(), pp => CanSelectMonth));
        public ICommand NewNoteCommand => _NewNoteCommand
            ?? (_NewNoteCommand = new RelayCommand(ap => NewNote(), pp => CanAddNewNote));
        #endregion Commands
        #region Predicates
        protected virtual bool CanPrevious => IsNotLoading;
        protected virtual bool CanNext => IsNotLoading;
        protected virtual bool CanOrientation => true;
        public virtual bool CanMax => true;
        public virtual bool CanTextMargin => true;
        protected virtual bool CanScaleUp => true;
        protected virtual bool CanScaleDown => true;
        protected virtual bool CanSelectWeek => IsNotLoading;
        protected virtual bool CanSelectDay => IsNotLoading;
        protected virtual bool CanSelectYear => IsNotLoading;
        protected virtual bool CanSelectMonth => IsNotLoading;
        protected override bool CanAddNew => false;
        private bool CanAddNewNote => NotesVM?.NewItemCommand?.CanExecute(null) ?? false;
        protected override bool CanEditSelected => false;
        protected override bool CanSave => false;
        protected override bool CanDeleteSelected => false;
        public bool Intersects(DateTime dt) { return SelectedDate <= dt && dt <= EndDate; }
        public bool Intersects(Note N) { return Intersects(N.DateTime); }
        public bool Intersects(CalendarNoteObject C) { return Intersects(C.DateTime); }
        public bool Intersects(CheckIn CI) { return Intersects(CI.DateTime); }
        public bool Intersects(CalendarCheckIn CI) { return Intersects(CI.DateTime); }
        public bool Intersects(DateTime start, DateTime end) { return start < EndDate && SelectedDate < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }
        #endregion Predicates
        #region Actions
        protected virtual void SelectWeek()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Week, SelectedDate)); }
        protected virtual void SelectMonth()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Month, SelectedDate)); }
        protected virtual void SelectYear()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Year, SelectedDate)); }
        protected virtual void SelectDay()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Day, SelectedDate)); }
        protected override async Task GetDataAsync()
        {
            Context = new TimeKeeperContext();
            Status = "Getting data from database...";
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;
            NotesVM = new NotesViewModel();
            await NotesVM.LoadData();
            await base.GetDataAsync();
            await Context.CheckIns.LoadAsync();
            CheckIns = Context.CheckIns.Local;
            Status = "Creating CalendarObjects...";
            await CreateTaskObjects();
        }
        private async Task CreateTaskObjects()
        {
            //Build Data
            await BuildTaskMaps();
            BuildCheckIns();

            //Interpret Data
            AllocateTimeFromCheckIns();
            AllocateTimeFromFilters();

            //Organize Data
            CalculateCollisions();
            AllocateEmptySpace();
            CleanUpStates();
            UnZipTaskMaps();
        }
        #region BuildTaskMaps
        private async Task BuildTaskMaps()
        {
            //This function is more intensive than others, it will be called once on load
            //Reduce the TimeTask set
            var RelevantTasks = FindTaskSet(new HashSet<TimeTask>(), SelectedDate, EndDate);
            foreach (var T in RelevantTasks)
                await T.BuildPerZonesAsync();
            TaskMaps = new List<CalendarTimeTaskMap>();
            //Create Maps with all PerZones; we will reduce this set later
            foreach (var T in RelevantTasks)
            {
                var map = new CalendarTimeTaskMap
                {
                    TimeTask = T,
                    PerZones = new HashSet<PerZone>(),
                };
                foreach (var P in T.PerZones)
                {
                    var per = new PerZone
                    {
                        Start = P.Key,
                        End = P.Value,
                        InclusionZones = new List<InclusionZone>(),
                        CalTaskObjs = new HashSet<CalendarTaskObject>(),
                        CheckIns = new List<CalendarCheckIn>(),
                    };
                    map.PerZones.Add(per);
                }
                TaskMaps.Add(map);
            }
            var RelevantPerZones = FindPerSet(new HashSet<PerZone>(), SelectedDate, EndDate);
            foreach (var M in TaskMaps)
            {
                //Reduce the PerZone set
                M.PerZones = new HashSet<PerZone>(M.PerZones.Intersect(RelevantPerZones));
                //Build InclusionZones with the reduced PerZone set
                var pers = new Dictionary<DateTime, DateTime>();
                foreach (var P in M.PerZones)
                    pers.Add(P.Start, P.End);
                await M.TimeTask.BuildInclusionZonesAsync(pers);
                foreach (var P in M.PerZones)
                {
                    if (M.TimeTask.TimeAllocation != null)
                    {
                        P.TimeConsumption = new Consumption
                        {
                            Allocation = M.TimeTask.TimeAllocation,
                            Remaining = M.TimeTask.TimeAllocation.AmountAsTimeSpan().Ticks,
                        };
                    }
                    var inZones = M.TimeTask.InclusionZones.Where(Z => P.Intersects(Z.Key, Z.Value));
                    foreach (var Z in inZones)
                    {
                        var zone = new InclusionZone
                        {
                            Start = Z.Key,
                            End = Z.Value,
                        };
                        P.InclusionZones.Add(zone);
                    }
                }
            }
        }
        private HashSet<TimeTask> FindTaskSet(HashSet<TimeTask> accumulatedFinds, DateTime start, DateTime end)
        {
            //Recursively select the set of Tasks that intersect the calendar view or previously added tasks.
            var foundTasks = Source.Where(T => T.Intersects(start, end)).Except(accumulatedFinds);
            if (foundTasks.Count() == 0) return accumulatedFinds;
            accumulatedFinds.Union(foundTasks);
            foreach (var T in foundTasks)
            {
                accumulatedFinds.Union(FindTaskSet(accumulatedFinds, T.Start, T.End));
            }
            return accumulatedFinds;
        }
        private HashSet<PerZone> FindPerSet(HashSet<PerZone> accumulatedFinds, DateTime start, DateTime end)
        {
            //Recursively select the set of PerZones that intersect the calendar view or previously added PerZones.
            var foundPers = (from M in TaskMaps
                             from P in M.PerZones
                             where P.Intersects(start, end)
                             select P).Except(accumulatedFinds);
            if (foundPers.Count() == 0) return accumulatedFinds;
            accumulatedFinds.Union(foundPers);
            foreach (var P in foundPers)
            {
                accumulatedFinds.Union(FindPerSet(accumulatedFinds, P.Start, P.End));
            }
            return accumulatedFinds;
        }
        #endregion BuildTaskMaps
        private void BuildCheckIns()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    var eventCheckIns = new List<CalendarCheckIn>();
                    var perCheckIns = new List<CalendarCheckIn>();
                    var inZoneCheckIns = new List<CalendarCheckIn>();
                    //find and map relevant event CIs in this PerZone
                    var checkIns =
                        from CI in CheckIns
                        where CI.TimeTask == M.TimeTask
                        where P.Intersects(CI.DateTime)
                        select CI;
                    foreach (var CI in checkIns)
                    {
                        eventCheckIns.Add(new CalendarCheckIn
                        {
                            DateTime = CI.DateTime,
                            Kind = CI.Text == "start" ? CheckInKind.EventStart : CheckInKind.EventEnd,
                            ParentMap = M,
                            ParentPerZone = P,
                        });
                    }
                    //map zone ends as CheckIns
                    perCheckIns.Add(new CalendarCheckIn
                    {
                        DateTime = P.Start,
                        Kind = CheckInKind.PerZoneStart,
                        ParentMap = M,
                        ParentPerZone = P,
                    });
                    perCheckIns.Add(new CalendarCheckIn
                    {
                        DateTime = P.End,
                        Kind = CheckInKind.PerZoneEnd,
                        ParentMap = M,
                        ParentPerZone = P,
                    });
                    foreach (var Z in P.InclusionZones)
                    {
                        //find and map relevant event CIs in this InclusionZone
                        var CIsOverZ =
                            from CI in eventCheckIns
                            where Z.Intersects(CI)
                            select CI;
                        foreach (var CI in CIsOverZ)
                            CI.ParentInclusionZone = Z;
                        //map zone ends as CheckIns
                        inZoneCheckIns.Add(new CalendarCheckIn
                        {
                            DateTime = Z.Start,
                            Kind = CheckInKind.InclusionZoneStart,
                            ParentMap = M,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        });
                        inZoneCheckIns.Add(new CalendarCheckIn
                        {
                            DateTime = Z.End,
                            Kind = CheckInKind.InclusionZoneEnd,
                            ParentMap = M,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        });
                    }
                    //Merge and sort CheckIn sets.
                    var allCheckIns =
                        from CI in eventCheckIns.Union(perCheckIns).Union(inZoneCheckIns)
                        orderby CI.Kind, CI.DateTime
                        select CI;
                    P.CheckIns = new List<CalendarCheckIn>(allCheckIns);
                }
            }
        }
        private void AllocateTimeFromCheckIns()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    //Create CalObjs from CheckIns
                    CalendarTaskObject pC = null;
                    CalendarCheckIn pN = null;
                    bool inZ = false;
                    foreach (var N in P.CheckIns)
                    {
                        // Refer to Plans.xlsx - CheckIns
                        switch (N.Kind)
                        {
                            case CheckInKind.InclusionZoneStart:
                                // F
                                if (pN.Kind == CheckInKind.EventStart && inZ == false)
                                {
                                    // 4
                                    pC.End = N.DateTime;
                                }
                                inZ = true;
                                break;
                            case CheckInKind.EventStart:
                                // B D
                                if (inZ)
                                {
                                    // B
                                    switch (pN.Kind)
                                    {
                                        case CheckInKind.InclusionZoneStart:
                                            // 6
                                            var C = new CalendarTaskObject
                                            {
                                                Start = N.DateTime,
                                                End = N.DateTime,
                                                State = CalendarTaskObject.States.Confirmed,
                                                ParentMap = N.ParentMap,
                                                ParentInclusionZone = N.ParentInclusionZone,
                                                ParentPerZone = N.ParentPerZone,
                                                StartLock = true,
                                                StateLock = true,
                                            };
                                            pC = C;
                                            P.CalTaskObjs.Add(C);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 2
                                            pC.End = N.DateTime;
                                            goto case CheckInKind.InclusionZoneStart;
                                        case CheckInKind.EventEnd:
                                            // 3
                                            goto case CheckInKind.InclusionZoneStart;
                                    }
                                }
                                else
                                {
                                    // D
                                    switch (pN.Kind)
                                    {
                                        case CheckInKind.PerZoneStart:
                                            // 8
                                            var C = new CalendarTaskObject
                                            {
                                                Start = N.DateTime,
                                                End = N.DateTime,
                                                State = CalendarTaskObject.States.Unscheduled,
                                                ParentMap = N.ParentMap,
                                                ParentInclusionZone = N.ParentInclusionZone,
                                                ParentPerZone = N.ParentPerZone,
                                                StartLock = true,
                                                StateLock = true,
                                            };
                                            pC = C;
                                            P.CalTaskObjs.Add(C);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 4
                                            pC.End = N.DateTime;
                                            goto case CheckInKind.PerZoneStart;
                                        case CheckInKind.EventEnd:
                                            // 5
                                            goto case CheckInKind.PerZoneStart;
                                    }
                                }
                                break;
                            case CheckInKind.EventEnd:
                                // C E
                                if (inZ)
                                {
                                    // C
                                    switch (pN.Kind)
                                    {
                                        case CheckInKind.InclusionZoneStart:
                                            // 6
                                            var C = new CalendarTaskObject
                                            {
                                                Start = pN.DateTime,
                                                End = N.DateTime,
                                                State = CalendarTaskObject.States.Confirmed,
                                                ParentMap = N.ParentMap,
                                                ParentInclusionZone = N.ParentInclusionZone,
                                                ParentPerZone = N.ParentPerZone,
                                                EndLock = true,
                                                StateLock = true,
                                            };
                                            pC = C;
                                            P.CalTaskObjs.Add(C);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 2
                                            pC.End = N.DateTime;
                                            pC.EndLock = true;
                                            break;
                                        case CheckInKind.EventEnd:
                                            // 3
                                            goto case CheckInKind.InclusionZoneStart;
                                    }
                                }
                                else
                                {
                                    // E
                                    switch (pN.Kind)
                                    {
                                        case CheckInKind.PerZoneStart:
                                            // 8
                                            var C = new CalendarTaskObject
                                            {
                                                Start = pN.DateTime,
                                                End = N.DateTime,
                                                State = CalendarTaskObject.States.Unscheduled,
                                                ParentMap = N.ParentMap,
                                                ParentInclusionZone = N.ParentInclusionZone,
                                                ParentPerZone = N.ParentPerZone,
                                                EndLock = true,
                                                StateLock = true,
                                            };
                                            pC = C;
                                            P.CalTaskObjs.Add(C);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 4
                                            pC.End = N.DateTime;
                                            pC.EndLock = true;
                                            break;
                                        case CheckInKind.EventEnd:
                                            // 5
                                            var C2 = new CalendarTaskObject
                                            {
                                                Start = pC.End,
                                                End = N.DateTime,
                                                State = CalendarTaskObject.States.Unscheduled,
                                                ParentMap = N.ParentMap,
                                                ParentInclusionZone = N.ParentInclusionZone,
                                                ParentPerZone = N.ParentPerZone,
                                                EndLock = true,
                                                StateLock = true,
                                            };
                                            pC = C2;
                                            P.CalTaskObjs.Add(C2);
                                            break;
                                        case CheckInKind.InclusionZoneEnd:
                                            // 7
                                            goto case CheckInKind.PerZoneStart;
                                    }
                                }
                                break;
                            case CheckInKind.InclusionZoneEnd:
                                // G
                                if (pN.Kind == CheckInKind.EventStart && inZ == true)
                                {
                                    // 2
                                    pC.End = N.DateTime;
                                }
                                inZ = false;
                                break;
                            case CheckInKind.PerZoneEnd:
                                // H
                                if (pN.Kind == CheckInKind.EventStart && inZ == false)
                                {
                                    // 4
                                    pC.End = N.DateTime;
                                }
                                break;
                        }
                        pN = N;
                    }
                    //Add up all the time
                    TimeSpan spent = new TimeSpan();
                    foreach (var C in P.CalTaskObjs)
                        spent += C.Duration;
                    //Allocate time
                    P.TimeConsumption.Remaining -= spent.Ticks;
                }
            }
        }
        #region AllocateTimeFromFilters
        private void AllocateTimeFromFilters()
        {
            foreach (var M in TaskMaps)
            {
                if (M.TimeTask.TimeAllocation == null)
                    AllocateAllTime(M);
                else
                    AllocationMethod(M);
            }
        }
        private void AllocateAllTime(CalendarTimeTaskMap M)
        {
            // If no time allocation is set, we create one CalendarObject per inclusion zone
            // with each Start/End set to the bounds of the inclusion zone.
            foreach (var P in M.PerZones)
            {
                foreach (var Z in P.InclusionZones)
                {
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    //create cal object that matches zone
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.End,
                        ParentMap = M,
                        ParentPerZone = P,
                        ParentInclusionZone = Z,
                    };
                    P.CalTaskObjs.Add(CalObj);
                }
            }
        }
        private void AllocationMethod(CalendarTimeTaskMap M)
        {
            switch (M.TimeTask.AllocationMethod)
            {
                case "Eager":
                    EagerTimeAllocate(M);
                    break;
                case "Even":
                    EvenTimeAllocate(M);
                    break;
                case "Apathetic":
                    ApatheticTimeAllocate(M);
                    break;
            }
        }
        private void EagerTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                // Fill the earliest zones first
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterAsc());
                foreach (var Z in P.InclusionZones)
                {
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks <= 0) break;
                    if (P.TimeConsumption.RemainingAsTimeSpan < Z.Duration)
                    {
                        //create cal obj the size of the remaining time
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.Start,
                            End = Z.Start + P.TimeConsumption.RemainingAsTimeSpan,
                            ParentMap = M,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        };
                        P.CalTaskObjs.Add(CalObj);
                        P.TimeConsumption.Remaining = 0;
                    }
                    else
                    {
                        //create cal obj the size of the zone
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.Start,
                            End = Z.End,
                            ParentMap = M,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        };
                        P.CalTaskObjs.Add(CalObj);
                        P.TimeConsumption.Remaining -= Z.Duration.Ticks;
                    }
                }
            }
        }
        private void EvenTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                //Fill zones evenly
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterAsc());
                //First loop that creates CalendarObjects
                foreach (var Z in P.InclusionZones)
                {
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks < TimeTask.MinimumDuration.Ticks) continue;
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.Start + TimeTask.MinimumDuration,
                        ParentMap = M,
                        ParentPerZone = P,
                        ParentInclusionZone = Z,
                    };
                    Z.SeedTaskObj = CalObj;
                    P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                    P.CalTaskObjs.Add(CalObj);
                }
                //Second loop that adds more time to CalendarObjects
                bool full = false;
                while (!full && (P.TimeConsumption.Remaining > 0))
                {
                    full = true;
                    //add a small amount of time to each CalObj until they are full or out of allocated time
                    foreach (var Z in P.InclusionZones)
                    {
                        if (P.TimeConsumption.Remaining <= 0) break;
                        if (Z.SeedTaskObj == null) continue;
                        if (Z.SeedTaskObj.End >= Z.End) continue;
                        Z.SeedTaskObj.End += TimeTask.MinimumDuration;
                        P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                        full = false;
                    }
                }
            }
        }
        private void ApatheticTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                // Fill the latest zones first
                if (P.InclusionZones.Count == 0) return;
                P.InclusionZones.Sort(new InclusionSorterDesc());
                foreach (var Z in P.InclusionZones)
                {
                    //Does this zone have a CalObj already?
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks <= 0) break;
                    if (P.TimeConsumption.RemainingAsTimeSpan < Z.Duration)
                    {
                        //create cal obj the size of the remaining time
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.End - P.TimeConsumption.RemainingAsTimeSpan,
                            End = Z.End,
                            ParentMap = M,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        };
                        P.CalTaskObjs.Add(CalObj);
                        P.TimeConsumption.Remaining = 0;
                    }
                    else
                    {
                        //create cal obj the size of the zone
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.Start,
                            End = Z.End,
                            ParentMap = M,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        };
                        P.CalTaskObjs.Add(CalObj);
                        P.TimeConsumption.Remaining -= Z.Duration.Ticks;
                    }
                }
            }
        }
        #endregion AllocateTimeFromFilters
        #region CalculateCollisions
        private void CalculateCollisions()
        {
            bool hasCollisions = true;
            while (hasCollisions)
            {
                var change = CalculateCollisionsPart2();
                if (change != null)
                {
                    //Deal with the change
                    switch (change.Item2)
                    {
                        case ModMode.Add:
                            //Add the CalObj to the collection
                            change.Item1.ParentPerZone.CalTaskObjs.Add(change.Item1);
                            break;
                        case ModMode.Delete:
                            //Delete the CalObj from the collection
                            change.Item1.ParentPerZone.CalTaskObjs.Remove(change.Item1);
                            break;
                    }
                    hasCollisions = true;
                }
                else
                {
                    hasCollisions = false;
                }
            }
        }
        private enum ModMode { Add, Delete }
        private Tuple<CalendarTaskObject, ModMode> CalculateCollisionsPart2()
        {
            //returns a collection of 
            //group calobjs by dimension
            var calObjDimensions =
                from M in TaskMaps
                from P in M.PerZones
                from C in P.CalTaskObjs
                orderby C.ParentMap.TimeTask.Priority, C.Start, C.End
                group C by C.ParentMap.TimeTask.Dimension;
            foreach (var dimension in calObjDimensions)
            {
                //Clear tangents
                foreach (var C1 in dimension)
                {
                    C1.LeftTangent = null;
                    C1.RightTangent = null;
                }
                //Find collisions
                bool hasChanges = true;
                while (hasChanges)
                {
                    //set to true when a CO is changed
                    hasChanges = false;
                    foreach (var C1 in dimension)
                    {
                        //get any intersecting CalObjs
                        var intersections =
                            from C2 in dimension
                            where C2 != C1
                            where C1.Intersects(C2)
                            orderby C2.ParentMap.TimeTask.Priority, C2.Start, C2.End
                            select C2;
                        foreach (var C2 in intersections)
                        {
                            if (C1.IsInside(C2))
                            {
                                var change = CalculateInsideCollision(C1, C2);
                                if (change != null) return change;
                            }
                            else if (C2.IsInside(C1))
                            {
                                var change = CalculateInsideCollision(C2, C1);
                                if (change != null) return change;
                            }
                            else if (C1.Start <= C2.Start && CalculateIntersectionCollision(C1, C2))
                            {
                                hasChanges = true;
                            }
                            else if (CalculateIntersectionCollision(C2, C1))
                            {
                                hasChanges = true;
                            }
                        }
                    }
                }
            }
            return null;
        }
        private Tuple<CalendarTaskObject, ModMode> CalculateInsideCollision(CalendarTaskObject insider, CalendarTaskObject outsider)
        {
            //Refer to Plans.xlsx - Collisions
            if (insider.StartLock || insider.EndLock ||
                insider.ParentMap.TimeTask.Priority > outsider.ParentMap.TimeTask.Priority)
            {
                //split outsider
                DateTime MDT = insider.Start + new TimeSpan(insider.Duration.Ticks / 2);
                var RC = new CalendarTaskObject();
                RC.Mimic(outsider);
                RC.End = MDT;
                outsider.Start = MDT;
                return new Tuple<CalendarTaskObject, ModMode>(RC, ModMode.Add);
            }
            else
            {
                //delete insider
                return new Tuple<CalendarTaskObject, ModMode>(insider, ModMode.Delete);
            }
        }
        private bool CalculateIntersectionCollision(CalendarTaskObject C1, CalendarTaskObject C2)
        {
            //returns false when this collision is ignored. Happens when there's a conflict.
            //Refer to Plans.xlsx - Collisions
            //Left Right Intersection Collisions C1 ∩ C2
            var LData = GetLeftPushData(C1);
            var RData = GetRightPushData(C2);
            //Refer to Plans.xlsx - Collisions2
            if (C2.StartLock) //H
            {
                if (C1.EndLock) //5
                {
                    //conflict
                    return false;
                }
                else if (LData.HasRoom) //2
                {
                    //C1 Push L
                    Push(C1, C2, LData);
                }
                else //3 4
                {
                    if (LData.Priority < C1.ParentMap.TimeTask.Priority) //3
                    {
                        //C1 Push L
                        Push(C1, C2, LData);
                    }
                    else //4
                    {
                        //C1 Shrink L
                        Shrink(C1, C2, LData);
                    }
                }
            }
            else if (RData.HasRoom) //B C
            {
                if (LData.Priority >= RData.Priority) //B 2 3 4 5
                {
                    //C2 Push R
                    Push(C1, C2, RData);
                }
                else //C
                {
                    if (LData.HasRoom) //2
                    {
                        //C1 Push L
                        Push(C1, C2, LData);
                    }
                    else //3 4 5
                    {
                        //C2 Push R
                        Push(C1, C2, RData);
                    }
                }
            }
            else //D E F G
            {
                if (LData.Priority >= RData.Priority) //D E
                {
                    if (C2.ParentMap.TimeTask.Priority > RData.Priority) //D
                    {
                        if (LData.HasRoom) //2
                        {
                            //C1 Push L
                            Push(C1, C2, LData);
                        }
                        else //3 4 5
                        {
                            //C2 Push R
                            Push(C1, C2, RData);
                        }
                    }
                    else //E
                    {
                        if (LData.HasRoom) //2
                        {
                            //C1 Push L
                            Push(C1, C2, LData);
                        }
                        else //3 4 5
                        {
                            //C2 Shrink R
                            Shrink(C1, C1, RData);
                        }
                    }
                }
                else //F G
                {
                    if (C2.ParentMap.TimeTask.Priority > RData.Priority) //F
                    {
                        if (C1.EndLock) //5
                        {
                            //C2 Push R
                            Push(C1, C2, RData);
                        }
                        if (LData.HasRoom) //2
                        {
                            //C1 Push L
                            Push(C1, C2, LData);
                        }
                        else //3 4
                        {
                            if (LData.Priority < C1.ParentMap.TimeTask.Priority) //3
                            {
                                //C1 Push L
                                Push(C1, C2, LData);
                            }
                            else //4
                            {
                                //C1 Shrink L
                                Shrink(C1, C2, LData);
                            }
                        }
                    }
                    else //G
                    {
                        if (C1.EndLock) //5
                        {
                            //C2 Shrink R
                            Shrink(C1, C1, RData);
                        }
                        if (LData.HasRoom) //2
                        {
                            //C1 Push L
                            Push(C1, C2, LData);
                        }
                        else //3 4
                        {
                            if (LData.Priority < C1.ParentMap.TimeTask.Priority) //3
                            {
                                //C1 Push L
                                Push(C1, C2, LData);
                            }
                            else //4
                            {
                                //C1 Shrink L
                                Shrink(C1, C2, LData);
                            }
                        }
                    }
                }
            }
            return true;
        }
        private struct PushData
        {
            public enum PushDirection { Left, Right }
            public PushDirection Direction;
            public bool HasRoom;
            public double Priority;
            public TimeSpan MaxRoom;
        }
        private PushData GetLeftPushData(CalendarTaskObject C)
        {
            bool C1HasRoom = true;
            var LT = C;
            double LP = C.ParentMap.TimeTask.Priority;
            TimeSpan LmaxRoom = new TimeSpan();
            while (true)
            {
                //If C1.HasRoom == F, LC is the nearest LT where LT.S🔒 or LT.Z.S >=LT.S or LT.LT.E🔒. 
                //LP = C.P where C is the LT with the lowest priority from C1 to LC.
                if (LT.ParentMap.TimeTask.Priority < LP)
                {
                    LP = LT.ParentMap.TimeTask.Priority;
                    LmaxRoom = LT.ParentMap.TimeTask.Duration;
                }
                //LT.HasRoom = F when LT.E🔒 or LT.S🔒 or LT.Z.S >= LT.S or LT.LT.HasRoom=F
                if (LT.EndLock ||
                    LT.StartLock ||
                    LT.ParentInclusionZone?.Start >= LT.Start)
                {
                    C1HasRoom = false;
                    break;
                }
                if (LT.LeftTangent == null)
                {
                    //If C1.HasRoom == T, LC is the LT where LT.LT == null. LP = LC.P.
                    LP = LT.ParentMap.TimeTask.Priority;
                    LmaxRoom = LT.Start - LT.ParentInclusionZone?.Start ?? TimeSpan.FromDays(1);
                    break;
                }
                else
                {
                    LT = LT.LeftTangent;
                }
            }
            return new PushData
            {
                Direction = PushData.PushDirection.Left,
                HasRoom = C1HasRoom,
                Priority = LP,
                MaxRoom = LmaxRoom,
            };
        }
        private PushData GetRightPushData(CalendarTaskObject C)
        {
            bool C2HasRoom = true;
            var RT = C;
            double RP = C.ParentMap.TimeTask.Priority;
            TimeSpan RmaxRoom = new TimeSpan();
            while (true)
            {
                if (RT.ParentMap.TimeTask.Priority < RP)
                {
                    RP = RT.ParentMap.TimeTask.Priority;
                    RmaxRoom = RT.ParentMap.TimeTask.Duration;
                }
                if (RT.EndLock ||
                    RT.StartLock ||
                    RT.ParentInclusionZone?.End <= RT.End)
                {
                    C2HasRoom = false;
                    break;
                }
                if (RT.RightTangent == null)
                {
                    RP = RT.ParentMap.TimeTask.Priority;
                    RmaxRoom = RT.ParentInclusionZone?.End - RT.End ?? TimeSpan.FromDays(1);
                    break;
                }
                else
                {
                    RT = RT.RightTangent;
                }
            }
            return new PushData
            {
                Direction = PushData.PushDirection.Right,
                HasRoom = C2HasRoom,
                Priority = RP,
                MaxRoom = RmaxRoom,
            };
        }
        private void Push(CalendarTaskObject C1, CalendarTaskObject C2, PushData data)
        {
            TimeSpan overlap = C1.End - C2.Start;
            //Assuming that C1 is on left C2 is on right, either Push C1 left or C2 right
            if (data.Direction == PushData.PushDirection.Left)
            {
                //C1 Push L
                TimeSpan room = C1.Start - C1.ParentInclusionZone.Start;
                TimeSpan push = new TimeSpan(Min(overlap.Ticks, Min(data.MaxRoom.Ticks, room.Ticks)));
                C1.Start -= push;
                C1.End -= push;
            }
            else
            {
                //C2 Push R
                TimeSpan room = C2.ParentInclusionZone.End - C2.End;
                TimeSpan push = new TimeSpan(Min(overlap.Ticks, Min(data.MaxRoom.Ticks, room.Ticks)));
                C2.Start += push;
                C2.End += push;
            }
            C1.RightTangent = C2;
            C2.LeftTangent = C1;
        }
        private void Shrink(CalendarTaskObject C1, CalendarTaskObject C2, PushData data)
        {
            TimeSpan overlap = C1.End - C2.Start;
            //Assuming that C1 is on left C2 is on right, either Shrink C1 left or C2 right
            if (data.Direction == PushData.PushDirection.Left)
            {
                //C1 Shrink L
                TimeSpan push = new TimeSpan(Min(overlap.Ticks, C1.Duration.Ticks));
                C1.End -= push;
            }
            else
            {
                //C2 Shrink R
                TimeSpan push = new TimeSpan(Min(overlap.Ticks, C2.Duration.Ticks));
                C2.Start += push;
            }
            C1.RightTangent = C2;
            C2.LeftTangent = C1;
        }
        #endregion Collisions
        #region AllocateEmptySpace
        private void AllocateEmptySpace()
        {
            //Recalculate remaining allocations. Identify empty spaces. For each empty space, 
            //check if there is any relevant task that can be allocated by priority of: 
            //Highest priority intersecting insufficient InclusionZone, 
            //highest priority intersecting InclusionZone that CanFill.
            CalculateTimeConsumptions();
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                var spaceDimensions = GetEmptySpaces();
                foreach (var dimension in spaceDimensions)
                {
                    foreach (var space in dimension)
                    {
                        //find any intersecting insufficient InclusionZones
                        var zones =
                            from M in TaskMaps
                            where M.TimeTask.Dimension == dimension.Key
                            from P in M.PerZones
                            from Z in P.InclusionZones
                            where Z.Intersects(space)
                            select Z;
                        var insuffZones =
                            from Z in zones
                            where Z.ParentPerZone.TimeConsumption.Remaining > 0
                            orderby Z.ParentPerZone.ParentMap.TimeTask.Priority
                            select Z;
                        var insuffZone = insuffZones.FirstOrDefault();
                        if (insuffZone != null)
                        {
                            AllocateEmptySpacePart2(space, insuffZone);
                            hasChanges = true;
                        }
                        else
                        {
                            //no insufficient zone found
                            //look for CanFill zones
                            var fillZones =
                                from Z in zones
                                where Z.ParentPerZone.ParentMap.TimeTask.CanFill
                                orderby Z.ParentPerZone.ParentMap.TimeTask.Priority
                                select Z;
                            var fillZone = fillZones.FirstOrDefault();
                            if (fillZone != null)
                            {
                                AllocateEmptySpacePart2(space, fillZone);
                                hasChanges = true;
                            }
                        }
                    }
                }
            }
        }
        private static void AllocateEmptySpacePart2(EmptyZone space, InclusionZone inZone)
        {
            if (inZone.Start <= space.Start &&
                space.Left?.ParentInclusionZone == inZone &&
                !space.Left.EndLock)
            {
                var newEnd = new DateTime(Min(inZone.End.Ticks, Min(space.End.Ticks, 
                    space.End.Ticks + (long)space.Left.ParentPerZone.TimeConsumption.Remaining)));
                var diff = newEnd - space.Left.End;
                space.Left.End = newEnd;
                space.Left.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            if (inZone.End >= space.End &&
                space.Right?.ParentInclusionZone == inZone &&
                !space.Right.EndLock)
            {
                var newStart = new DateTime(Max(inZone.Start.Ticks, Max(space.Start.Ticks,
                    space.Start.Ticks - (long)space.Left.ParentPerZone.TimeConsumption.Remaining)));
                var diff = space.Right.Start - newStart;
                space.Right.Start = newStart;
                space.Right.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            {
                var start = new DateTime(Max(inZone.Start.Ticks, space.Start.Ticks));
                var end = new DateTime(Min(inZone.End.Ticks, Min(space.End.Ticks,
                    start.Ticks + (long)inZone.ParentPerZone.TimeConsumption.Remaining)));
                var C = new CalendarTaskObject
                {
                    Start = start,
                    End = end,
                    ParentMap = inZone.ParentPerZone.ParentMap,
                    ParentPerZone = inZone.ParentPerZone,
                    ParentInclusionZone = inZone,
                };
                inZone.ParentPerZone.CalTaskObjs.Add(C);
            }
        }
        private void CalculateTimeConsumptions()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    TimeSpan spent = new TimeSpan(0);
                    foreach (var C in P.CalTaskObjs)
                        spent += C.Duration;
                    P.TimeConsumption.Remaining = P.TimeConsumption.Allocation.Amount - spent.Ticks;
                }
            }
        }
        private List<Grouping<int, EmptyZone>> GetEmptySpaces()
        {
            //group COs by dimension
            var calObjDimensions =
                from M in TaskMaps
                from P in M.PerZones
                from C in P.CalTaskObjs
                orderby C.Start
                group C by C.ParentMap.TimeTask.Dimension;
            var spaceDimensions = new List<Grouping<int, EmptyZone>>();
            foreach (var dimension in calObjDimensions)
            {
                var spaces = new Grouping<int, EmptyZone>();
                spaces.Key = dimension.Key;
                //Find earliest Per
                var start =
                    (from M in TaskMaps
                     where M.TimeTask.Dimension == dimension.Key
                     from P in M.PerZones
                     select P).Min(P =>
                     P.Start);
                DateTime dt = start;
                CalendarTaskObject prev = null;
                foreach (var C in dimension)
                {
                    if (dt < C.Start)
                    {
                        //found an empty space
                        var Z = new EmptyZone
                        {
                            Start = dt,
                            End = C.Start,
                            Left = prev,
                            Right = C,
                        };
                        spaces.Add(Z);
                    }
                    dt = C.End;
                }
                spaceDimensions.Add(spaces);
            }
            return spaceDimensions;
        }
        #endregion AllocateEmptySpace
        #region CleanUpStates
        private void CleanUpStates()
        {
            FixMisalignments();
            FixWrongStates();
            FlagInsufficients();
            FlagConflicts();
        }
        private void FixMisalignments()
        {
            //If there are any CalObjs that cross a zone boundary by more than MinDur, split it. 
            var misalignments =
                from M in TaskMaps
                from P in M.PerZones
                from Z in P.InclusionZones
                from C in P.CalTaskObjs
                where C.ParentPerZone == Z.ParentPerZone
                where C.Intersects(Z) && !C.IsInside(Z)
                select C;
            foreach (var C in misalignments)
            {
                if (C.Start < C.ParentInclusionZone.Start)
                {
                    var split = new CalendarTaskObject();
                    split.Mimic(C);
                    split.End = C.ParentInclusionZone.Start;
                    split.State = CalendarTaskObject.States.Unscheduled;
                    if (split.Duration.Ticks >= 0)
                    {
                        C.Start = split.End;
                        split.ParentPerZone.CalTaskObjs.Add(split);
                    }
                }
                if (C.End > C.ParentInclusionZone.End)
                {
                    var split = new CalendarTaskObject();
                    split.Mimic(C);
                    split.Start = C.ParentInclusionZone.End;
                    split.State = CalendarTaskObject.States.Unscheduled;
                    if (split.Duration.Ticks >= 0)
                    {
                        C.End = split.Start;
                        split.ParentPerZone.CalTaskObjs.Add(split);
                    }
                }
            }
        }
        private void FixWrongStates()
        {
            //If a CalObj marked Unscheduled is within a proper InclusionZone, mark it correctly. 
            //If any CalObj not marked Unscheduled is outside of a proper InclusionZone, mark it Unscheduled.
            var CalObjsOverZones =
                from M in TaskMaps
                from P in M.PerZones
                from Z in P.InclusionZones
                from C in P.CalTaskObjs
                where C.ParentPerZone == Z.ParentPerZone
                where C.Intersects(Z)
                select C;
            var CalObjs =
                from M in TaskMaps
                from P in M.PerZones
                from C in P.CalTaskObjs
                select C;
            var CalObjsOutside = CalObjs.Except(CalObjsOverZones);
            foreach (var C in CalObjsOverZones)
            {
                if (C.State == CalendarTaskObject.States.Unscheduled)
                {
                    if (C.StartLock || C.EndLock)
                    {
                        C.State = CalendarTaskObject.States.Confirmed;
                    }
                    else if (C.ParentMap.TimeTask.AutoCheckIn)
                    {
                        C.State = CalendarTaskObject.States.AutoConfirm;
                    }
                    else
                    {
                        C.State = CalendarTaskObject.States.Unconfirmed;
                    }
                }
            }
            foreach (var C in CalObjsOutside)
            {
                if (C.State != CalendarTaskObject.States.Unscheduled)
                {
                    C.State = CalendarTaskObject.States.Unscheduled;
                }
            }
        }
        private void FlagInsufficients()
        {
            //If a Per has remaining > 0, mark all of its CalObjs as Insufficient. 
            var insuffPers =
                from M in TaskMaps
                from P in M.PerZones
                where P.TimeConsumption.Remaining > 0
                select P;
            foreach (var P in insuffPers)
            {
                foreach (var C in P.CalTaskObjs)
                {
                    C.State = CalendarTaskObject.States.Insufficient;
                }
            }
        }
        private void FlagConflicts()
        {
            //If there are still collisions, mark them as Conflict. 
            var calObjDimensions =
                from M in TaskMaps
                from P in M.PerZones
                from C in P.CalTaskObjs
                group C by C.ParentMap.TimeTask.Dimension;
            foreach (var dimension in calObjDimensions)
            {
                foreach (var C1 in dimension)
                {
                    var intersections =
                        from C2 in dimension
                        where C2 != C1
                        where C1.Intersects(C2)
                        select C2;
                    foreach (var C2 in intersections)
                    {
                        C1.State = CalendarTaskObject.States.Conflict;
                        C2.State = CalendarTaskObject.States.Conflict;
                    }
                }
            }
            //If there are any CalObjs with no time, mark them as cancelled
            var CalObjs =
                from M in TaskMaps
                from P in M.PerZones
                from C in P.CalTaskObjs
                select C;
            foreach (var C in CalObjs)
            {
                if (C.Start == C.End)
                {
                    C.State = CalendarTaskObject.States.Cancel;
                }
            }
        }
        #endregion CleanUpStates
        private void UnZipTaskMaps()
        {
            CalNoteObjsCollection = new CollectionViewSource();
            CalNoteObjsCollection.Source = new ObservableCollection<CalendarNoteObject>();
            CalTaskObjsCollection = new CollectionViewSource();
            CalTaskObjsCollection.Source = new ObservableCollection<CalendarTaskObject>();
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    foreach (var CalObj in P.CalTaskObjs)
                    {
                        CalTaskObjsView.AddNewItem(CalObj);
                        CalTaskObjsView.CommitNew();
                        AdditionalCalObjSetup(CalObj);
                    }
                }
            }
            OnPropertyChanged(nameof(CalTaskObjsView));
            OnPropertyChanged(nameof(CalNoteObjsView));
        }
        protected virtual void AdditionalCalObjSetup(CalendarTaskObject CalObj) { }
        private void AddNewCheckIn(DateTime dt, bool start, TimeTask task)
        {
            //TODO
            //add to the correct map, no need to call BuildCheckIns()
            //add to the database collection
            //rebuild CalObjs
        }
        private void EditCheckIn(CalendarCheckIn CI)
        {
            //TODO
            //find and edit the CI from TaskMaps, no need to call BuildCheckIns()
            //find and edit the CI in the database collection
            //rebuild CalObjs
        }
        private void DeleteCheckIn(CalendarCheckIn CI)
        {
            //TODO
            //find and delete the CI from TaskMaps, no need to call BuildCheckIns()
            //find and delete the CI from the database collection
            //rebuild CalObjs
        }
        protected virtual async Task PreviousAsync()
        {
            IsLoading = true;
            await CreateTaskObjects();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual async Task NextAsync()
        {
            IsLoading = true;
            await CreateTaskObjects();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual void ToggleOrientation()
        {
            if (Orientation == Orientation.Horizontal)
                Orientation = Orientation.Vertical;
            else
                Orientation = Orientation.Horizontal;
        }
        protected virtual void ScaleUp() { ScaleSudoCommand = 1; }
        protected virtual void ScaleDown() { ScaleSudoCommand = -1; }
        private void NewNote()
        {
            NotesVM.NewItemCommand.Execute(null);
        }
        #endregion Actions
    }
}
