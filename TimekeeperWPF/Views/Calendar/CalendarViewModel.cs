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
            await SetUpCalendarObjects();
            await base.GetDataAsync();
        }
        protected virtual async Task SetUpCalendarObjects()
        {
            Status = "Creating CalendarObjects...";
            CreateNoteObjects();
            await CreateTaskObjects();
        }
        private void CreateNoteObjects()
        {
            CalNoteObjsCollection = new CollectionViewSource();
            CalNoteObjsCollection.Source = new ObservableCollection<CalendarNoteObject>();
            NotesVM.View.Filter = N => Intersects((Note)N);
            foreach (Note N in NotesVM.View)
            {
                CalendarNoteObject CalObj = new CalendarNoteObject();
                CalObj.DateTime = N.DateTime;
                CalObj.ToolTip = N;
                CalNoteObjsView.AddNewItem(CalObj);
                CalNoteObjsView.CommitNew();
            }
            OnPropertyChanged(nameof(CalNoteObjsView));
        }
        private async Task CreateTaskObjects()
        {
            View.Filter = T => ((TimeTask)T).Intersects(SelectedDate, EndDate);
            foreach (TimeTask T in View)
            {
                await T.BuildPerZonesAsync(SelectedDate, EndDate);
                await T.BuildInclusionZonesAsync();
            }
            BuildTaskMaps();
            BuildAllocations();
            //DetermineTaskStates();
            //CalculateCollisions
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
                        CalTaskObjs = new List<CalendarTaskObject>(),
                        CalNoteObjs = new List<CalendarNoteObject>(),
                    };
                    M.InclusionZones.Add(zone);
                }
                TaskMaps.Add(M);
            }
        }
        private void BuildAllocations()
        {
            //Time Allocations
            foreach (var M in TaskMaps)
            {
                if (M.InclusionZones.Count == 0) continue;
                if (M.TimeTask.TimeAllocation == null) AllocateAllTime(M);
                else if (M.TimeTask.TimeAllocation.Per == null) AllocateTimeResource(M);
                else AllocateTimePerTime(M);
            }
        }
        private void AllocateAllTime(CalendarTimeTaskMap M)
        {
            // If no time allocation is set, we create one CalendarObject per inclusion zone
            // with each Start/End set to the bounds of the inclusion zone.
            foreach (var Z in M.InclusionZones)
            {
                //create cal object that matches zone
                CalendarTaskObject CalObj = new CalendarTaskObject
                {
                    Start = Z.Start,
                    End = Z.End,
                    ParentMap = M
                };
                Z.CalTaskObjs.Add(CalObj);
            }
            FinalizeCalTaskObjs(M);
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
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.Start + A.RemainingAsTimeSpan,
                        ParentMap = M
                    };
                    Z.CalTaskObjs.Add(CalObj);
                    A.Remaining = 0;
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
                    Z.CalTaskObjs.Add(CalObj);
                    A.Remaining -= Z.Duration.Ticks;
                }
            }
            FinalizeCalTaskObjs(M);
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
                CalendarTaskObject CalObj = new CalendarTaskObject
                {
                    Start = Z.Start,
                    End = Z.Start + TimeTask.MinimumDuration,
                    ParentMap = M
                };
                A.Remaining -= TimeTask.MinimumDuration.Ticks;
                Z.CalTaskObjs.Add(CalObj);
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
                    foreach (var CalObj in Z.CalTaskObjs)
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
            FinalizeCalTaskObjs(M);
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
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.End - A.RemainingAsTimeSpan,
                        End = Z.End,
                        ParentMap = M
                    };
                    Z.CalTaskObjs.Add(CalObj);
                    A.Remaining = 0;
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
                    Z.CalTaskObjs.Add(CalObj);
                    A.Remaining -= Z.Duration.Ticks;
                }
            }
            FinalizeCalTaskObjs(M);
        }
        private void FinalizeCalTaskObjs(CalendarTimeTaskMap M)
        {
            CalTaskObjsCollection = new CollectionViewSource();
            CalTaskObjsCollection.Source = new ObservableCollection<CalendarTaskObject>();
            foreach (var Z in M.InclusionZones)
            {
                foreach (var CalObj in Z.CalTaskObjs)
                {
                    CalObj.Start = CalObj.Start.RoundUp(TimeTask.MinimumDuration);
                    CalObj.End = CalObj.End.RoundUp(TimeTask.MinimumDuration);
                    CalTaskObjsView.AddNewItem(CalObj);
                    CalTaskObjsView.CommitNew();
                    AdditionalCalObjSetup(CalObj);
                }
            }
            OnPropertyChanged(nameof(CalTaskObjsView));
        }
        protected virtual void AdditionalCalObjSetup(CalendarTaskObject CalObj) { }
        private void DetermineTaskStates()
        {
            //foreach InclusionZone, find all Intersecting Notes that are not of type "Note" and have matching dimension
            //then alter the CalObj under the note
            foreach (var M in TaskMaps)
            {
                foreach (var Z in M.InclusionZones)
                {
                    var notes = from N in (NotesVM.Source)
                                where Z.Intersects(N)
                                where (N.TimeTask == null && N.Dimension == M.TimeTask.Dimension && N.TaskType == M.TimeTask.TaskType)
                                    || N.TimeTask == M.TimeTask
                                select N;
                }
            }
        }
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
        private void NewNote()
        {
            NotesVM.NewItemCommand.Execute(null);
        }
        #endregion Actions
    }
}