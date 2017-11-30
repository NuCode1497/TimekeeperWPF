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
            await base.GetDataAsync();

            Status = "Creating CalendarObjects...";
            await CreateTaskObjects();
        }
        private async Task CreateTaskObjects()
        {
            View.Filter = T => ((TimeTask)T).Intersects(SelectedDate, EndDate);
            foreach (TimeTask T in View)
            {
                await T.BuildZonesAsync(SelectedDate, EndDate);
            }
            BuildTaskMaps();
            BuildAllocations();
            //DetermineTaskStates();
            //CalculateCollisions
            UnZipTaskMaps();
        }
        private void BuildTaskMaps()
        {
            TaskMaps = new List<CalendarTimeTaskMap>();
            foreach (TimeTask T in View)
            {
                var map = new CalendarTimeTaskMap
                {
                    TimeTask = T,
                    PerZones = new List<PerZone>(),
                };
                foreach (var P in T.PerZones)
                {
                    var per = new PerZone
                    {
                        Start = P.Key,
                        End = P.Value,
                        //Consumptions = new List<Consumption>(),
                        InclusionZones = new List<InclusionZone>(),
                        CalTaskObjs = new List<CalendarTaskObject>(),
                        CalNoteObjs = new List<CalendarNoteObject>(),
                    };
                    if (T.TimeAllocation != null)
                    {
                        per.TimeConsumption = new Consumption
                        {
                            Allocation = T.TimeAllocation,
                            Remaining = T.TimeAllocation.AmountAsTimeSpan().Ticks,
                        };
                    }
                    //foreach (var A in T.Allocations)
                    //{
                    //    if (A == T.TimeAllocation) continue;
                    //    var consume = new Consumption
                    //    {
                    //        Allocation = A,
                    //        Remaining = A.Amount,
                    //    };
                    //    per.Consumptions.Add(consume);
                    //}
                    var inZones = T.InclusionZones.Where(Z => per.Intersects(Z.Key, Z.Value));
                    foreach (var Z in inZones)
                    {
                        var zone = new InclusionZone
                        {
                            Start = Z.Key,
                            End = Z.Value,
                        };
                        per.InclusionZones.Add(zone);
                    }
                    map.PerZones.Add(per);
                }
                TaskMaps.Add(map);
            }
        }
        private void AllocateTimeFromNotes()
        {
            //get the relevant notes in each dimension
            var noteDimensions =
                from N in NotesVM.Source
                where N.TimeTask != null
                from M in TaskMaps
                where M.TimeTask.Dimension == N.TimeTask.Dimension
                from P in M.PerZones
                where P.Intersects(N)
                orderby N.DateTime
                group N by N.TimeTask.Dimension;
            foreach (var dimension in noteDimensions)
            {
                CalendarTaskObject calObj = null;
                PerZone per = null;
                InclusionZone inZone = null;
                Note prevN = null;
                foreach (var N in dimension)
                {
                    //check if per ended
                    if (per != null)
                    {
                        if (N.DateTime > per.End)
                        {
                            calObj.End = per.End;

                        }
                        else if (per.End == N.DateTime)
                        {
                            calObj.End = per.End;
                        }
                    }
                    switch (N.Text.ToLower())
                    {
                        case "start":
                            if (calObj != null)
                            {
                                calObj.End = N.DateTime;

                            }
                            calObj = new CalendarTaskObject
                            {
                                Start = N.DateTime,
                                End = N.DateTime + TimeTask.MinimumDuration,
                                ParentMap = TaskMaps.First(M => M.TimeTask == N.TimeTask),
                            };
                            break;
                    }
                    prevN = N;
                }
            }
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
                    foreach (var CalObj in P.CalNoteObjs)
                    {
                        CalNoteObjsView.AddNewItem(CalObj);
                        CalNoteObjsView.CommitNew();
                    }
                }
            }
            OnPropertyChanged(nameof(CalTaskObjsView));
            OnPropertyChanged(nameof(CalNoteObjsView));
        }
        protected virtual void AdditionalCalObjSetup(CalendarTaskObject CalObj) { }
        private void DetermineTaskStates()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    foreach (var N in P.CalNoteObjs)
                    {
                        bool hasTask = N.Note.TimeTask != null;
                        bool isOverInZone = P.InclusionZones.Count(Z => Z.Intersects(N.Note)) > 0;
                        var CalObj = P.CalTaskObjs.Where(C => C.Intersects(N.Note)).FirstOrDefault();
                        bool isOverCalObj = CalObj != null;
                        bool isNoteInPast = N.Note.DateTime <= DateTime.Now;
                        if (hasTask)
                        {
                            if (isOverInZone)
                            {
                                if (isOverCalObj)
                                {
                                    if (isNoteInPast)
                                    {
                                        switch (N.Note.Text)
                                        {
                                            case "Complete":
                                                CalObj.State = CalendarTaskObject.States.Completed;
                                                break;
                                            case "Confirm":
                                                CalObj.State = CalendarTaskObject.States.Completed;
                                                break;
                                            case "Incomplete":
                                                CalObj.State = CalendarTaskObject.States.Incomplete;
                                                break;
                                            case "Cancel":
                                                CalObj.State = CalendarTaskObject.States.Incomplete;
                                                break;
                                            case "Start":
                                                CalObj.Start = N.Note.DateTime;
                                                break;
                                            case "End":
                                                CalObj.End = N.Note.DateTime;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (N.Note.Text)
                                        {
                                            case "Complete":
                                                CalObj.State = CalendarTaskObject.States.Completed;
                                                break;
                                            case "Confirm":
                                                CalObj.State = CalendarTaskObject.States.Completed;
                                                break;
                                            case "Incomplete":
                                                CalObj.State = CalendarTaskObject.States.Incomplete;
                                                break;
                                            case "Cancel":
                                                CalObj.State = CalendarTaskObject.States.Incomplete;
                                                break;
                                            case "Start":
                                                CalObj.Start = N.Note.DateTime;
                                                break;
                                            case "End":
                                                CalObj.End = N.Note.DateTime;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
