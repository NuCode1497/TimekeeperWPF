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
        public CalendarViewModel()
        {
            _TimeTasksVM.Parent = this;
            _CheckInsVM.Parent = this;
            _NotesVM.Parent = this;
        }
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
                    CheckInsVM.SelectedItem = CheckInsVM.Source.Where(CI => 
                    ((CalendarCheckInObject)value).CheckIn.Id == CI.Id).FirstOrDefault();
                }
                else if (value is CalendarNoteObject)
                {
                    SelectedItemType = CalendarObjectTypes.Note;
                    NotesVM.SelectedItem = NotesVM.Source.Where(CI =>
                    ((CalendarNoteObject)value).Note.Id == CI.Id).FirstOrDefault();
                }
                else if (value is CalendarTaskObject)
                {
                    SelectedItemType = CalendarObjectTypes.Task;
                    TimeTasksVM.SelectedItem = TimeTasksVM.Source.Where(CI =>
                    ((CalendarTaskObject)value).ParentPerZone.ParentMap.TimeTask.Id == CI.Id).FirstOrDefault();
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
        internal virtual async Task LoadData()
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
        internal virtual void AddNew(CalendarObjectTypes type)
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
        internal virtual void NewCheckIn()
        {
            CurrentEditItemType = CalendarObjectTypes.CheckIn;
            CheckInsVM.NewItemCommand.Execute(null);
            Status = "Adding new CheckIn";
            SelectedItem = null;
            IsAddingNew = true;
        }
        internal virtual void NewNote()
        {
            CurrentEditItemType = CalendarObjectTypes.Note;
            NotesVM.NewItemCommand.Execute(null);
            Status = "Adding new Note";
            SelectedItem = null;
            IsAddingNew = true;
        }
        internal virtual void NewTimeTask()
        {
            CurrentEditItemType = CalendarObjectTypes.Task;
            TimeTasksVM.NewItemCommand.Execute(null);
            Status = "Adding new Task";
            SelectedItem = null;
            IsAddingNew = true;
        }
        internal virtual void EditSelected()
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
        internal virtual void Cancel()
        {
            CheckInsVM?.Cancel();
            NotesVM?.Cancel();
            TimeTasksVM?.Cancel();
            Status = "Canceled";
            EndEdit();
        }
        internal virtual async Task<bool> Commit()
        {
            bool success = false;
            switch (CurrentEditItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    if (IsAddingNew)
                    {
                        var CI = CheckInsVM.CurrentEditItem;
                        success = await CheckInsVM.Commit();
                        if (!success) break;
                        var NCIO = MapNewCheckIn(CI);
                        MapTaskObjects();
                        if (NCIO != null) AddNewCheckInObject(NCIO);
                        OnPropertyChanged(nameof(CalCIObjsView));
                        Status = CalendarObjectTypes.CheckIn + " Added";
                    }
                    break;
                case CalendarObjectTypes.Note:
                    if (IsAddingNew)
                    {
                        var N = NotesVM.CurrentEditItem;
                        success = await NotesVM.Commit();
                        if (!success) break;
                        AddNewNoteObject(N);
                        OnPropertyChanged(nameof(CalNoteObjsView));
                        Status = CalendarObjectTypes.Note + " Added";
                    }
                    else if (IsEditingItem)
                    {
                        var N = NotesVM.CurrentEditItem;
                        success = await NotesVM.Commit();
                        if (!success) break;
                        var NO = CurrentEditItem as CalendarNoteObject;
                        NO.Note = N;
                        OnPropertyChanged(nameof(CalNoteObjsView));
                        Status = CalendarObjectTypes.Note + " Modified";
                    }
                    break;
                case CalendarObjectTypes.Task:
                    if (IsAddingNew)
                    {
                        var T = TimeTasksVM.CurrentEditItem;
                        success = await TimeTasksVM.Commit();
                        if (!success) break;
                        await BuildNewTaskMap(T);
                        Status = CalendarObjectTypes.Task + " Added";
                    }
                    else if (IsEditingItem)
                    {
                        var T = TimeTasksVM.CurrentEditItem;
                        success = await TimeTasksVM.Commit();
                        if (!success) break;
                        var TO = CurrentEditItem as CalendarTaskObject;
                        TaskMaps.Remove(TO.ParentPerZone.ParentMap);
                        await BuildNewTaskMap(T);
                        Status = CalendarObjectTypes.Task + " Modified";
                    }
                    //update for checkIn editor
                    await CheckInsVM.LoadDataAsync();
                    CreateCheckInObjects();
                    break;
            }
            EndEdit();
            return success;
        }
        internal virtual async Task<bool> DeleteSelected()
        {
            bool success = false;
            switch (SelectedItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    success = await CheckInsVM.DeleteSelected();
                    if (!success) break;
                    await GetDataAsync();
                    Status = CalendarObjectTypes.CheckIn + " Deleted";
                    break;
                case CalendarObjectTypes.Note:
                    success = await NotesVM.DeleteSelected();
                    if (!success) break;
                    var N = SelectedItem as CalendarNoteObject;
                    CalNoteObjsView.Remove(N);
                    OnPropertyChanged(nameof(CalNoteObjsView));
                    Status = CalendarObjectTypes.Note + " Deleted";
                    break;
                case CalendarObjectTypes.Task:
                    success = await TimeTasksVM.DeleteSelected();
                    if (!success) break;
                    await GetDataAsync();
                    Status = CalendarObjectTypes.Task + " Deleted";
                    break;
            }
            SelectedItem = null;
            return success;
        }
        internal abstract void SaveAs();
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
            var visibleCheckIns =
                from CI in CheckInsVM.Source
                from CIm in mappedEventCheckIns
                where CI.Id != CIm.Id
                where CI.IsWithin(this)
                select CI;
            foreach (var CI in visibleCheckIns)
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
            var notes = NotesVM.Source.Where(N => N.IsWithin(this));
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
                    if (!CI.IsWithin(P)) continue;
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
                        if (!CI.IsWithin(Z)) continue;
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
                        if (CI.TimeTask.Id != M.TimeTask.Id || !CI.IsWithin(P)) continue;
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
            ClearAllocations();
            AllocateTimeFromCheckIns();
            AllocateTimeFromFilters();
            CalculateCollisions();
            AllocateEmptySpace();
            CleanUpStates();
            UnZipTaskMaps();
        }
        private void ClearAllocations()
        {
            foreach (var M in TaskMaps)
            {
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
            }
        }
        #region AllocateTimeFromCheckIns
        private void AllocateTimeFromCheckIns()
        {
            Status = "Allocating Time From CheckIns...";
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    //Create CalObjs from CheckIns
                    CalendarTaskObject pC = null;
                    CalendarCheckInObject pCI = null;
                    bool inZ = false;
                    foreach (var CI in P.CheckIns)
                    {
                        // Refer to Plans.xlsx - CheckIns
                        switch (CI.Kind)
                        {
                            case CheckInKind.InclusionZoneStart:
                                // F
                                if (pCI.Kind == CheckInKind.EventStart && inZ == false)
                                {
                                    // 4
                                    SetUnschCEndF4(pC, CI);
                                }
                                inZ = true;
                                break;
                            case CheckInKind.EventStart:
                                // B D
                                if (inZ)
                                {
                                    // B
                                    switch (pCI.Kind)
                                    {
                                        case CheckInKind.InclusionZoneStart:
                                            // 6
                                            pC = CreateCalObjB6(P, CI);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 2
                                            SetInZCEndG2(pC, CI);
                                            goto case CheckInKind.InclusionZoneStart;
                                        case CheckInKind.EventEnd:
                                            // 3
                                            goto case CheckInKind.InclusionZoneStart;
                                    }
                                }
                                else
                                {
                                    // D
                                    switch (pCI.Kind)
                                    {
                                        case CheckInKind.PerZoneStart:
                                            // 8
                                            pC = CreateCalObjD8(P, CI);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 4
                                            SetUnschCEndF4(pC, CI);
                                            goto case CheckInKind.PerZoneStart;
                                        case CheckInKind.EventEnd:
                                            // 5
                                            goto case CheckInKind.PerZoneStart;
                                        case CheckInKind.InclusionZoneEnd:
                                            goto case CheckInKind.PerZoneStart;
                                    }
                                }
                                break;
                            case CheckInKind.EventEnd:
                                // C E
                                if (inZ)
                                {
                                    // C
                                    switch (pCI.Kind)
                                    {
                                        case CheckInKind.InclusionZoneStart:
                                            // 6
                                            pC = CreateCalObjC6(P, pCI, CI);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 2
                                            SetLockedEnd(pC, CI);
                                            break;
                                        case CheckInKind.EventEnd:
                                            // 3
                                            goto case CheckInKind.InclusionZoneStart;
                                    }
                                }
                                else
                                {
                                    // E
                                    switch (pCI.Kind)
                                    {
                                        case CheckInKind.PerZoneStart:
                                            // 8
                                            pC = CreateCalObjE8(P, pCI, CI);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 4
                                            SetLockedEnd(pC, CI);
                                            break;
                                        case CheckInKind.EventEnd:
                                            // 5
                                            goto case CheckInKind.PerZoneStart;
                                        case CheckInKind.InclusionZoneEnd:
                                            // 7
                                            goto case CheckInKind.PerZoneStart;
                                    }
                                }
                                break;
                            case CheckInKind.InclusionZoneEnd:
                                // G
                                if (pCI.Kind == CheckInKind.EventStart && inZ == true)
                                {
                                    // 2
                                    SetInZCEndG2(pC, CI);
                                }
                                inZ = false;
                                break;
                            case CheckInKind.PerZoneEnd:
                                // H
                                if (pCI.Kind == CheckInKind.EventStart && inZ == false)
                                {
                                    // 4
                                    SetUnschCEndF4(pC, CI);
                                }
                                break;
                        }
                        pCI = CI;
                    }
                }
            }
        }
        private static CalendarTaskObject CreateCalObjE8(PerZone P, CalendarCheckInObject pCI, CalendarCheckInObject CI)
        {
            DateTime start = pCI.DateTime;
            if (CI.ParentPerZone.TimeConsumption != null)
            {
                var remStart = CI.DateTime - CI.ParentPerZone.TimeConsumption.RemainingAsTimeSpan;
                start = new DateTime(Max(remStart.Ticks, pCI.DateTime.Ticks));
                var diff = (CI.DateTime - start).Ticks;
                if (diff > 0)
                    CI.ParentPerZone.TimeConsumption.Remaining -= (CI.DateTime - start).Ticks;
                else
                    start = CI.DateTime;
            }
            var C = new CalendarTaskObject
            {
                Start = start,
                End = CI.DateTime,
                State = CalendarTaskObject.States.Unscheduled,
                ParentInclusionZone = CI.ParentInclusionZone,
                ParentPerZone = CI.ParentPerZone,
                EndLock = true,
                StateLock = true,
            };
            P.CalTaskObjs.Add(C);
            return C;
        }
        private static CalendarTaskObject CreateCalObjC6(PerZone P, CalendarCheckInObject pCI, CalendarCheckInObject CI)
        {
            DateTime start = pCI.DateTime;
            if (CI.ParentPerZone.TimeConsumption != null)
            {
                var remStart = CI.DateTime - CI.ParentPerZone.TimeConsumption.RemainingAsTimeSpan;
                start = new DateTime(Max(remStart.Ticks, Max(CI.ParentInclusionZone.Start.Ticks, pCI.DateTime.Ticks)));
                var diff = (CI.DateTime - start).Ticks;
                if (diff > 0)
                    CI.ParentPerZone.TimeConsumption.Remaining -= (CI.DateTime - start).Ticks;
                else
                    start = CI.DateTime;
            }
            var C = new CalendarTaskObject
            {
                Start = start,
                End = CI.DateTime,
                State = CalendarTaskObject.States.Confirmed,
                ParentInclusionZone = CI.ParentInclusionZone,
                ParentPerZone = CI.ParentPerZone,
                EndLock = true,
                StateLock = true,
            };
            P.CalTaskObjs.Add(C);
            return C;
        }
        private static CalendarTaskObject CreateCalObjD8(PerZone P, CalendarCheckInObject CI)
        {
            var C = new CalendarTaskObject
            {
                Start = CI.DateTime,
                End = CI.DateTime,
                State = CalendarTaskObject.States.Unscheduled,
                ParentInclusionZone = CI.ParentInclusionZone,
                ParentPerZone = CI.ParentPerZone,
                StartLock = true,
                StateLock = true,
            };
            P.CalTaskObjs.Add(C);
            return C;
        }
        private static CalendarTaskObject CreateCalObjB6(PerZone P, CalendarCheckInObject CI)
        {
            var C = new CalendarTaskObject
            {
                Start = CI.DateTime,
                End = CI.DateTime,
                State = CalendarTaskObject.States.Confirmed,
                ParentInclusionZone = CI.ParentInclusionZone,
                ParentPerZone = CI.ParentPerZone,
                StartLock = true,
                StateLock = true,
            };
            P.CalTaskObjs.Add(C);
            return C;
        }
        private static void SetLockedEnd(CalendarTaskObject C, CalendarCheckInObject CI)
        {
            C.End = CI.DateTime;
            C.EndLock = true;
            if (C.ParentPerZone.TimeConsumption != null)
                C.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
        }
        private static void SetUnschCEndF4(CalendarTaskObject C, CalendarCheckInObject CI)
        {
            //Assume that C.Start is locked, and C not in Z
            if (C.ParentPerZone.TimeConsumption != null)
            {
                var remEnd = C.End + C.ParentPerZone.TimeConsumption.RemainingAsTimeSpan;
                var end = new DateTime(Min(remEnd.Ticks, CI.DateTime.Ticks));
                var diff = (end - C.End).Ticks;
                if (end > C.Start)
                {
                    C.ParentPerZone.TimeConsumption.Remaining -= diff;
                    C.End = end;
                }
            }
            else
            {
                if (CI.DateTime > C.Start)
                    C.End = CI.DateTime;
            }
        }
        private static void SetInZCEndG2(CalendarTaskObject C, CalendarCheckInObject CI)
        {
            //Assume that C.End is locked, and C in Z
            if (C.ParentPerZone.TimeConsumption != null)
            {
                var remEnd = C.End + C.ParentPerZone.TimeConsumption.RemainingAsTimeSpan;
                var end = new DateTime(Min(C.ParentInclusionZone.End.Ticks, Min(remEnd.Ticks, CI.DateTime.Ticks)));
                var diff = (end - C.End).Ticks;
                if (end > C.Start)
                {
                    C.ParentPerZone.TimeConsumption.Remaining -= diff;
                    C.End = end;
                }
            }
            else
            {
                if (CI.DateTime > C.Start)
                    C.End = CI.DateTime;
            }
        }
        #endregion AllocateTimeFromCheckIns
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
                case "EvenEager":
                    EvenEagerTimeAllocate(M);
                    break;
                case "EvenCentered":
                    EvenCenteredTimeAllocate(M);
                    break;
                case "EvenApathetic":
                    EvenApatheticTimeAllocate(M);
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
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    if (P.TimeConsumption.RemainingAsTimeSpan <= Z.Duration)
                    {
                        //create cal obj the size of the remaining time
                        CalendarTaskObject CalObj = new CalendarTaskObject
                        {
                            Start = Z.Start,
                            End = Z.Start + P.TimeConsumption.RemainingAsTimeSpan,
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
                            ParentInclusionZone = Z,
                            ParentPerZone = P,
                        };
                        P.CalTaskObjs.Add(CalObj);
                        P.TimeConsumption.Remaining -= Z.Duration.Ticks;
                    }
                }
            }
        }
        private void EvenEagerTimeAllocate(CalendarTimeTaskMap M)
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
                    if (Z.Duration.Ticks < TimeTask.MinimumDuration.Ticks) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.Start + TimeTask.MinimumDuration,
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
        private void EvenCenteredTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                //Fill zones evenly, but centered
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterAsc());
                //First loop that creates CalendarObjects
                foreach (var Z in P.InclusionZones)
                {
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks < TimeTask.MinimumDuration.Ticks) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    DateTime MDT = new DateTime(Z.Start.Ticks + Z.Duration.Ticks / 2).RoundDown(TimeTask.MinimumDuration);
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = MDT,
                        End = MDT + TimeTask.MinimumDuration,
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
                        //Add some time to the left
                        if (P.TimeConsumption.Remaining <= 0) break;
                        if (Z.SeedTaskObj == null) continue;
                        if (Z.SeedTaskObj.Start - TimeTask.MinimumDuration >= Z.Start)
                        {
                            Z.SeedTaskObj.Start -= TimeTask.MinimumDuration;
                            P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                            full = false;
                        }

                        //Add some time to the right
                        if (P.TimeConsumption.Remaining <= 0) break;
                        if (Z.SeedTaskObj == null) continue;
                        if (Z.SeedTaskObj.End < Z.End)
                        {
                            Z.SeedTaskObj.End += TimeTask.MinimumDuration;
                            P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                            full = false;
                        }
                    }
                }
            }
        }
        private void EvenApatheticTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                //Fill zones evenly, but late
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterDesc());
                //First loop that creates CalendarObjects
                foreach (var Z in P.InclusionZones)
                {
                    if (P.TimeConsumption.Remaining <= 0) break;
                    if (Z.Duration.Ticks < TimeTask.MinimumDuration.Ticks) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.End - TimeTask.MinimumDuration,
                        End = Z.End,
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
                        if (Z.SeedTaskObj.Start - TimeTask.MinimumDuration < Z.Start) continue;
                        Z.SeedTaskObj.Start -= TimeTask.MinimumDuration;
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
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    if (P.TimeConsumption.RemainingAsTimeSpan <= Z.Duration)
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
            var dimensions = GetDimensions();
            foreach (var D in dimensions)
            {
                var calObjs = GetCalObjsUnOrdered(D);
                foreach (var C in calObjs)
                {
                    C.CanReDist = true;
                }
                bool hasReDists = true;
                while(hasReDists)
                {
                    hasReDists = false;
                    CollisionsStep1(D);
                    CollisionsStep2(D);
                    if (CollisionsStep3(D))
                        hasReDists = true;
                }
                CollisionsStep4(D);
            }
        }
        private void CollisionsStep1(int D)
        {
            //Step 1 - do only push collisions and split collisions where the insider wins, ignore the rest
            bool hasChanges = true;
            while (hasChanges)
            {
                var newC = CollisionsStep1Part2(D);
                if (newC != null)
                {

                    newC.ParentPerZone.CalTaskObjs.Add(newC);
                    hasChanges = true;
                }
                else hasChanges = false;
            }
        }
        private CalendarTaskObject CollisionsStep1Part2(int D)
        {
            var calObjs = GetCalObjsByPriority(D);
            //Clear tangents
            foreach (var C in calObjs)
            {
                C.Step1IgnoreFlag = false;
                C.LeftTangent = null;
                C.RightTangent = null;
            }
            //Find collisions
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                var intersections = GetIntersections(calObjs);
                foreach (var i in intersections)
                {
                    if (i.Item1.Step1IgnoreFlag || i.Item2.Step1IgnoreFlag)
                        continue;
                    if (i.Item1.IsInside(i.Item2))
                    {
                        var newC = InsideCollision(i.Item1, i.Item2);
                        if (newC != null) return newC;
                    }
                    else if (i.Item2.IsInside(i.Item1))
                    {
                        var newC = InsideCollision(i.Item2, i.Item1);
                        if (newC != null) return newC;
                    }
                    else if (Step1Push(i.Item1, i.Item2))
                    {
                        hasChanges = true;
                        break;
                    }
                }
            }
            return null;
        }
        private static CalendarTaskObject InsideCollision(CalendarTaskObject insider, CalendarTaskObject outsider)
        {
            //Refer to Plans.xlsx - Collisions
            if (insider.StartLock || insider.EndLock ||
                insider.ParentPerZone.ParentMap.TimeTask.Priority > outsider.ParentPerZone.ParentMap.TimeTask.Priority)
            {
                if (outsider.State == CalendarTaskObject.States.Unscheduled)
                {
                    if (outsider.EndLock)
                    {
                        if (outsider.StartLock)
                        {
                            return SplitOutsider(insider, outsider);
                        }
                        else
                        {
                            //shrink left
                            var diff = insider.End - outsider.Start;
                            outsider.Start = insider.End;
                            if (outsider.ParentPerZone.TimeConsumption != null)
                                outsider.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
                            return null;
                        }
                    }
                    else if (outsider.StartLock)
                    {
                        //shrink right
                        var diff = outsider.End - insider.Start;
                        outsider.End = insider.Start;
                        if (outsider.ParentPerZone.TimeConsumption != null)
                            outsider.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
                        return null;
                    }
                }
                else return SplitOutsider(insider, outsider);
            }
            return null;
        }
        private static CalendarTaskObject SplitOutsider(CalendarTaskObject insider, CalendarTaskObject outsider)
        {
            DateTime MDT = insider.Start + new TimeSpan(insider.Duration.Ticks / 2);
            var Left = new CalendarTaskObject();
            var Right = outsider;
            Left.Mimic(outsider);
            Left.EndLock = false;
            Left.End = MDT;
            Right.Start = MDT;
            Right.StartLock = false;
            if (outsider.State == CalendarTaskObject.States.Confirmed)
            {
                if (!Left.StartLock) Left.State = CalendarTaskObject.States.Unconfirmed;
                if (!Right.EndLock) Right.State = CalendarTaskObject.States.Unconfirmed;
            }
            return Left;
        }
        private static bool Step1Push(CalendarTaskObject C1, CalendarTaskObject C2)
        {
            //returns true when the collision successfully pushes, false otherwise
            var C = DetermineCollision(C1, C2);
            if (C != null)
            {
                switch (C.Result)
                {
                    case Collision.CollisionResult.PushLeft:
                        TimeSpan Lroom = C.Left.Start - C.Left.ParentInclusionZone.Start;
                        TimeSpan Lpush = new TimeSpan(Min(C.Overlap.Ticks, Min(C.LData.MaxRoom.Ticks, Lroom.Ticks)));
                        C.Left.Start -= Lpush;
                        C.Left.End -= Lpush;
                        C.Left.RightTangent = C.Right;
                        C.Right.LeftTangent = C.Left;
                        return true;
                    case Collision.CollisionResult.ShrinkLeft:
                    case Collision.CollisionResult.ReDistLeft:
                        if (C.Left.State == CalendarTaskObject.States.Unscheduled)
                        {
                            var diff = C.Left.End - C.Right.Start;
                            C.Left.End = C.Right.Start;
                            if (C.Left.ParentPerZone.TimeConsumption != null)
                                C.Left.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
                        }
                        else
                        {
                            C.Left.Step1IgnoreFlag = true;
                        }
                        return false;
                    case Collision.CollisionResult.PushRight:
                        TimeSpan Rroom = C.Right.ParentInclusionZone.End - C.Right.End;
                        TimeSpan Rpush = new TimeSpan(Min(C.Overlap.Ticks, Min(C.RData.MaxRoom.Ticks, Rroom.Ticks)));
                        C.Right.Start += Rpush;
                        C.Right.End += Rpush;
                        C.Left.RightTangent = C.Right;
                        C.Right.LeftTangent = C.Left;
                        return true;
                    case Collision.CollisionResult.ShrinkRight:
                    case Collision.CollisionResult.ReDistRight:
                        if (C2.State == CalendarTaskObject.States.Unscheduled)
                        {
                            var diff = C.Left.End - C.Right.Start;
                            C.Right.Start = C.Left.End;
                            if (C.Right.ParentPerZone.TimeConsumption != null)
                                C.Right.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
                        }
                        else
                        {
                            C.Right.Step1IgnoreFlag = true;
                        }
                        return false;
                }
            }
            return false;
        }
        private void CollisionsStep2(int D)
        {
            //Step 2 - split remaining insider collisions where insider loses
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                var newC = CollisionsStep2Part2(D);
                if (newC != null)
                {
                    newC.ParentPerZone.CalTaskObjs.Add(newC);
                    hasChanges = true;
                }
            }
        }
        private CalendarTaskObject CollisionsStep2Part2(int D)
        {
            var calObjs = GetCalObjsByPriority(D);
            var intersections = GetIntersections(calObjs);
            foreach (var i in intersections)
            {
                if (i.Item1.IsInside(i.Item2))
                {
                    var newC = SplitOutsider(i.Item1, i.Item2);
                    if (newC != null) return newC;
                }
                else if (i.Item2.IsInside(i.Item1))
                {
                    var newC = SplitOutsider(i.Item2, i.Item1);
                    if (newC != null) return newC;
                }
            }
            return null;
        }
        private bool CollisionsStep3(int D)
        {
            //Step 3 - redistribute
            //returns true if any redistributions were attempted
            bool hasReDists = false;
            var calObjs = GetCalObjsByPriority(D);
            var intersections = GetIntersections(calObjs);
            foreach (var i in intersections)
            {
                var collision = DetermineCollision(i.Item1, i.Item2);
                if (collision == null) continue;
                if (collision.Loser.CanReDist)
                {
                    hasReDists = true;
                    bool hasChanges = true;
                    while (hasChanges)
                    {
                        hasChanges = false;
                        if (collision.Overlap.Ticks <= 0) break;
                        if (CollisionsStep3Part2(D, collision, collision.Loser))
                        {
                            hasChanges = true;
                        }
                    }
                }
                if (collision.Winner.CanReDist)
                {
                    hasReDists = true;
                    bool hasChanges = true;
                    while (hasChanges)
                    {
                        hasChanges = false;
                        if (collision.Overlap.Ticks <= 0) break;
                        if (CollisionsStep3Part2(D, collision, collision.Winner))
                        {
                            hasChanges = true;
                        }
                    }
                }
            }
            return hasReDists;
        }
        private bool CollisionsStep3Part2(int D, Collision collision, CalendarTaskObject C)
        {
            //returns true when there is a redistribution
            var spaces = GetEmptySpaces(D);
            foreach (var S in spaces)
            {
                foreach (var Z in C.ParentPerZone.InclusionZones)
                {
                    if (!S.Intersects(Z)) continue;
                    var SZOverlap = S.GetOverlap(Z);
                    var shrink = new TimeSpan(Min(SZOverlap.Ticks, collision.Overlap.Ticks));
                    if (shrink.Ticks <= 0) continue;
                    //shrink in the correct direction
                    if (C == collision.Left)
                        C.End -= shrink;
                    else
                        C.Start += shrink;
                    //update the allocation
                    C.ParentPerZone.TimeConsumption.Remaining += shrink.Ticks;
                    //allocate the empty space
                    FillEmptyWithInsuffZ(S, Z);
                    //break out of the loops to update the spaces collection
                    return true;
                }
            }
            //we have exhausted all possibilities for redistributing C
            C.CanReDist = false;
            return false;
        }
        private void CollisionsStep4(int D)
        {
            //Step 4 - if any collisions still exist, these will be shrinks that cant be redistributed
            var calObjs = GetCalObjsByPriority(D);
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                var intersections = GetIntersections(calObjs);
                foreach (var i in intersections)
                {
                    if (Step4Shrink(i.Item1, i.Item2))
                    {
                        hasChanges = true;
                        break;
                    }
                }
            }
        }
        private static bool Step4Shrink(CalendarTaskObject C1, CalendarTaskObject C2)
        {
            //returns true when the collision successfully shrinks, false otherwise
            var C = DetermineCollision(C1, C2);
            if (C != null)
            {
                switch (C.Result)
                {
                    case Collision.CollisionResult.PushLeft:
                    case Collision.CollisionResult.ShrinkLeft:
                    case Collision.CollisionResult.ReDistLeft:
                        TimeSpan Lpush = new TimeSpan(Min(C.Overlap.Ticks, C.Left.Duration.Ticks));
                        C.Left.End -= Lpush;
                        break;
                    case Collision.CollisionResult.PushRight:
                    case Collision.CollisionResult.ShrinkRight:
                    case Collision.CollisionResult.ReDistRight:
                        TimeSpan Rpush = new TimeSpan(Min(C.Overlap.Ticks, C.Right.Duration.Ticks));
                        C.Right.Start += Rpush;
                        break;
                }
            }
            return false;
        }
        private static Collision DetermineCollision(CalendarTaskObject C1, CalendarTaskObject C2)
        {
            //returns false when this collision is ignored.
            //Refer to Plans.xlsx - Collisions
            //Left Right Intersection Collisions C1 ∩ C2
            CalendarTaskObject L;
            CalendarTaskObject R;
            if (C1.Start < C2.Start)
            {
                L = C1;
                R = C2;
            }
            else if (C1.Start == C2.Start)
            {
                if (C1.End <= C2.End)
                {
                    L = C1;
                    R = C2;
                }
                else
                {
                    L = C2;
                    R = C1;
                }
            }
            else
            {
                L = C2;
                R = C1;
            }
            var LData = GetLeftPushData(L);
            var RData = GetRightPushData(R);
            var collision = new Collision
            {
                LData = LData,
                RData = RData,
                Left = L,
                Right = R,
            };
            //Refer to Plans.xlsx - Redistributions
            if (R.StartLock) //L
            {
                if (L.EndLock) //7
                {
                    //conflict
                    return null;
                }
                else if (LData.HasRoom) //2
                {
                    //PushLeft
                    collision.Result = Collision.CollisionResult.PushLeft;
                    collision.cell = "L2";
                }
                else if (LData.CanReDist) //3 4
                {
                    if (LData.Priority < L.Priority) //3
                    {
                        //PushLeft
                        collision.Result = Collision.CollisionResult.PushLeft;
                        collision.cell = "L3";
                    }
                    else //4
                    {
                        //ReDistLeft
                        collision.Result = Collision.CollisionResult.ReDistLeft;
                        collision.cell = "L4";
                    }
                }
                else if (LData.Priority < L.Priority) //5
                {
                    //PushLeft
                    collision.Result = Collision.CollisionResult.PushLeft;
                    collision.cell = "L5";
                }
                else //6
                {
                    //ReDistLeft
                    collision.Result = Collision.CollisionResult.ShrinkLeft;
                    collision.cell = "L6";
                }
            }
            else if (RData.HasRoom) //B C
            {
                if (LData.Priority >= RData.Priority) //B
                {
                    //PushRight
                    collision.Result = Collision.CollisionResult.PushRight;
                    collision.cell = "B";
                }
                else //C
                {
                    if (LData.HasRoom) //2
                    {
                        //PushLeft
                        collision.Result = Collision.CollisionResult.PushLeft;
                        collision.cell = "C2";
                    }
                    else //34567
                    {
                        //PushRight
                        collision.Result = Collision.CollisionResult.PushRight;
                        collision.cell = "C34567";
                    }
                }
            }
            else if (RData.CanReDist) //D E F G
            {
                if (LData.Priority >= RData.Priority) //D E
                {
                    if (R.Priority > RData.Priority) //D
                    {
                        if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "D2";
                        }
                        else //34567
                        {
                            //PushRight
                            collision.Result = Collision.CollisionResult.PushRight;
                            collision.cell = "D34567";
                        }
                    }
                    else //E
                    {
                        if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "E2";
                        }
                        else //34567
                        {
                            //ReDistRight
                            collision.Result = Collision.CollisionResult.ReDistRight;
                            collision.cell = "E34567";
                        }
                    }
                }
                else //F G
                {
                    if (R.Priority > RData.Priority) //F
                    {
                        if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "F2";
                        }
                        else //3 4 567
                        {
                            if (LData.CanReDist) //3 4
                            {
                                if (LData.Priority < L.Priority) //3
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "F3";
                                }
                                else //4
                                {
                                    //ReDistLeft
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "F4";
                                }
                            }
                            else //567
                            {
                                //PushRight
                                collision.Result = Collision.CollisionResult.PushRight;
                                collision.cell = "F567";
                            }
                        }
                    }
                    else //G
                    {
                        if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "G2";
                        }
                        else //3 4 567
                        {
                            if (LData.CanReDist)
                            {
                                if (LData.Priority < L.Priority) //3
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "G3";
                                }
                                else //4
                                {
                                    //ReDistLeft
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "G4";
                                }
                            }
                            else //567
                            {
                                //ReDistRight
                                collision.Result = Collision.CollisionResult.ReDistRight;
                                collision.cell = "G567";
                            }
                        }
                    }
                }
            }
            else //H I J K
            {
                if (LData.Priority >= RData.Priority) //H I
                {
                    if (R.Priority > RData.Priority) //H
                    {
                        if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "D2";
                        }
                        else //3 4 567
                        {
                            if (LData.CanReDist) //3 4
                            {
                                if (LData.Priority < L.Priority) //3
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "H3";
                                }
                                else //4
                                {
                                    //ReDistLeft
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "H4";
                                }
                            }
                            else //567
                            {
                                //PushRight
                                collision.Result = Collision.CollisionResult.PushRight;
                                collision.cell = "H567";
                            }
                        }
                    }
                    else //I
                    {
                        if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "I2";
                        }
                        else //3 4 567
                        {
                            if (LData.CanReDist) //3 4
                            {
                                if (LData.Priority < L.Priority) //3
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "I3";
                                }
                                else //4
                                {
                                    //ReDistLeft
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "I4";
                                }
                            }
                            else //567
                            {
                                //PushRight
                                collision.Result = Collision.CollisionResult.ShrinkRight;
                                collision.cell = "I567";
                            }
                        }
                    }
                }
                else //J K
                {
                    if (R.Priority > RData.Priority) //J
                    {
                        if (L.EndLock) //7
                        {
                            //PushRight
                            collision.Result = Collision.CollisionResult.PushRight;
                            collision.cell = "J7";
                        }
                        else if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "J2";
                        }
                        else //3 4 5 6 
                        {
                            if (LData.CanReDist) //3 4
                            {
                                if (LData.Priority < L.Priority) //3
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "J3";
                                }
                                else //4
                                {
                                    //ReDistLeft
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "J4";
                                }
                            }
                            else //5 6
                            {
                                if (LData.Priority < L.Priority) //5
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "J5";
                                }
                                else //6
                                {
                                    //ShrinkLeft
                                    collision.Result = Collision.CollisionResult.ShrinkLeft;
                                    collision.cell = "J6";
                                }
                            }
                        }
                    }
                    else //K
                    {
                        if (L.EndLock) //7
                        {
                            //ShrinkRight
                            collision.Result = Collision.CollisionResult.ShrinkRight;
                            collision.cell = "K7";
                        }
                        else if (LData.HasRoom) //2
                        {
                            //PushLeft
                            collision.Result = Collision.CollisionResult.PushLeft;
                            collision.cell = "K2";
                        }
                        else //3 4 5 6
                        {
                            if (LData.CanReDist)
                            {
                                if (LData.Priority < L.Priority) //3
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "K3";
                                }
                                else //4
                                {
                                    //ReDistLeft
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "K4";
                                }
                            }
                            else //5 6
                            {
                                if (LData.Priority < L.Priority)
                                {
                                    //PushLeft
                                    collision.Result = Collision.CollisionResult.PushLeft;
                                    collision.cell = "K5";
                                }
                                else
                                {
                                    //ShrinkLeft
                                    collision.Result = Collision.CollisionResult.ShrinkLeft;
                                    collision.cell = "K6";
                                }
                            }
                        }
                    }
                }
            }
            return collision;
        }
        private class Collision
        {
            public enum CollisionResult { PushLeft, ShrinkLeft, ReDistLeft, PushRight, ShrinkRight, ReDistRight }
            public CollisionResult Result;
            public string cell = "";
            public PushData LData;
            public PushData RData;
            public CalendarTaskObject Left;
            public CalendarTaskObject Right;
            public TimeSpan Overlap => Left.GetOverlap(Right);
            public CalendarTaskObject Loser
            {
                get
                {
                    switch (Result)
                    {
                        case CollisionResult.PushLeft:
                        case CollisionResult.ShrinkLeft:
                        case CollisionResult.ReDistLeft:
                            return Left;
                        case CollisionResult.PushRight:
                        case CollisionResult.ShrinkRight:
                        case CollisionResult.ReDistRight:
                        default:
                            return Right;
                    }
                }
            }
            public CalendarTaskObject Winner
            {
                get
                {
                    switch (Result)
                    {
                        case CollisionResult.PushLeft:
                        case CollisionResult.ShrinkLeft:
                        case CollisionResult.ReDistLeft:
                            return Right;
                        case CollisionResult.PushRight:
                        case CollisionResult.ShrinkRight:
                        case CollisionResult.ReDistRight:
                        default:
                            return Left;
                    }
                }
            }
        }
        private struct PushData
        {
            public enum PushDirection { Left, Right }
            public PushDirection Direction;
            public bool HasRoom;
            public bool CanReDist;
            public double Priority;
            public TimeSpan MaxRoom;
            public override string ToString()
            {
                string s = String.Format("{0} {1} P[{2}] {3} {4}",
                    Direction,
                    HasRoom ? "" : "🔒",
                    Priority,
                    MaxRoom.ShortGoodString(),
                    CanReDist ? "CanReDist" : "");
                return s;
            }
        }
        private static PushData GetLeftPushData(CalendarTaskObject C)
        {
            bool hasRoom = true;
            var LT = C;
            bool canReDist = C.CanReDist;
            double LP = C.ParentPerZone.ParentMap.TimeTask.Priority;
            TimeSpan LmaxRoom = new TimeSpan();
            while (true)
            {
                //If C.HasRoom == F, LC is the nearest LT where LT.S🔒 or LT.Z.S >= LT.S or LT.LT.E🔒. 
                //LP = WC.P where WC is the LT with the lowest priority from C to LC.
                //LmaxRoom is the amount of room WC has to be pushed
                //Also if any LT CanReDist, then WC must also have CanReDist
                if (canReDist)
                {
                    if (LT.CanReDist)
                    {
                        if (LT.ParentPerZone.ParentMap.TimeTask.Priority < LP)
                        {
                            LP = LT.ParentPerZone.ParentMap.TimeTask.Priority;
                            LmaxRoom = LT.Start - (LT.ParentInclusionZone?.Start ?? LT.ParentPerZone.Start);
                        }
                    }
                }
                else
                {
                    if (LT.CanReDist)
                    {
                        canReDist = true;
                    }
                    if (LT.ParentPerZone.ParentMap.TimeTask.Priority < LP)
                    {
                        LP = LT.ParentPerZone.ParentMap.TimeTask.Priority;
                        LmaxRoom = LT.Start - (LT.ParentInclusionZone?.Start ?? LT.ParentPerZone.Start);
                    }
                }
                if (LT.EndLock ||
                    LT.StartLock ||
                    (LT.LeftTangent?.EndLock ?? false) ||
                    LT.ParentInclusionZone?.Start >= LT.Start)
                {
                    //HasRoom = F when LT.LT.E🔒 or LT.E🔒 or LT.S🔒 or LT.Z.S >= LT.S or LT.LT.HasRoom=F
                    hasRoom = false;
                    break;
                }
                else 
                if (LT.LeftTangent == null)
                {
                    //HasRoom == T when LC is the LT where LT.LT == null. LP = LC.P. WC = LC.
                    LP = LT.ParentPerZone.ParentMap.TimeTask.Priority;
                    LmaxRoom = LT.Start - (LT.ParentInclusionZone?.Start ?? LT.ParentPerZone.Start);
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
                HasRoom = hasRoom,
                Priority = LP,
                MaxRoom = LmaxRoom,
                CanReDist = canReDist,
            };
        }
        private static PushData GetRightPushData(CalendarTaskObject C)
        {
            bool hasRoom = true;
            var RT = C;
            bool canReDist = C.CanReDist;
            double RP = C.ParentPerZone.ParentMap.TimeTask.Priority;
            TimeSpan RmaxRoom = new TimeSpan();
            while (true)
            {
                if (canReDist)
                {
                    if (RT.CanReDist)
                    {
                        if (RT.ParentPerZone.ParentMap.TimeTask.Priority < RP)
                        {
                            RP = RT.ParentPerZone.ParentMap.TimeTask.Priority;
                            RmaxRoom = (RT.ParentInclusionZone?.End ?? RT.ParentPerZone.End) - RT.End;
                        }
                    }
                }
                else
                {
                    if (RT.CanReDist)
                    {
                        canReDist = true;
                    }
                    if (RT.ParentPerZone.ParentMap.TimeTask.Priority < RP)
                    {
                        RP = RT.ParentPerZone.ParentMap.TimeTask.Priority;
                        RmaxRoom = (RT.ParentInclusionZone?.End ?? RT.ParentPerZone.End) - RT.End;
                    }
                }
                if (RT.ParentPerZone.ParentMap.TimeTask.Priority < RP)
                {
                    RP = RT.ParentPerZone.ParentMap.TimeTask.Priority;
                    RmaxRoom = (RT.ParentInclusionZone?.End ?? RT.ParentPerZone.End) - RT.End;
                }
                if (RT.EndLock ||
                    RT.StartLock ||
                    (RT.RightTangent?.StartLock ?? false) ||
                    RT.ParentInclusionZone?.End <= RT.End)
                {
                    hasRoom = false;
                    break;
                }
                else 
                if (RT.RightTangent == null)
                {
                    RP = RT.ParentPerZone.ParentMap.TimeTask.Priority;
                    RmaxRoom = (RT.ParentInclusionZone?.End ?? RT.ParentPerZone.End) - RT.End;
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
                HasRoom = hasRoom,
                Priority = RP,
                MaxRoom = RmaxRoom,
                CanReDist = canReDist,
            };
        }
        private static List<Tuple<CalendarTaskObject, CalendarTaskObject>> GetIntersections(List<CalendarTaskObject> calObjs)
        {
            //this method should be much faster than LINQ and avoids duplicates
            var intersections = new List<Tuple<CalendarTaskObject, CalendarTaskObject>>();
            for (int a = 0; a < calObjs.Count; a++)
                for (int b = a + 1; b < calObjs.Count; b++)
                    if (calObjs[a].Intersects(calObjs[b]))
                        intersections.Add(new Tuple<CalendarTaskObject, CalendarTaskObject>(calObjs[a], calObjs[b]));
            return intersections;
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
                var dimensions = GetDimensions();
                foreach (var D in dimensions)
                {
                    var spaces = GetEmptySpaces(D);
                    foreach (var S in spaces)
                    {
                        var zones =
                            from M in TaskMaps
                            where M.TimeTask.Dimension == D
                            from P in M.PerZones
                            from Z in P.InclusionZones
                            where Z.Intersects(S)
                            select Z;
                        var insuffZones =
                            from Z in zones
                            where Z.ParentPerZone.TimeConsumption?.Remaining > 0
                            orderby Z.ParentPerZone.ParentMap.TimeTask.Priority descending
                            select Z;
                        var insuffZone = insuffZones.FirstOrDefault();
                        if (insuffZone != null)
                        {
                            FillEmptyWithInsuffZ(S, insuffZone);
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
                                FillEmptyWithFiller(S, fillZone);
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
        private List<EmptyZone> GetEmptySpaces(int dimension)
        {
            var spaces = new List<EmptyZone>();
            var calObjs = GetCalObjsByStart(dimension);
            //Find earliest Per
            var start =
                (from M in TaskMaps
                 where M.TimeTask.Dimension == dimension
                 from P in M.PerZones
                 select P).Min(P => P.Start);
            //Find latest Per
            var end =
                (from M in TaskMaps
                 where M.TimeTask.Dimension == dimension
                 from P in M.PerZones
                 select P).Max(P => P.End);
            DateTime dt = start;
            CalendarTaskObject prev = null;
            foreach (var C in calObjs)
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
                if (dt < C.End)
                {
                    dt = C.End;
                    prev = C;
                }
            }
            //check for trailing space after last CalObj
            if (dt < end)
            {
                var Z = new EmptyZone
                {
                    Start = dt,
                    End = end,
                    Left = prev,
                    Right = null
                };
                spaces.Add(Z);
            }
            return spaces;
        }
        #endregion AllocateEmptySpace
        #region CleanUpStates
        private void CleanUpStates()
        {
            Status = "Cleaning Up...";
            MergeDumbSplits();
            FixMisalignments();
            RemoveCancels();
            FlagAutoConfirms();
            FlagInsufficients();
            FlagConflicts();
        }
        private void MergeDumbSplits()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    bool hasChanges = true;
                    while (hasChanges)
                    {
                        var kill = MergeDumbSplitsPart2(P);
                        if (kill != null)
                        {
                            kill.ParentPerZone.CalTaskObjs.Remove(kill);
                            hasChanges = true;
                        }
                        else hasChanges = false;
                    }
                }
            }
        }
        private static CalendarTaskObject MergeDumbSplitsPart2(PerZone P)
        {
            foreach (var C1 in P.CalTaskObjs)
            {
                foreach (var C2 in P.CalTaskObjs)
                {
                    if (C1 == C2) continue;
                    if (C1.ParentInclusionZone == C2.ParentInclusionZone &&
                        C1.End == C2.Start &&
                        !C1.EndLock &&
                        !C2.StartLock)
                    {
                        //Merge the split
                        C1.End = C2.End;
                        C1.EndLock = C2.EndLock;
                        C1.RightTangent = C2.RightTangent;
                        return C2;
                    }
                }
            }
            return null;
        }
        private void FlagInsufficients()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    //Flag Insufficients
                    if (P.TimeConsumption?.Remaining > 0)
                        foreach (var C in P.CalTaskObjs)
                            C.State = CalendarTaskObject.States.Insufficient;
                }
            }
        }
        private void FlagAutoConfirms()
        {
            foreach (var M in TaskMaps)
            {
                if (!M.TimeTask.AutoCheckIn) continue;
                foreach (var P in M.PerZones)
                {
                    foreach (var C in P.CalTaskObjs)
                    {
                        if (!C.StartLock && !C.EndLock &&
                            C.ParentInclusionZone != null)
                        {
                            C.State = CalendarTaskObject.States.AutoConfirm;
                        }
                    }
                }
            }
        }
        private void RemoveCancels()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    var cancels = new HashSet<CalendarTaskObject>();
                    foreach (var C in P.CalTaskObjs)
                    {
                        //Flag Cancels
                        if (C.Start == C.End)
                        {
                            cancels.Add(C);
                            continue;
                        }
                    }
                    //Delete Cancels
                    foreach (var C in cancels)
                        P.CalTaskObjs.Remove(C);
                }
            }
        }
        private void FixMisalignments()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
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
                                if (C.Intersects(Z) && (!C.IsWithin(Z) || C.ParentInclusionZone != Z))
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
            }
        }
        private void FlagConflicts()
        {
            //If there are still collisions, mark them as Conflict. 
            var dimensions = GetDimensions();
            foreach (var D in dimensions)
            {
                var calObjs = GetCalObjsByStart(D);
                var intersections = GetIntersections(calObjs);
                foreach (var i in intersections)
                {
                    i.Item1.State = CalendarTaskObject.States.Conflict;
                    i.Item2.State = CalendarTaskObject.States.Conflict;
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
        protected virtual void AdditionalCalTaskObjSetup(CalendarTaskObject CalObj) { }
        private IEnumerable<int> GetDimensions()
        {
            return (from M in TaskMaps
                    select M.TimeTask.Dimension).Distinct();
        }
        private HashSet<CalendarTaskObject> GetCalObjsUnOrdered(int dimension)
        {
            return new HashSet<CalendarTaskObject>(
                from M in TaskMaps
                where M.TimeTask.Dimension == dimension
                from P in M.PerZones
                from C in P.CalTaskObjs
                select C);
        }
        private List<CalendarTaskObject> GetCalObjsByPriority(int dimension)
        {
            return new List<CalendarTaskObject>(
                from M in TaskMaps
                where M.TimeTask.Dimension == dimension
                from P in M.PerZones
                from C in P.CalTaskObjs
                orderby C.ParentPerZone.ParentMap.TimeTask.Priority descending, C.Start, C.End
                select C);
        }
        private List<CalendarTaskObject> GetCalObjsByStart(int dimension)
        {
            return new List<CalendarTaskObject>(
                from M in TaskMaps
                where M.TimeTask.Dimension == dimension
                from P in M.PerZones
                from C in P.CalTaskObjs
                orderby C.Start, C.End
                select C);
        }
        #endregion MapTaskObjects
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
