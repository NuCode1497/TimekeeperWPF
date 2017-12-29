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
using System.ComponentModel;

namespace TimekeeperWPF
{
    public enum CalendarObjectTypes
    { CheckIn, Note, Task }

    public abstract class CalendarViewModel : ObservableObject, IView, IDisposable, IZone
    {
        private Orientation _Orientation = Orientation.Vertical;
        private String _status = "Ready";
        private bool _IsEnabled = true;
        private bool _Max = false;
        private bool _TextMargin = true;
        private ICommand _OrientationCommand;
        public abstract string Name { get; }
        public String Status
        {
            get { return _status; }
            protected set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
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
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            protected set
            {
                _IsEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEnabled));
            }
        }
        public bool IsNotEnabled => !IsEnabled;
        public ICommand OrientationCommand => _OrientationCommand
            ?? (_OrientationCommand = new RelayCommand(ap => ToggleOrientation(), pp => CanOrientation));
        protected virtual bool CanOrientation => true;
        public virtual bool CanMax => true;
        public virtual bool CanTextMargin => true;
        protected virtual void ToggleOrientation()
        {
            if (Orientation == Orientation.Horizontal)
                Orientation = Orientation.Vertical;
            else
                Orientation = Orientation.Horizontal;
        }
        #region Navigate
        private ICommand _PreviousCommand;
        private ICommand _NextCommand;
        private ICommand _SelectWeekCommand;
        private ICommand _SelectDayCommand;
        private ICommand _SelectYearCommand;
        private ICommand _SelectMonthCommand;
        public ICommand PreviousCommand => _PreviousCommand
            ?? (_PreviousCommand = new RelayCommand(async ap => await PreviousAsync(), pp => CanPrevious));
        public ICommand NextCommand => _NextCommand
            ?? (_NextCommand = new RelayCommand(async ap => await NextAsync(), pp => CanNext));
        public ICommand SelectWeekCommand => _SelectWeekCommand
            ?? (_SelectWeekCommand = new RelayCommand(ap => SelectWeek(), pp => CanSelectWeek));
        public ICommand SelectDayCommand => _SelectDayCommand
            ?? (_SelectDayCommand = new RelayCommand(ap => SelectDay(), pp => CanSelectDay));
        public ICommand SelectYearCommand => _SelectYearCommand
            ?? (_SelectYearCommand = new RelayCommand(ap => SelectYear(), pp => CanSelectYear));
        public ICommand SelectMonthCommand => _SelectMonthCommand
            ?? (_SelectMonthCommand = new RelayCommand(ap => SelectMonth(), pp => CanSelectMonth));
        protected virtual bool CanPrevious => IsNotLoading;
        protected virtual bool CanNext => IsNotLoading;
        protected virtual bool CanSelectWeek => IsNotLoading;
        protected virtual bool CanSelectDay => IsNotLoading;
        protected virtual bool CanSelectYear => IsNotLoading;
        protected virtual bool CanSelectMonth => IsNotLoading;
        public event RequestViewChangeEventHandler RequestViewChange;
        protected virtual void OnRequestViewChange(RequestViewChangeEventArgs e)
        { RequestViewChange?.Invoke(this, e); }
        protected virtual void SelectWeek()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Week, Start)); }
        protected virtual void SelectMonth()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Month, Start)); }
        protected virtual void SelectYear()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Year, Start)); }
        protected virtual void SelectDay()
        { OnRequestViewChange(new RequestViewChangeEventArgs(CalendarViewType.Day, Start)); }
        protected virtual async Task PreviousAsync()
        {
            IsLoading = true;
            await CreateCalendarObjects();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual async Task NextAsync()
        {
            IsLoading = true;
            await CreateCalendarObjects();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion Navigate
        #region Scale
        private int _ScaleSudoCommand;
        private ICommand _ScaleUpCommand;
        private ICommand _ScaleDownCommand;
        public int ScaleSudoCommand
        {
            get { return _ScaleSudoCommand; }
            private set
            {
                _ScaleSudoCommand = value;
                OnPropertyChanged();
            }
        }
        public ICommand ScaleUpCommand => _ScaleUpCommand
            ?? (_ScaleUpCommand = new RelayCommand(ap => ScaleUp(), pp => CanScaleUp));
        public ICommand ScaleDownCommand => _ScaleDownCommand
            ?? (_ScaleDownCommand = new RelayCommand(ap => ScaleDown(), pp => CanScaleDown));
        protected virtual bool CanScaleUp => true;
        protected virtual bool CanScaleDown => true;
        protected virtual void ScaleUp() { ScaleSudoCommand = 1; }
        protected virtual void ScaleDown() { ScaleSudoCommand = -1; }
        #endregion Scale
        #region Zone
        private DateTime _Start;
        public virtual DateTime Start
        {
            get { return _Start; }
            set
            {
                DateTime newValue = value.Date;
                if (_Start == newValue) return;
                _Start = newValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Year));
                OnPropertyChanged(nameof(Month));
                OnPropertyChanged(nameof(Day));
                OnPropertyChanged(nameof(MonthString));
                OnPropertyChanged(nameof(YearString));
                OnPropertyChanged(nameof(DayLongString));
                OnPropertyChanged(nameof(WeekString));
                OnPropertyChanged(nameof(End));
            }
        }
        public abstract DateTime End { get; set; }
        public TimeSpan Duration => End - Start;
        public int Year => Start.Year;
        public int Month => Start.Month;
        public int Day => Start.Day;
        public string DayLongString => Start.ToLongDateString();
        public string YearString => Start.ToString("yyy");
        public string MonthString => Start.ToString("MMMM");
        public string WeekString => Start.ToString("MMMM dd, yyy");
        #endregion Zone
        #region CRUDS
        private TimeTasksViewModel _TimeTasksVM = new TimeTasksViewModel();
        private CheckInsViewModel _CheckInsVM = new CheckInsViewModel();
        private NotesViewModel _NotesVM = new NotesViewModel();
        private UIElement _SelectedItem;
        private UIElement _CurrentEditItem;
        private CalendarObjectTypes _SelectedItemType;
        private CalendarObjectTypes _CurrentEditItemType;
        private bool _IsLoading = false;
        private bool _IsEditingItem = false;
        private bool _IsAddingNew = false;
        private bool _HasSelected = false;
        private bool _IsSaving = false;
        private ICommand _CancelCommand;
        private ICommand _CommitCommand;
        private ICommand _GetDataCommand;
        private ICommand _NewItemCommand;
        private ICommand _NewNoteCommand;
        private ICommand _NewCheckInCommand;
        private ICommand _NewTimeTaskCommand;
        private ICommand _EditSelectedCommand;
        private ICommand _DeleteSelectedCommand;
        private ICommand _SaveAsCommand;
        public NotesViewModel NotesVM
        {
            get { return _NotesVM; }
            set
            {
                _NotesVM = value;
                OnPropertyChanged();
            }
        }
        public CheckInsViewModel CheckInsVM
        {
            get { return _CheckInsVM; }
            set
            {
                _CheckInsVM = value;
                OnPropertyChanged();
            }
        }
        public TimeTasksViewModel TimeTasksVM
        {
            get { return _TimeTasksVM; }
            set
            {
                _TimeTasksVM = value;
                OnPropertyChanged();
            }
        }
        public CollectionViewSource CalTaskObjsCollection { get; set; }
        public ObservableCollection<CalendarTaskObject> CalTaskObjsSource =>
            CalTaskObjsCollection?.Source as ObservableCollection<CalendarTaskObject>;
        public ListCollectionView CalTaskObjsView =>
            CalTaskObjsCollection?.View as ListCollectionView;
        public CollectionViewSource CalNoteObjsCollection { get; set; }
        public ObservableCollection<CalendarNoteObject> CalNoteObjsSource =>
            CalNoteObjsCollection?.Source as ObservableCollection<CalendarNoteObject>;
        public ListCollectionView CalNoteObjsView =>
            CalNoteObjsCollection?.View as ListCollectionView;
        public CollectionViewSource CalCIObjsCollection { get; set; }
        public ObservableCollection<CalendarCheckInObject> CalCIObjsSource =>
            CalCIObjsCollection?.Source as ObservableCollection<CalendarCheckInObject>;
        public ListCollectionView CalCIObjsView =>
            CalCIObjsCollection?.View as ListCollectionView;
        public UIElement SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (IsEditingItemOrAddingNew)
                {
                    OnPropertyChanged();
                    return;
                }
                if (_SelectedItem == value) return;
                if (value is NowMarkerHorizontal ||
                    value is NowMarkerVertical)
                {
                    SelectedItem = null;
                    return;
                }
                if (value is CalendarCheckInObject)
                {
                    SelectedItemType = CalendarObjectTypes.CheckIn;
                    CheckInsVM.SelectedItem = ((CalendarCheckInObject)value).CheckIn;
                }
                else if (value is CalendarNoteObject)
                {
                    SelectedItemType = CalendarObjectTypes.Note;
                    NotesVM.SelectedItem = ((CalendarNoteObject)value).Note;
                }
                else if (value is CalendarTaskObject)
                {
                    SelectedItemType = CalendarObjectTypes.Task;
                    TimeTasksVM.SelectedItem = ((CalendarTaskObject)value).ParentPerZone.ParentMap.TimeTask;
                }
                _SelectedItem = value;
                if (SelectedItem == null)
                {
                    HasSelected = false;
                }
                else
                {
                    HasSelected = true;
                    Status = SelectedItemType + " Selected";
                }
                OnPropertyChanged();
            }
        }
        public UIElement CurrentEditItem
        {
            get { return _CurrentEditItem; }
            protected set
            {
                if (IsEditingItemOrAddingNew)
                {
                    OnPropertyChanged();
                    return;
                }
                if (value == _CurrentEditItem) return;
                if (value is NowMarkerHorizontal ||
                    value is NowMarkerVertical)
                {
                    return;
                }
                if (value is CalendarCheckInObject)
                {
                    CurrentEditItemType = CalendarObjectTypes.CheckIn;
                }
                else if (value is CalendarNoteObject)
                {
                    CurrentEditItemType = CalendarObjectTypes.Note;
                }
                else if (value is CalendarTaskObject)
                {
                    CurrentEditItemType = CalendarObjectTypes.Task;
                }
                _CurrentEditItem = value;
                OnPropertyChanged();
            }
        }
        public CalendarObjectTypes SelectedItemType
        {
            get { return _SelectedItemType; }
            set
            {
                _SelectedItemType = value;
                OnPropertyChanged();
            }
        }
        public CalendarObjectTypes CurrentEditItemType
        {
            get { return _CurrentEditItemType; }
            set
            {
                _CurrentEditItemType = value;
                OnPropertyChanged();
            }
        }
        public bool IsLoading
        {
            get { return _IsLoading; }
            protected set
            {
                _IsLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
        public bool IsEditingItem
        {
            get { return _IsEditingItem; }
            protected set
            {
                _IsEditingItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditingItem));
                OnPropertyChanged(nameof(IsEditingItemOrAddingNew));
                OnPropertyChanged(nameof(IsNotEditingItemOrAddingNew));
            }
        }
        public bool IsAddingNew
        {
            get { return _IsAddingNew; }
            protected set
            {
                _IsAddingNew = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotAddingNew));
                OnPropertyChanged(nameof(IsEditingItemOrAddingNew));
                OnPropertyChanged(nameof(IsNotEditingItemOrAddingNew));
            }
        }
        public bool IsSaving
        {
            get { return _IsSaving; }
            set
            {
                _IsSaving = value;
                OnPropertyChanged();
            }
        }
        public bool HasSelected
        {
            get { return _HasSelected; }
            protected set
            {
                _HasSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotSelected));
            }
        }
        public bool IsEditingItemOrAddingNew => IsEditingItem || IsAddingNew;
        public bool IsNotEditingItemOrAddingNew => !IsEditingItemOrAddingNew;
        public bool IsNotLoading => !IsLoading;
        public bool IsNotEditingItem => !IsEditingItem;
        public bool IsNotAddingNew => !IsAddingNew;
        public bool IsNotSaving => !IsSaving;
        public bool HasNotSelected => !HasSelected;
        public ICommand CancelCommand => _CancelCommand
            ?? (_CancelCommand = new RelayCommand(ap => Cancel(), pp => CanCancel));
        public ICommand CommitCommand => _CommitCommand
            ?? (_CommitCommand = new RelayCommand(async ap => await Commit(), pp => CanCommit));
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(async ap => await LoadData(), pp => CanGetData));
        public ICommand NewItemCommand => _NewItemCommand
            ?? (_NewItemCommand = new RelayCommand(ap => AddNew((CalendarObjectTypes)ap), pp => 
            pp is CalendarObjectTypes && CanAddNew((CalendarObjectTypes)pp)));
        public ICommand EditSelectedCommand => _EditSelectedCommand
            ?? (_EditSelectedCommand = new RelayCommand(ap => EditSelected(), pp => CanEditSelected));
        public ICommand DeleteSelectedCommand => _DeleteSelectedCommand
            ?? (_DeleteSelectedCommand = new RelayCommand(async ap => await DeleteSelected(), pp => CanDeleteSelected));
        public ICommand SaveAsCommand => _SaveAsCommand
            ?? (_SaveAsCommand = new RelayCommand(ap => SaveAs(), pp => CanSave));
        public ICommand NewNoteCommand => _NewNoteCommand
            ?? (_NewNoteCommand = new RelayCommand(ap => NewNote(), pp => CanAddNewNote));
        public ICommand NewCheckInCommand => _NewCheckInCommand
            ?? (_NewCheckInCommand = new RelayCommand(ap => NewCheckIn(), pp => CanAddNewCheckIn));
        public ICommand NewTimeTaskCommand => _NewTimeTaskCommand
            ?? (_NewTimeTaskCommand = new RelayCommand(ap => NewTimeTask(), pp => CanAddNewTimeTask));
        private bool IsReady => IsNotSaving && IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew;
        protected virtual bool CanCancel
        {
            get
            {
                if (IsEditingItemOrAddingNew)
                {
                    switch (CurrentEditItemType)
                    {
                        case CalendarObjectTypes.CheckIn:
                            return CheckInsVM?.CancelCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Note:
                            return NotesVM?.CancelCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Task:
                            return TimeTasksVM?.CancelCommand?.CanExecute(null) ?? false;
                    }
                }
                return false;
            }
        }
        protected virtual bool CanCommit
        {
            get
            {
                if (IsEditingItemOrAddingNew)
                {
                    switch (CurrentEditItemType)
                    {
                        case CalendarObjectTypes.CheckIn:
                            return CheckInsVM?.CommitCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Note:
                            return NotesVM?.CommitCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Task:
                            return TimeTasksVM?.CommitCommand?.CanExecute(null) ?? false;
                    }
                }
                return false;
            }
        }
        protected virtual bool CanGetData
        {
            get
            {
                return IsNotSaving
                    && IsNotLoading
                    && (NotesVM == null ? true : NotesVM?.GetDataCommand?.CanExecute(null) ?? false)
                    && (CheckInsVM == null ? true : CheckInsVM?.GetDataCommand?.CanExecute(null) ?? false)
                    && (TimeTasksVM == null ? true : TimeTasksVM?.GetDataCommand?.CanExecute(null) ?? false);
            }
        }
        protected virtual bool CanAddNew(CalendarObjectTypes type)
        {
            if (IsReady)
            {
                switch (type)
                {
                    case CalendarObjectTypes.CheckIn:
                        return CheckInsVM?.NewItemCommand?.CanExecute(null) ?? false;
                    case CalendarObjectTypes.Note:
                        return NotesVM?.NewItemCommand?.CanExecute(null) ?? false;
                    case CalendarObjectTypes.Task:
                        return TimeTasksVM?.NewItemCommand?.CanExecute(null) ?? false;
                }
            }
            return false;
        }
        protected virtual bool CanEditSelected
        {
            get
            {
                if (IsReady && HasSelected)
                {
                    switch (SelectedItemType)
                    {
                        case CalendarObjectTypes.CheckIn:
                            return CheckInsVM?.EditSelectedCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Note:
                            return NotesVM?.EditSelectedCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Task:
                            return TimeTasksVM?.EditSelectedCommand?.CanExecute(null) ?? false;
                    }
                }
                return false;
            }
        }
        protected virtual bool CanDeleteSelected
        {
            get
            {
                if (IsReady && HasSelected)
                {
                    switch (SelectedItemType)
                    {
                        case CalendarObjectTypes.CheckIn:
                            return CheckInsVM?.DeleteSelectedCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Note:
                            return NotesVM?.DeleteSelectedCommand?.CanExecute(null) ?? false;
                        case CalendarObjectTypes.Task:
                            return TimeTasksVM?.DeleteSelectedCommand?.CanExecute(null) ?? false;
                    }
                }
                return false;
            }
        }
        protected virtual bool CanSave
        {
            get
            {
                return IsReady
                    && (NotesVM?.SaveAsCommand?.CanExecute(null) ?? false)
                    && (CheckInsVM?.SaveAsCommand?.CanExecute(null) ?? false)
                    && (TimeTasksVM?.SaveAsCommand?.CanExecute(null) ?? false);
            }
        }
        protected virtual bool CanAddNewNote => IsReady && (NotesVM?.NewItemCommand?.CanExecute(null) ?? false);
        protected virtual bool CanAddNewCheckIn => IsReady && (CheckInsVM?.NewItemCommand?.CanExecute(null) ?? false);
        protected virtual bool CanAddNewTimeTask => IsReady && (TimeTasksVM?.NewItemCommand?.CanExecute(null) ?? false);
        public virtual async Task LoadData()
        {
            IsEnabled = false;
            IsLoading = true;
            Cancel();
            SelectedItem = null;
            Status = "Loading Data...";
            Dispose();
            try
            {
                await GetDataAsync();
                IsEnabled = true;
                Status = "Ready";
            }
            catch (Exception ex)
            {
                ExceptionViewer ev = new ExceptionViewer(String.Format("Error Loading {0} Data", Name), ex);
                ev.ShowDialog();
                Status = "Failed to get data!";
            }
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual async Task GetDataAsync()
        {
            Status = "Getting data from database...";
            await NotesVM.LoadDataAsync();
            await CheckInsVM.LoadDataAsync();
            await TimeTasksVM.LoadDataAsync();
            await CreateCalendarObjects();
        }
        protected virtual void AddNew(CalendarObjectTypes type)
        {
            switch (type)
            {
                case CalendarObjectTypes.CheckIn:
                    NewCheckIn();
                    break;
                case CalendarObjectTypes.Note:
                    NewNote();
                    break;
                case CalendarObjectTypes.Task:
                    NewTimeTask();
                    break;
            }
        }
        protected virtual void NewCheckIn()
        {
            CurrentEditItemType = CalendarObjectTypes.CheckIn;
            CheckInsVM.NewItemCommand.Execute(null);
            Status = "Adding new CheckIn";
            SelectedItem = null;
            IsAddingNew = true;
        }
        protected virtual void NewNote()
        {
            CurrentEditItemType = CalendarObjectTypes.Note;
            NotesVM.NewItemCommand.Execute(null);
            Status = "Adding new Note";
            SelectedItem = null;
            IsAddingNew = true;
        }
        protected virtual void NewTimeTask()
        {
            CurrentEditItemType = CalendarObjectTypes.Task;
            TimeTasksVM.NewItemCommand.Execute(null);
            Status = "Adding new Task";
            SelectedItem = null;
            IsAddingNew = true;
        }
        protected virtual void EditSelected()
        {
            CurrentEditItem = SelectedItem;
            SelectedItem = null;
            IsEditingItem = true;
            switch (SelectedItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    Cancel();
                    break;
                case CalendarObjectTypes.Note:
                    NotesVM.EditSelectedCommand.Execute(null);
                    break;
                case CalendarObjectTypes.Task:
                    TimeTasksVM.EditSelectedCommand.Execute(null);
                    break;
            }
            Status = "Editing " + CurrentEditItemType;
        }
        protected virtual void EndEdit()
        {
            IsEditingItem = false;
            IsAddingNew = false;
            CurrentEditItem = null;
            SelectedItem = null;
            //Refresh all of the buttons
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual void Cancel()
        {
            CheckInsVM?.CancelCommand?.Execute(null);
            NotesVM?.CancelCommand?.Execute(null);
            TimeTasksVM?.CancelCommand?.Execute(null);
            Status = "Canceled";
            EndEdit();
        }
        protected virtual async Task Commit()
        {
            switch (CurrentEditItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    if (IsAddingNew)
                    {
                        var NCIO = MapNewCheckIn(CheckInsVM.CurrentEditItem);
                        MapTaskObjects();
                        if (NCIO != null) AddNewCheckInObject(NCIO);
                        OnPropertyChanged(nameof(CalCIObjsView));
                        Status = CalendarObjectTypes.CheckIn + " Added";
                        CheckInsVM.CommitCommand.Execute(null);
                    }
                    break;
                case CalendarObjectTypes.Note:
                    if (IsAddingNew)
                    {
                        AddNewNoteObject(NotesVM.CurrentEditItem);
                        OnPropertyChanged(nameof(CalNoteObjsView));
                        Status = CalendarObjectTypes.Note + " Added";
                        NotesVM.CommitCommand.Execute(null);
                    }
                    else if (IsEditingItem)
                    {
                        var N = CurrentEditItem as CalendarNoteObject;
                        N.Note = NotesVM.CurrentEditItem;
                        OnPropertyChanged(nameof(CalNoteObjsView));
                        Status = CalendarObjectTypes.Note + " Modified";
                        NotesVM.CommitCommand.Execute(null);
                    }
                    break;
                case CalendarObjectTypes.Task:
                    if (IsAddingNew)
                    {
                        await BuildNewTaskMap(TimeTasksVM.CurrentEditItem);
                        Status = CalendarObjectTypes.Task + " Added";
                        TimeTasksVM.CommitCommand.Execute(null);
                    }
                    else if (IsEditingItem)
                    {
                        var T = CurrentEditItem as CalendarTaskObject;
                        TaskMaps.Remove(T.ParentPerZone.ParentMap);
                        await BuildNewTaskMap(TimeTasksVM.CurrentEditItem);
                        Status = CalendarObjectTypes.Task + " Modified";
                        TimeTasksVM.CommitCommand.Execute(null);
                    }
                    await CheckInsVM.LoadDataAsync();
                    break;
            }
            EndEdit();
        }
        protected virtual async Task DeleteSelected()
        {
            switch (SelectedItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    var CCIO = SelectedItem as CalendarCheckInObject;
                    CCIO?.ParentPerZone.CheckIns.Remove(CCIO);
                    CheckInsVM.DeleteSelectedCommand.Execute(null);
                    MapTaskObjects();
                    CalCIObjsView.Remove(CCIO);
                    OnPropertyChanged(nameof(CalCIObjsView));
                    Status = CalendarObjectTypes.CheckIn + " Deleted";
                    break;
                case CalendarObjectTypes.Note:
                    NotesVM.DeleteSelectedCommand.Execute(null);
                    var N = SelectedItem as CalendarNoteObject;
                    CalNoteObjsView.Remove(N);
                    OnPropertyChanged(nameof(CalNoteObjsView));
                    Status = CalendarObjectTypes.Note + " Deleted";
                    break;
                case CalendarObjectTypes.Task:
                    var T = SelectedItem as CalendarTaskObject;
                    TaskMaps.Remove(T.ParentPerZone.ParentMap);
                    TimeTasksVM.DeleteSelectedCommand.Execute(null);
                    await BuildTaskMaps();
                    Status = CalendarObjectTypes.Task + " Deleted";
                    await CheckInsVM.LoadDataAsync();
                    break;
            }
            SelectedItem = null;
        }
        protected abstract void SaveAs();
        private async Task CreateCalendarObjects()
        {
            await BuildTaskMaps();
            CreateCheckInObjects();
            CreateNoteObjects();
            Status = "Ready";
        }
        private void CreateCheckInObjects()
        {
            CalCIObjsCollection = new CollectionViewSource();
            CalCIObjsCollection.Source = new ObservableCollection<CalendarCheckInObject>();
            var mappedEventCheckInObjects =
                from M in TaskMaps
                from P in M.PerZones
                from CIO in P.CheckIns
                where CIO.Kind == CheckInKind.EventEnd || CIO.Kind == CheckInKind.EventStart
                select CIO;
            var mappedEventCheckIns =
                from CIO in mappedEventCheckInObjects
                select CIO.CheckIn;
            var checkIns = CheckInsVM.Source
                .Where(CI => CI.IsInside(this))
                .Except(mappedEventCheckIns);
            foreach (var CI in checkIns)
                AddNewCheckInObject(new CalendarCheckInObject
                {
                    CheckIn = CI,
                    Kind = CI.Start ? CheckInKind.EventStart : CheckInKind.EventEnd,
                });
            foreach (var CIO in mappedEventCheckInObjects)
                AddNewCheckInObject(CIO);
            OnPropertyChanged(nameof(CalCIObjsView));
        }
        private void AddNewCheckInObject(CalendarCheckInObject CIO)
        {
            CIO.ToolTip = CIO.CheckIn;
            CalCIObjsView.AddNewItem(CIO);
            CalCIObjsView.CommitNew();
        }
        private void CreateNoteObjects()
        {
            CalNoteObjsCollection = new CollectionViewSource();
            CalNoteObjsCollection.Source = new ObservableCollection<CalendarNoteObject>();
            var notes = NotesVM.Source.Where(N => N.IsInside(this));
            foreach (var N in notes)
                AddNewNoteObject(N);
            OnPropertyChanged(nameof(CalNoteObjsView));
        }
        private void AddNewNoteObject(Note N)
        {
            var NO = new CalendarNoteObject
            {
                Note = N,
                ToolTip = N,
            };
            CalNoteObjsView.AddNewItem(NO);
            CalNoteObjsView.CommitNew();
        }
        #endregion CRUDS
        #region TaskMaps
        // See: Plans.xlsx - Calendar Passes, States, CheckIns, Collisions, Collisions2
        public HashSet<CalendarTimeTaskMap> TaskMaps;
        private async Task BuildTaskMaps()
        {
            Status = "Mapping Relevant Tasks...";
            TaskMaps = new HashSet<CalendarTimeTaskMap>();
            var TaskDimensions =
                from T in TimeTasksVM.Source
                group T by T.Dimension;
            foreach (var dimension in TaskDimensions)
            {
                var RelevantTasks = FindTaskSet(dimension, new HashSet<TimeTask>(), Start, End);
                await InitTaskMaps(dimension.Key, RelevantTasks);
            }
            MapCheckIns();
            MapTaskObjects();
            Status = "Ready";
        }
        private async Task BuildNewTaskMap(TimeTask task)
        {
            Status = "Mapping New Task...";
            var dimensionTasks =
                from T in TimeTasksVM.Source
                where T.Dimension == task.Dimension
                select T;
            var RelevantTasks = 
                FindTaskSet(dimensionTasks, new HashSet<TimeTask>(), task.Start, task.End)
                .Except(from M in TaskMaps select M.TimeTask);
            await InitTaskMaps(task.Dimension, RelevantTasks);
            MapCheckIns();
            MapTaskObjects();
            Status = "Ready";
        }
        #region InitTaskMaps
        private async Task InitTaskMaps(int dimension, IEnumerable<TimeTask> RelevantTasks)
        {
            //This function is more intensive than others, it will be called once on load.
            //Create Maps with all PerZones; we will reduce this set later.
            foreach (var T in RelevantTasks)
            {
                await T.BuildPerZonesAsync();
                var M = new CalendarTimeTaskMap
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
                        ParentMap = M,
                    };
                    M.PerZones.Add(per);
                }
                TaskMaps.Add(M);
            }
            var dimensionMaps =
                from M in TaskMaps
                where M.TimeTask.Dimension == dimension
                select M;
            var RelevantPerZones = FindPerSet(dimensionMaps, new HashSet<PerZone>(), Start, End);
            foreach (var M in dimensionMaps)
            {
                //Reduce the PerZone set.
                M.PerZones = new HashSet<PerZone>(M.PerZones.Intersect(RelevantPerZones));

                //Build InclusionZones with the reduced PerZone set.
                var pers = new Dictionary<DateTime, DateTime>();
                foreach (var P in M.PerZones)
                    pers.Add(P.Start, P.End);
                await M.TimeTask.BuildInclusionZonesAsync(pers);
                foreach (var P in M.PerZones)
                {
                    P.InclusionZones = new List<InclusionZone>();
                    var inZones = M.TimeTask.InclusionZones.Where(Z => P.Intersects(Z.Key, Z.Value));
                    foreach (var Z in inZones)
                    {
                        var zone = new InclusionZone
                        {
                            Start = Z.Key,
                            End = Z.Value,
                            ParentPerZone = P,
                        };
                        P.InclusionZones.Add(zone);
                    }
                }
            }
        }
        private IEnumerable<TimeTask> FindTaskSet(IEnumerable<TimeTask> tasks, IEnumerable<TimeTask> accumulatedFinds, DateTime start, DateTime end)
        {
            //Recursively select the set of Tasks that intersect the calendar view or previously added tasks.
            var foundTasks = (from T in tasks
                              where T.Intersects(start, end)
                              select T).Except(accumulatedFinds);
            if (foundTasks.Count() == 0) return accumulatedFinds;
            accumulatedFinds = new HashSet<TimeTask>(accumulatedFinds.Union(foundTasks));
            foreach (var T in foundTasks)
            {
                accumulatedFinds = new HashSet<TimeTask>(accumulatedFinds.Union(FindTaskSet(tasks, accumulatedFinds, T.Start, T.End)));
            }
            return accumulatedFinds;
        }
        private IEnumerable<PerZone> FindPerSet(IEnumerable<CalendarTimeTaskMap> maps, IEnumerable<PerZone> accumulatedFinds, DateTime start, DateTime end)
        {
            //Recursively select the set of PerZones that intersect the calendar view or previously added PerZones.
            var foundPers = (from M in maps
                             from P in M.PerZones
                             where P.Intersects(start, end)
                             select P).Except(accumulatedFinds);
            if (foundPers.Count() == 0) return accumulatedFinds;
            accumulatedFinds = new HashSet<PerZone>(accumulatedFinds.Union(foundPers));
            foreach (var P in foundPers)
            {
                accumulatedFinds = new HashSet<PerZone>(accumulatedFinds.Union(FindPerSet(maps, accumulatedFinds, P.Start, P.End)));
            }
            return accumulatedFinds;
        }
        #endregion InitTaskMaps
        private CalendarCheckInObject MapNewCheckIn(CheckIn CI)
        {
            foreach (var M in TaskMaps)
            {
                if (M.TimeTask.Id != CI.TimeTask.Id) continue;
                //find the correct Per
                foreach (var P in M.PerZones)
                {
                    if (!CI.IsInside(P)) continue;
                    var CCI = new CalendarCheckInObject
                    {
                        CheckIn = CI,
                        Kind = CI.Start ? CheckInKind.EventStart : CheckInKind.EventEnd,
                        ParentPerZone = P,
                    };
                    //add
                    P.CheckIns.Add(CCI);
                    //sort
                    P.CheckIns = new List<CalendarCheckInObject>(
                        from c in P.CheckIns
                        orderby c.DateTime, c.Kind
                        select c);
                    //set the parent inclusion zone
                    foreach (var Z in P.InclusionZones)
                    {
                        if (!CI.IsInside(Z)) continue;
                        CCI.ParentInclusionZone = Z;
                        return CCI;
                    }
                    return CCI;
                }
            }
            return null;
        }
        private void MapCheckIns()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    var eventCheckIns = new List<CalendarCheckInObject>();
                    var perCheckIns = new List<CalendarCheckInObject>();
                    var inZoneCheckIns = new List<CalendarCheckInObject>();
                    //find and map relevant event CIs in this PerZone
                    foreach (var CI in CheckInsVM.Source)
                    {
                        if (CI.TimeTask.Id != M.TimeTask.Id || !CI.IsInside(P)) continue;
                        eventCheckIns.Add(new CalendarCheckInObject
                        {
                            CheckIn = CI,
                            Kind = CI.Start ? CheckInKind.EventStart : CheckInKind.EventEnd,
                            ParentPerZone = P,
                        });
                    }
                    //map zone ends as CheckIns
                    perCheckIns.Add(new CalendarCheckInObject
                    {
                        CheckIn = new CheckIn
                        {
                            Start = true,
                            DateTime = P.Start,
                            TimeTask = P.ParentMap.TimeTask,
                        },
                        Kind = CheckInKind.PerZoneStart,
                        ParentPerZone = P,
                    });
                    perCheckIns.Add(new CalendarCheckInObject
                    {
                        CheckIn = new CheckIn
                        {
                            Start = false,
                            DateTime = P.End,
                            TimeTask = P.ParentMap.TimeTask,
                        },
                        Kind = CheckInKind.PerZoneEnd,
                        ParentPerZone = P,
                    });
                    foreach (var Z in P.InclusionZones)
                    {
                        //find and map relevant event CIs in this InclusionZone
                        foreach (var CI in eventCheckIns)
                        {
                            if (!CI.IsInside(Z)) continue;
                            CI.ParentInclusionZone = Z;
                        }
                        //map zone ends as CheckIns
                        inZoneCheckIns.Add(new CalendarCheckInObject
                        {
                            CheckIn = new CheckIn
                            {
                                Start = true,
                                DateTime = Z.Start,
                                TimeTask = P.ParentMap.TimeTask,
                            },
                            Kind = CheckInKind.InclusionZoneStart,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        });
                        inZoneCheckIns.Add(new CalendarCheckInObject
                        {
                            CheckIn = new CheckIn
                            {
                                Start = false,
                                DateTime = Z.End,
                                TimeTask = P.ParentMap.TimeTask,
                            },
                            Kind = CheckInKind.InclusionZoneEnd,
                            ParentPerZone = P,
                            ParentInclusionZone = Z,
                        });
                    }
                    //Merge and sort CheckIn sets.
                    var allCheckIns =
                        from CI in eventCheckIns.Union(perCheckIns).Union(inZoneCheckIns)
                        orderby CI.DateTime, CI.Kind
                        select CI;
                    P.CheckIns = new List<CalendarCheckInObject>(allCheckIns);
                }
            }
        }
        #region MapTaskObjects
        private void MapTaskObjects()
        {
            foreach (var M in TaskMaps)
                foreach (var P in M.PerZones)
                {
                    P.CalTaskObjs = new HashSet<CalendarTaskObject>();
                    if (M.TimeTask.TimeAllocation != null)
                    {
                        P.TimeConsumption = new Consumption
                        {
                            Allocation = M.TimeTask.TimeAllocation,
                            Remaining = M.TimeTask.TimeAllocation.AmountAsTimeSpan.Ticks,
                        };
                    }
                }
            AllocateTimeFromCheckIns();
            AllocateTimeFromFilters();
            CalculateCollisions();
            AllocateEmptySpace();
            CleanUpStates();
            UnZipTaskMaps();
        }
        private void AllocateTimeFromCheckIns()
        {
            Status = "Allocating Time From CheckIns...";
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    //Create CalObjs from CheckIns
                    CalendarTaskObject pC = null;
                    CalendarCheckInObject pN = null;
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
                                                //ParentMap = N.ParentMap,
                                                //ParentInclusionZone = N.ParentInclusionZone,
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
                                                //ParentMap = N.ParentMap,
                                                //ParentInclusionZone = N.ParentInclusionZone,
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
                                                //ParentMap = N.ParentMap,
                                                //ParentInclusionZone = N.ParentInclusionZone,
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
                                                //ParentMap = N.ParentMap,
                                                //ParentInclusionZone = N.ParentInclusionZone,
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
                                                //ParentMap = N.ParentMap,
                                                //ParentInclusionZone = N.ParentInclusionZone,
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
                    if (P.TimeConsumption != null)
                        P.TimeConsumption.Remaining -= spent.Ticks;
                }
            }
        }
        #region AllocateTimeFromFilters
        private void AllocateTimeFromFilters()
        {
            Status = "Allocating Time From Filters...";
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
                        //ParentMap = M,
                        ParentInclusionZone = Z,
                        ParentPerZone = P,
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
                            //ParentMap = M,
                            ParentInclusionZone = Z,
                            ParentPerZone = P,
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
                            //ParentMap = M,
                            ParentInclusionZone = Z,
                            ParentPerZone = P,
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
                        //ParentMap = M,
                        ParentInclusionZone = Z,
                        ParentPerZone = P,
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
                            //ParentMap = M,
                            ParentInclusionZone = Z,
                            ParentPerZone = P,
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
                            //ParentMap = M,
                            ParentInclusionZone = Z,
                            ParentPerZone = P,
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
            Status = "Calculating Collisions...";
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
                orderby C.ParentPerZone.ParentMap.TimeTask.Priority descending, C.Start, C.End
                group C by C.ParentPerZone.ParentMap.TimeTask.Dimension;
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
                            orderby C2.ParentPerZone.ParentMap.TimeTask.Priority descending, C2.Start, C2.End
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
                insider.ParentPerZone.ParentMap.TimeTask.Priority > outsider.ParentPerZone.ParentMap.TimeTask.Priority)
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
                    if (LData.Priority < C1.ParentPerZone.ParentMap.TimeTask.Priority) //3
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
                    if (C2.ParentPerZone.ParentMap.TimeTask.Priority > RData.Priority) //D
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
                    if (C2.ParentPerZone.ParentMap.TimeTask.Priority > RData.Priority) //F
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
                            if (LData.Priority < C1.ParentPerZone.ParentMap.TimeTask.Priority) //3
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
                            if (LData.Priority < C1.ParentPerZone.ParentMap.TimeTask.Priority) //3
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
            double LP = C.ParentPerZone.ParentMap.TimeTask.Priority;
            TimeSpan LmaxRoom = new TimeSpan();
            while (true)
            {
                //If C1.HasRoom == F, LC is the nearest LT where LT.S🔒 or LT.Z.S >=LT.S or LT.LT.E🔒. 
                //LP = C.P where C is the LT with the lowest priority from C1 to LC.
                if (LT.ParentPerZone.ParentMap.TimeTask.Priority < LP)
                {
                    LP = LT.ParentPerZone.ParentMap.TimeTask.Priority;
                    LmaxRoom = LT.ParentPerZone.ParentMap.TimeTask.Duration;
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
                    LP = LT.ParentPerZone.ParentMap.TimeTask.Priority;
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
            double RP = C.ParentPerZone.ParentMap.TimeTask.Priority;
            TimeSpan RmaxRoom = new TimeSpan();
            while (true)
            {
                if (RT.ParentPerZone.ParentMap.TimeTask.Priority < RP)
                {
                    RP = RT.ParentPerZone.ParentMap.TimeTask.Priority;
                    RmaxRoom = RT.ParentPerZone.ParentMap.TimeTask.Duration;
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
                    RP = RT.ParentPerZone.ParentMap.TimeTask.Priority;
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
            Status = "Allocating Empty Space...";
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
                        var zones =
                            from M in TaskMaps
                            where M.TimeTask.Dimension == dimension.Key
                            from P in M.PerZones
                            from Z in P.InclusionZones
                            where Z.Intersects(space)
                            select Z;
                        var insuffZones =
                            from Z in zones
                            where Z.ParentPerZone.TimeConsumption?.Remaining > 0
                            orderby Z.ParentPerZone.ParentMap.TimeTask.Priority descending
                            select Z;
                        var insuffZone = insuffZones.FirstOrDefault();
                        if (insuffZone != null)
                        {
                            FillEmptyWithInsuffZ(space, insuffZone);
                            hasChanges = true;
                        }
                        else
                        {
                            //no insufficient zone found
                            //look for CanFill zones
                            var fillZones =
                                from Z in zones
                                where Z.ParentPerZone.ParentMap.TimeTask.CanFill
                                orderby Z.ParentPerZone.ParentMap.TimeTask.Priority descending
                                select Z;
                            var fillZone = fillZones.FirstOrDefault();
                            if (fillZone != null)
                            {
                                FillEmptyWithFiller(space, fillZone);
                                hasChanges = true;
                            }
                        }
                    }
                }
            }
        }
        private static void FillEmptyWithInsuffZ(EmptyZone S, InclusionZone Z)
        {
            //Check if the CalObjs on the left or right of the space can be used to fill
            //otherwise, create a new CalObj to fill
            if (Z.Start <= S.Start &&
                S.Left?.ParentInclusionZone == Z &&
                !S.Left.EndLock)
            {
                var newEnd = new DateTime(Min(Z.End.Ticks, Min(S.End.Ticks,
                S.Left.End.Ticks + (long)Max(S.Left.ParentPerZone.TimeConsumption.Remaining, 0))));
                var diff = newEnd - S.Left.End;
                S.Left.End = newEnd;
                S.Left.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            if (Z.End >= S.End &&
                S.Right?.ParentInclusionZone == Z &&
                !S.Right.StartLock)
            {
                var newStart = new DateTime(Max(Z.Start.Ticks, Max(S.Start.Ticks,
                S.Right.Start.Ticks - (long)Max(S.Right.ParentPerZone.TimeConsumption.Remaining, 0))));
                var diff = S.Right.Start - newStart;
                S.Right.Start = newStart;
                S.Right.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            {
                var start = new DateTime(Max(Z.Start.Ticks, S.Start.Ticks));
                var end = new DateTime(Min(Z.End.Ticks, Min(S.End.Ticks,
                    start.Ticks + (long)Max(Z.ParentPerZone.TimeConsumption.Remaining, 0))));
                var C = new CalendarTaskObject
                {
                    Start = start,
                    End = end,
                    ParentPerZone = Z.ParentPerZone,
                    ParentInclusionZone = Z,
                };
                Z.ParentPerZone.CalTaskObjs.Add(C);
                Z.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
            }
        }
        private static void FillEmptyWithFiller(EmptyZone S, InclusionZone Z)
        {
            //Check if the CalObjs on the left or right of the space can be used to fill
            //otherwise, create a new CalObj to fill
            if (Z.Start <= S.Start &&
                S.Left?.ParentInclusionZone == Z &&
                !S.Left.EndLock)
            {
                var newEnd = new DateTime(Min(Z.End.Ticks, S.End.Ticks));
                var diff = newEnd - S.Left.End;
                S.Left.End = newEnd;
                if (S.Left.ParentPerZone.TimeConsumption != null)
                    S.Left.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            if (Z.End >= S.End &&
                S.Right?.ParentInclusionZone == Z &&
                !S.Right.StartLock)
            {
                var newStart = new DateTime(Max(Z.Start.Ticks, S.Start.Ticks));
                var diff = S.Right.Start - newStart;
                S.Right.Start = newStart;
                if (S.Right.ParentPerZone.TimeConsumption != null)
                    S.Right.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            {
                var start = new DateTime(Max(Z.Start.Ticks, S.Start.Ticks));
                var end = new DateTime(Min(Z.End.Ticks, S.End.Ticks));
                var C = new CalendarTaskObject
                {
                    Start = start,
                    End = end,
                    ParentPerZone = Z.ParentPerZone,
                    ParentInclusionZone = Z,
                };
                Z.ParentPerZone.CalTaskObjs.Add(C);
                if (Z.ParentPerZone.TimeConsumption != null)
                    Z.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
            }
        }
        private void CalculateTimeConsumptions()
        {
            foreach (var M in TaskMaps)
            {
                if (M.TimeTask.TimeAllocation == null) continue;
                foreach (var P in M.PerZones)
                {
                    TimeSpan spent = new TimeSpan(0);
                    foreach (var C in P.CalTaskObjs)
                        spent += C.Duration;
                    P.TimeConsumption.Remaining = P.TimeConsumption.Allocation.AmountAsTimeSpan.Ticks - spent.Ticks;
                }
            }
        }
        private IEnumerable<IGrouping<int, EmptyZone>> GetEmptySpaces()
        {
            //group COs by dimension
            var calObjDimensions =
                from M in TaskMaps
                from P in M.PerZones
                from C in P.CalTaskObjs
                orderby C.Start
                group C by C.ParentPerZone.ParentMap.TimeTask.Dimension;
            var spaceDimensions = new HashSet<Grouping<int, EmptyZone>>();
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
                    prev = C;
                }
                spaceDimensions.Add(spaces);
            }
            return spaceDimensions;
        }
        #endregion AllocateEmptySpace
        #region CleanUpStates
        private void CleanUpStates()
        {
            Status = "Cleaning Up...";
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    FixMisalignments(P);
                    foreach (var C in P.CalTaskObjs)
                    {
                        foreach (var Z in P.InclusionZones)
                        {
                            //Fix Wrong States for CalObjs over a zone
                            if (C.State == CalendarTaskObject.States.Unscheduled)
                            {
                                if (C.StartLock || C.EndLock)
                                    C.State = CalendarTaskObject.States.Confirmed;
                                else if (C.ParentPerZone.ParentMap.TimeTask.AutoCheckIn)
                                    C.State = CalendarTaskObject.States.AutoConfirm;
                                else
                                    C.State = CalendarTaskObject.States.Unconfirmed;
                            }
                        }
                        //Fix Wrong States for CalObjs not over a zone
                        if (C.ParentInclusionZone == null &&
                            C.State != CalendarTaskObject.States.Unscheduled)
                            C.State = CalendarTaskObject.States.Unscheduled;
                        //Flag Cancels
                        if (C.Start == C.End)
                            C.State = CalendarTaskObject.States.Cancel;
                    }
                    //Flag Insufficients
                    if (P.TimeConsumption?.Remaining > 0)
                        foreach (var C in P.CalTaskObjs)
                            C.State = CalendarTaskObject.States.Insufficient;
                }
            }
            FlagConflicts();
        }
        private static void FixMisalignments(PerZone P)
        {
            HashSet<CalendarTaskObject> newTaskObjs = new HashSet<CalendarTaskObject>();
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                foreach (var C in P.CalTaskObjs)
                {
                    foreach (var Z in P.InclusionZones)
                    {
                        //Fix Misalignments (this probably will never happen)
                        if (C.Intersects(Z) && (!C.IsInside(Z) || C.ParentInclusionZone != Z))
                        {
                            if (C.Start < Z.Start)
                            {
                                var split = new CalendarTaskObject();
                                split.Mimic(C);
                                split.End = Z.Start;
                                C.Start = Z.Start;
                                newTaskObjs.Add(split);
                            }
                            if (C.End > Z.End)
                            {
                                var split = new CalendarTaskObject();
                                split.Mimic(C);
                                split.Start = Z.End;
                                C.End = Z.End;
                                newTaskObjs.Add(split);
                            }
                            C.ParentInclusionZone = Z;
                        }
                    }
                }
                if (newTaskObjs.Count > 0)
                {
                    foreach (var C in newTaskObjs)
                        P.CalTaskObjs.Add(C);
                    newTaskObjs.Clear();
                    hasChanges = true;
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
                group C by C.ParentPerZone.ParentMap.TimeTask.Dimension;
            foreach (var dimension in calObjDimensions)
            {
                foreach (var C1 in dimension)
                {
                    foreach (var C2 in dimension)
                    {
                        if (C1 != C2 && C1.Intersects(C2))
                        {
                            C1.State = CalendarTaskObject.States.Conflict;
                            C2.State = CalendarTaskObject.States.Conflict;
                        }
                    }
                }
            }
        }
        #endregion CleanUpStates
        private void UnZipTaskMaps()
        {
            Status = "Un-Zipping TaskMaps...";
            CalTaskObjsCollection = new CollectionViewSource();
            CalTaskObjsCollection.Source = new ObservableCollection<CalendarTaskObject>();
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    foreach (var C in P.CalTaskObjs)
                    {
                        CalTaskObjsView.AddNewItem(C);
                        CalTaskObjsView.CommitNew();
                        AdditionalCalTaskObjSetup(C);
                    }
                }
            }
            OnPropertyChanged(nameof(CalTaskObjsView));
        }
        #endregion MapTaskObjects
        protected virtual void AdditionalCalTaskObjSetup(CalendarTaskObject CalObj) { }
        #endregion TaskMaps
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    CheckInsVM?.Dispose();
                    NotesVM?.Dispose();
                    TimeTasksVM?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ViewModel() {
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
