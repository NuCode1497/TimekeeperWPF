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
        #endregion
        #region Events
        public event RequestViewChangeEventHandler RequestViewChange;
        protected virtual void OnRequestViewChange(RequestViewChangeEventArgs e)
        { RequestViewChange?.Invoke(this, e); }
        #endregion
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
        #endregion
        #region Commands
        public ICommand PreviousCommand => _PreviousCommand
            ?? (_PreviousCommand = new RelayCommand(ap => Previous(), pp => CanPrevious));
        public ICommand NextCommand => _NextCommand
            ?? (_NextCommand = new RelayCommand(ap => Next(), pp => CanNext));
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
        #endregion
        #region Predicates
        protected virtual bool CanPrevious => true;
        protected virtual bool CanNext => true;
        protected virtual bool CanOrientation => true;
        public virtual bool CanMax => true;
        public virtual bool CanTextMargin => true;
        protected virtual bool CanScaleUp => true;
        protected virtual bool CanScaleDown => true;
        protected virtual bool CanSelectWeek => true;
        protected virtual bool CanSelectDay => true;
        protected virtual bool CanSelectYear => true;
        protected virtual bool CanSelectMonth => true;
        //TODO
        protected override bool CanAddNew => false;
        protected override bool CanEditSelected => false;
        protected override bool CanSave => false;
        protected override bool CanDeleteSelected => false;
        #endregion
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
            //get tasks 
            //read task data and create CalendarObjects
            //calculate collisions and reorganize CalendarObjects by changing their datetimes
            Context = new TimeKeeperContext();
            await Context.TimeTasks.LoadAsync();
            Items.Source = Context.TimeTasks.Local;

            SetUpCalendarObjects();

            await base.GetDataAsync();
        }
        protected virtual void SetUpCalendarObjects()
        {
            CalendarObjectsCollection = new CollectionViewSource();
            CalendarObjectsCollection.Source = new ObservableCollection<UIElement>();
            //CreateNoteObjects();
            //CreateEventObjectsFromNotes();
            CreateEventObjectsFromTimeTasks();
            OnPropertyChanged(nameof(CalendarObjectsView));
        }
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
                CalObj.ParentEntity = N;
                CalendarObjectsView.AddNewItem(CalObj);
                CalendarObjectsView.CommitNew();
            }
            View.Filter = null;
        }
        private void CreateEventObjectsFromNotes()
        {
            CalendarObject prevCalObj = null;
            View.Filter = N =>
            {
                Note note = (Note)N;
                return IsNoteRelevant(note)
                    && note.TaskType.Name != "Note";
            };
            View.CustomSort = new NoteDateTimeSorterAsc();
            List<CalendarObject> CalObjs = new List<CalendarObject>();
            foreach (Note N in View)
            {
                CalendarObject CalObj = new CalendarObject();
                CalObj.Start = N.DateTime;
                CalObj.End = N.DateTime.AddHours(1);
                CalObj.ToolTip = N;
                CalObj.ParentEntity = N;
                if (prevCalObj == null)
                {
                    prevCalObj = CalObj;
                }
                else
                {
                    prevCalObj.End = N.DateTime;
                    prevCalObj.ToolTip += String.Format("\n{0}\n{1} to\n{2}",
                    prevCalObj.DurationString(), 
                    prevCalObj.Start,
                    prevCalObj.End);
                    prevCalObj = CalObj;
                }
                CalObjs.Add(CalObj);
            }
            foreach (var CalObj in CalObjs)
            {
                CalendarObjectsView.AddNewItem(CalObj);
                CalendarObjectsView.CommitNew();
                AdditionalCalObjSetup(CalObj);
            }
            View.Filter = null;
        }
        private void CreateEventObjectsFromTimeTasks()
        {
            View.Filter = T => IsTaskRelevant((TimeTask)T);
            BuildTaskMaps();
            foreach (var M in TaskMaps)
            {
                if (M.InclusionZones.Count == 0) continue;
                if (AllocateTime(M)) continue;
                if (AllocateTimeResource(M)) continue;
                if (AllocateTimePerTime(M)) continue;
            }
        }
        private void BuildTaskMaps()
        {
            TaskMaps = new List<CalendarTimeTaskMap>();
            foreach (TimeTask T in View)
            {
                T.BuildInclusionZones();
                var map = new CalendarTimeTaskMap();
                map.TimeTask = T;
                map.InclusionZones = new List<InclusionZone>();
                foreach (var Z in T.InclusionZones)
                {
                    var zone = new InclusionZone();
                    zone.Start = Z.Key;
                    zone.End = Z.Value;
                    zone.CalendarObjects = new List<CalendarObject>();
                }
            }
        }
        private bool AllocateTime(CalendarTimeTaskMap M)
        {
            // If no allocation is set, we create one CalendarObject per inclusion zone
            // with each Start/End set to the bounds of the inclusion zone.
            if (M.TimeTask.Allocations.Count == 0)
            {
                foreach (var Z in M.InclusionZones)
                {
                    //create cal object that matches zone
                    CalendarObject CalObj = new CalendarObject();
                    CalObj.Start = Z.Start;
                    CalObj.End = Z.End;
                    CalObj.ParentEntity = M.TimeTask;
                    Z.CalendarObjects.Add(CalObj);
                    CalendarObjectsView.AddNewItem(CalObj);
                    CalendarObjectsView.CommitNew();
                    AdditionalCalObjSetup(CalObj);
                }
                return true;
            }
            return false;
        }
        private bool AllocateTimeResource(CalendarTimeTaskMap M)
        {
            // Find allocations where Resource is time related and Per is null
            var timeAllocs =
                from A in M.TimeTask.Allocations
                where A.Per == null
                where Resource.TimeResourceChoices.Contains(A.Resource.Name)
                select A;
            if (timeAllocs.Count() != 0)
            {
                TimeTaskAllocation A = timeAllocs.First();
                A.Remaining = A.ResourceAsTimeSpan().Ticks;
                EagerAllocate(M, A);
                return true;
            }
            return false;
        }
        private bool AllocateTimePerTime(CalendarTimeTaskMap M)
        {
            // Allocation.Amount is the time that will be spent for each Allocation.Per
            // segment aligned with the calendar. 

            // Find allocations with both Resource and Per that are time related
            var timePerTimeAllocs =
                from A in M.TimeTask.Allocations
                where Resource.TimeResourceChoices.Contains(A.Per.Name)
                where Resource.TimeResourceChoices.Contains(A.Resource.Name)
                select A;
            if (timePerTimeAllocs.Count() != 0)
            {
                TimeTaskAllocation A = timePerTimeAllocs.First();
                switch (A.Per.Name)
                {
                    case "Hour":
                        ATPTPart2(M, A, dt => dt.HourStart(), dt => dt.AddHours(1));
                        break;
                    case "Day":
                        ATPTPart2(M, A, dt => dt.Date, dt => dt.AddDays(1));
                        break;
                    case "Week":
                        ATPTPart2(M, A, dt => dt.WeekStart(), dt => dt.AddDays(7));
                        break;
                    case "Month":
                        ATPTPart2(M, A, dt => dt.MonthStart(), dt => dt.AddMonths(1));
                        break;
                    case "Year":
                        ATPTPart2(M, A, dt => dt.YearStart(), dt => dt.AddYears(1));
                        break;
                }
                return true;
            }
            return false;
        }
        private void ATPTPart2(CalendarTimeTaskMap M, TimeTaskAllocation A, 
            Func<DateTime, DateTime> starter, Func<DateTime, DateTime> adder)
        {
            var allocatedTime = A.ResourceAsTimeSpan().Ticks;
            var perTime = A.PerAsTimeSpan().Ticks;
            if (allocatedTime > perTime) throw new ArgumentException(
                "TimeTaskAllocation.Resource must be smaller than TimeTaskAllocation.Per. ", nameof(A));

            CalendarTimeTaskMap perMap;
            DateTime dt = M.InclusionZones.First().Start;
            dt = starter(dt);
            while (dt < M.TimeTask.End)
            {
                DateTime next = adder(dt);
                A.Remaining = allocatedTime;
                //get subset of zones intersecting current per
                perMap = new CalendarTimeTaskMap()
                {
                    TimeTask = M.TimeTask,
                    InclusionZones = new List<InclusionZone>(
                        from Z in M.InclusionZones
                        where Z.Intersects(dt, next)
                        select Z)
                };
                EvenAllocate(perMap, A);
                dt = next;
            }
        }
        private void EagerAllocate(CalendarTimeTaskMap Map, TimeTaskAllocation A)
        {
            // Fill the earliest zones first
            foreach (var Z in Map.InclusionZones)
            {
                if (A.Remaining <= 0) break;
                if (Z.Duration.Ticks <= 0) break;
                CalendarObject CalObj = new CalendarObject();
                if (A.RemainingAsTimeSpan < Z.Duration)
                {
                    //create cal obj the size of the remaining time
                    CalObj.Start = Z.Start;
                    CalObj.End = Z.Start + A.RemainingAsTimeSpan;
                    A.Remaining = 0;
                }
                else
                {
                    //create cal obj the size of the zone
                    CalObj.Start = Z.Start;
                    CalObj.End = Z.End;
                    A.Remaining -= Z.Duration.Ticks;
                }
                CalObj.ParentEntity = Map.TimeTask;
                Z.CalendarObjects.Add(CalObj);
                CalendarObjectsView.AddNewItem(CalObj);
                CalendarObjectsView.CommitNew();
                AdditionalCalObjSetup(CalObj);
            }
        }
        private void EvenAllocate(CalendarTimeTaskMap Map, TimeTaskAllocation A)
        {
            // Fill the zones evenly

            // Solution 1: add time to 1, check allocations, add time to 2, check allocations etc...
            // repeat, stop when allocations met or no more space

            // We will iterate through each zone adding time to the associated CalendarObject.
            //TODO
            //this should be just the zones within the current per
            foreach (var zone in Zones)
            {
                AllocatedCalendarObjects.Add(zone, new CalendarObject()
                {
                    Start = zone.Key,
                    End = zone.Key
                });
            }
            // get the remaining area of the zone with the smallest remaining area greater than zero
            var smallest = AllocatedCalendarObjects.Min(Z => new TimeSpan(Math.Max(0, Z.Key.Value.Ticks - (Z.Value.Duration).Ticks)));
            // get the total area if that area was distributed to every zone
            var smallestDistributed = new TimeSpan(smallest.Ticks * zoneCount);
            // check if the remaining time can be at least partially distributed with the above amount
            if (smallestDistributed < A.RemainingAsTimeSpan)
            {
                //distribute the smallest
                foreach (var Z in AllocatedCalendarObjects)
                {
                    Z.Value.End += smallest;
                }
            }
            // else evenly distribute remaining allocation, then distribute remainder allocation using solution 1
            else
            {
                //TODO: Maybe I should just use solution 1 always because it would be awkward to spread a task too thin and should at least be in chunks of 5 minutes
            }
            // find all zones that aren't full and repeat until allocation is met
        }
        private void ApatheticAllocate(TimeTask T, TimeTaskAllocation A, Dictionary<DateTime, TimeSpan> Zones)
        {
            //TODO
        }
        protected virtual void AdditionalCalObjSetup(CalendarObject CalObj) { }
        protected bool IsTaskRelevant(TimeTask task)
        {
            return IsDateRangeRelevant(task.Start, task.End);
        }
        protected bool IsNoteRelevant(Note note)
        {
            return IsDateRelevant(note.DateTime);
        }
        protected bool IsDateRelevant(DateTime date)
        {
            return (date >= SelectedDate && date <= EndDate);
        }
        protected bool IsDateRangeRelevant(DateTime d1, DateTime d2)
        {
            return (d1 < d2) && (IsDateRelevant(d1) || IsDateRelevant(d2));
        }
        protected virtual void Previous()
        {
            SetUpCalendarObjects();
        }
        protected virtual void Next()
        {
            SetUpCalendarObjects();
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
        #endregion
    }
}