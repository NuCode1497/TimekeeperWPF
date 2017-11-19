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

namespace TimekeeperWPF
{
    public abstract class CalendarViewModel : TypedLabeledEntitiesViewModel<TimeTask>
    {
        #region Fields
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
        #endregion Fields
        #region Events
        public event RequestViewChangeEventHandler RequestViewChange;
        protected virtual void OnRequestViewChange(RequestViewChangeEventArgs e)
        { RequestViewChange?.Invoke(this, e); }
        #endregion Events
        #region Properties
        public List<CalendarTimeTaskMap> TaskMaps;
        public CollectionViewSource CalendarObjectsCollection { get; set; }
        public ObservableCollection<UIElement> CalendarObjectsSource => CalendarObjectsCollection?.Source as ObservableCollection<UIElement>;
        public ListCollectionView CalendarObjectsView => CalendarObjectsCollection?.View as ListCollectionView;
        public UIElement SelectedCalendarObject
        {
            get { return _SelectedCalendarObect; }
            set
            {
                if (_SelectedCalendarObect == value) return;
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
        //TODO
        protected override bool CanAddNew => false;
        protected override bool CanEditSelected => false;
        protected override bool CanSave => false;
        protected override bool CanDeleteSelected => false;

        protected bool IsNoteRelevant(Note note)
        {
            return note.DateTime >= SelectedDate && note.DateTime <= EndDate;
        }
        public bool Intersects(DateTime start, DateTime end)
        {
            return start < EndDate && SelectedDate < end;
        }
        public bool Intersects(InclusionZone Z)
        {
            return Intersects(Z.Start, Z.End);
        }
        public bool Intersects(TimeTask T)
        {
            return Intersects(T.Start, T.End);
        }
        public bool Intersects(CalendarObject C)
        {
            return Intersects(C.Start, C.End);
        }
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
            Status = "Creating CalendarObjects...";
            await SetUpCalendarObjects();
            //calculate collisions and reorganize CalendarObjects by changing their datetimes
            await base.GetDataAsync();
        }
        protected virtual async Task SetUpCalendarObjects()
        {
            CalendarObjectsCollection = new CollectionViewSource();
            CalendarObjectsCollection.Source = new ObservableCollection<UIElement>();
            //CreateNoteObjects();
            await CreateEventObjectsFromTimeTasks();
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
        //TODO update by adding CRUD stuff for notes
        private void CreateNoteObjects()
        {
            View.Filter = N =>
            {
                Note note = (Note)N;
                return IsNoteRelevant(note)
                    && note.TaskType.Name == "Note";
            };
            foreach (Note N in View)
            {
                CalendarObject CalObj = new CalendarObject();
                CalObj.Start = N.DateTime;
                CalObj.End = N.DateTime.AddMinutes(1);
                CalObj.ToolTip = N;
                CalendarObjectsView.AddNewItem(CalObj);
                CalendarObjectsView.CommitNew();
            }
            View.Filter = null;
        }
        private async Task CreateEventObjectsFromTimeTasks()
        {
            View.Filter = T => ((TimeTask)T).Intersects(SelectedDate, EndDate);
            foreach (TimeTask T in View)
            {
                await T.BuildPerZonesAsync(SelectedDate, EndDate);
                await T.BuildInclusionZonesAsync();
            }
            BuildTaskMaps();
            //Deal with time allocations
            foreach (var M in TaskMaps)
            {
                if (M.InclusionZones.Count == 0) continue;
                if (M.TimeTask.TimeAllocation == null) AllocateAllTime(M);
                else if (M.TimeTask.TimeAllocation.Per == null) AllocateTimeResource(M);
                else AllocateTimePerTime(M);
            }
            //TODO Deal with other types of allocations like dollars per hour, gas per dollar, etc.
        }
        private void BuildTaskMaps()
        {
            //create mappings for each CalendarObject in each Inclusion Zone of each TimeTask
            TaskMaps = new List<CalendarTimeTaskMap>();
            foreach (TimeTask T in View)
            {
                var M = new CalendarTimeTaskMap
                {
                    TimeTask = T,
                    InclusionZones = new List<InclusionZone>()
                };
                foreach (var Z in T.InclusionZones)
                {
                    var zone = new InclusionZone
                    {
                        Start = Z.Key,
                        End = Z.Value,
                        CalendarObjects = new List<CalendarObject>()
                    };
                    M.InclusionZones.Add(zone);
                }
                TaskMaps.Add(M);
            }
        }
        private void AllocateAllTime(CalendarTimeTaskMap M)
        {
            // If no time allocation is set, we create one CalendarObject per inclusion zone
            // with each Start/End set to the bounds of the inclusion zone.
            foreach (var Z in M.InclusionZones)
            {
                //create cal object that matches zone
                CalendarObject CalObj = new CalendarObject
                {
                    Start = Z.Start,
                    End = Z.End,
                    ParentMap = M
                };
                Z.CalendarObjects.Add(CalObj);
            }
            FinalizeCalObjs(M);
        }
        private void AllocateTimeResource(CalendarTimeTaskMap M)
        {
            //If no Per is set, allocate time across entire task
            M.TimeTask.TimeAllocation.Remaining = M.TimeTask.TimeAllocation.AmountAsTimeSpan().Ticks;
            AllocationMethod(M, M.TimeTask.TimeAllocation);
        }
        private void AllocateTimePerTime(CalendarTimeTaskMap M)
        {
            // Allocation.Amount is the amount of time that will be allocated for each Allocation.Per
            // segment aligned with the calendar. 

            TimeTaskAllocation A = M.TimeTask.TimeAllocation;
            var allocatedTime = A.AmountAsTimeSpan().Ticks;
            //get list of relevant per zones
            var perZones = M.TimeTask.PerZones.Where(P => Intersects(P.Key, P.Value));
            foreach (var P in perZones)
            {
                A.Remaining = allocatedTime;
                //get subset of zones intersecting current per
                CalendarTimeTaskMap perMap = new CalendarTimeTaskMap()
                {
                    TimeTask = M.TimeTask,
                    InclusionZones = M.InclusionZones.Where(Z => Z.Intersects(P.Key, P.Value)).ToList()
                };
                AllocationMethod(perMap, A);
            }
        }
        private void AllocationMethod(CalendarTimeTaskMap M, TimeTaskAllocation A)
        {
            switch (M.TimeTask.AllocationMethod)
            {
                case "Eager":
                    EagerAllocate(M, A);
                    break;
                case "Even":
                    EvenAllocate(M, A);
                    break;
                case "Apathetic":
                    ApatheticAllocate(M, A);
                    break;
            }
        }
        private void EagerAllocate(CalendarTimeTaskMap M, TimeTaskAllocation A)
        {
            // Fill the earliest zones first
            if (M.InclusionZones.Count == 0) return;
            M.InclusionZones.Sort(new InclusionSorterAsc());
            foreach (var Z in M.InclusionZones)
            {
                if (A.Remaining <= 0) break;
                if (Z.Duration.Ticks <= 0) break;
                if (A.RemainingAsTimeSpan < Z.Duration)
                {
                    //create cal obj the size of the remaining time
                    CalendarObject CalObj = new CalendarObject
                    {
                        Start = Z.Start,
                        End = Z.Start + A.RemainingAsTimeSpan,
                        ParentMap = M
                    };
                    Z.CalendarObjects.Add(CalObj);
                    A.Remaining = 0;
                }
                else
                {
                    //create cal obj the size of the zone
                    CalendarObject CalObj = new CalendarObject
                    {
                        Start = Z.Start,
                        End = Z.End,
                        ParentMap = M
                    };
                    Z.CalendarObjects.Add(CalObj);
                    A.Remaining -= Z.Duration.Ticks;
                }
            }
            FinalizeCalObjs(M);
        }
        private void EvenAllocate(CalendarTimeTaskMap M, TimeTaskAllocation A)
        {
            //Fill zones evenly
            if (M.InclusionZones.Count == 0) return;
            M.InclusionZones.Sort(new InclusionSorterAsc());
            //First loop that creates CalendarObjects
            foreach (var Z in M.InclusionZones)
            {
                if (A.Remaining <= 0) break;
                if (Z.Duration.Ticks <= 0) break;
                CalendarObject CalObj = new CalendarObject
                {
                    Start = Z.Start,
                    End = Z.Start + TimeTask.MinimumDuration,
                    ParentMap = M
                };
                A.Remaining -= TimeTask.MinimumDuration.Ticks;
                Z.CalendarObjects.Add(CalObj);
            }
            //Second loop that adds more time to CalendarObjects
            bool full = false;
            while (!full && (A.Remaining >= TimeTask.MinimumDuration.Ticks))
            {
                full = true;
                //add a small amount of time to each CalObj until they are full or out of allocated time
                foreach (var Z in M.InclusionZones)
                {
                    if (Z.Duration.Ticks <= 0) break;
                    foreach (var CalObj in Z.CalendarObjects)
                    {
                        if (A.Remaining <= 0) break;
                        //If zone is full
                        if (Z.Duration.Ticks - CalObj.Duration.Ticks <= 0) break;
                        CalObj.End += TimeTask.MinimumDuration;
                        A.Remaining -= TimeTask.MinimumDuration.Ticks;
                        full = false;
                    }
                    if (A.Remaining <= 0) break;
                }
            }
            FinalizeCalObjs(M);
        }
        private void ApatheticAllocate(CalendarTimeTaskMap M, TimeTaskAllocation A)
        {
            // Fill the latest zones first
            if (M.InclusionZones.Count == 0) return;
            M.InclusionZones.Sort(new InclusionSorterDesc());
            foreach (var Z in M.InclusionZones)
            {
                if (A.Remaining <= 0) break;
                if (Z.Duration.Ticks <= 0) break;
                if (A.RemainingAsTimeSpan < Z.Duration)
                {
                    //create cal obj the size of the remaining time
                    CalendarObject CalObj = new CalendarObject
                    {
                        Start = Z.End - A.RemainingAsTimeSpan,
                        End = Z.End,
                        ParentMap = M
                    };
                    Z.CalendarObjects.Add(CalObj);
                    A.Remaining = 0;
                }
                else
                {
                    //create cal obj the size of the zone
                    CalendarObject CalObj = new CalendarObject
                    {
                        Start = Z.Start,
                        End = Z.End,
                        ParentMap = M
                    };
                    Z.CalendarObjects.Add(CalObj);
                    A.Remaining -= Z.Duration.Ticks;
                }
            }
            FinalizeCalObjs(M);
        }
        private void FinalizeCalObjs(CalendarTimeTaskMap M)
        {
            //Third loop that finalizes CalendarObjects
            foreach (var Z in M.InclusionZones)
            {
                foreach (var CalObj in Z.CalendarObjects)
                {
                    CalObj.Start = CalObj.Start.RoundUp(TimeTask.MinimumDuration);
                    CalObj.End = CalObj.End.RoundUp(TimeTask.MinimumDuration);
                    CalendarObjectsView.AddNewItem(CalObj);
                    CalendarObjectsView.CommitNew();
                    AdditionalCalObjSetup(CalObj);
                }
            }
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
        protected virtual void AdditionalCalObjSetup(CalendarObject CalObj) { }
        protected virtual async Task PreviousAsync()
        {
            IsLoading = true;
            await SetUpCalendarObjects();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual async Task NextAsync()
        {
            IsLoading = true;
            await SetUpCalendarObjects();
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
        #endregion Actions
    }
}