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
            await BuildTaskMaps();
            BuildCheckIns();
            //BuildAllocations();
            //UnZipTaskMaps();
        }
        private async Task BuildTaskMaps()
        {
            //This function is more intensive than others, you only need to call it once on load
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
                        // Refer to Plans.xlsx\CheckIns
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
                                                ParentMap = M,
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
                                                ParentMap = M,
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
                                                ParentMap = M,
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
                                                ParentMap = M,
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
                                                ParentMap = M,
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
        private void AllocateTimeFromFilters()
        {
            //
        }
        private void BuildAllocations()
        {
            //Time Allocations
            foreach (var M in TaskMaps)
            {
                if (M.PerZones.Count == 0) continue;
                if (M.TimeTask.TimeAllocation == null) AllocateAllTime(M);
                else AllocationMethod(M);
            }
            //Other Allocations
        }
        private void AllocateAllTime(CalendarTimeTaskMap M)
        {
            // If no time allocation is set, we create one CalendarObject per inclusion zone
            // with each Start/End set to the bounds of the inclusion zone.
            foreach (var P in M.PerZones)
            {
                foreach (var Z in P.InclusionZones)
                {
                    //create cal object that matches zone
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.End,
                        ParentMap = M
                    };
                    CalObj.Start = CalObj.Start.RoundUp(TimeTask.MinimumDuration);
                    CalObj.End = CalObj.End.RoundUp(TimeTask.MinimumDuration);
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
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks <= 0) break;
                    if (P.TimeConsumption.RemainingAsTimeSpan < Z.Duration)
                    {
                        //create cal obj the size of the remaining time
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.Start,
                            End = Z.Start + P.TimeConsumption.RemainingAsTimeSpan,
                            ParentMap = M
                        };
                        Z.SeedTaskObj = CalObj;
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
                            ParentMap = M
                        };
                        Z.SeedTaskObj = CalObj;
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
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks <= 0) break;
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.Start + TimeTask.MinimumDuration,
                        ParentMap = M
                    };
                    P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                    Z.SeedTaskObj = CalObj;
                    P.CalTaskObjs.Add(CalObj);
                }
                //Second loop that adds more time to CalendarObjects
                bool full = false;
                while (!full && (P.TimeConsumption.Remaining >= TimeTask.MinimumDuration.Ticks))
                {
                    full = true;
                    //add a small amount of time to each CalObj until they are full or out of allocated time
                    foreach (var Z in P.InclusionZones)
                    {
                        if (Z.Duration.Ticks <= 0) break;
                        if (P.TimeConsumption.Remaining <= 0) break;
                        //If zone is full
                        if (Z.Duration.Ticks - Z.SeedTaskObj.Duration.Ticks <= 0) break;
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
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks <= 0) break;
                    if (P.TimeConsumption.RemainingAsTimeSpan < Z.Duration)
                    {
                        //create cal obj the size of the remaining time
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.End - P.TimeConsumption.RemainingAsTimeSpan,
                            End = Z.End,
                            ParentMap = M
                        };
                        P.CalTaskObjs.Add(CalObj);
                        Z.SeedTaskObj = CalObj;
                        P.TimeConsumption.Remaining = 0;
                    }
                    else
                    {
                        //create cal obj the size of the zone
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.Start,
                            End = Z.End,
                            ParentMap = M
                        };
                        P.CalTaskObjs.Add(CalObj);
                        Z.SeedTaskObj = CalObj;
                        P.TimeConsumption.Remaining -= Z.Duration.Ticks;
                    }
                }
            }
        }
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
