﻿// Copyright 2017 (C) Cody Neuburger  All rights reserved.
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

    public abstract class CalendarViewModel : EditableObject, IView, IDisposable, IZone
    {
        public CalendarViewModel()
        {
            Start = DateTime.Now;
            _TimeTasksVM.Parent = this;
            _TimeTasksVM.Managed = true;
            _CheckInsVM.Parent = this;
            _CheckInsVM.Managed = true;
            _NotesVM.Parent = this;
            _NotesVM.Managed = true;
        }
        #region Interface
        public abstract string Name { get; }
        private String _status = "Ready";
        public String Status
        {
            get { return _status; }
            protected set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        private DateTime _MousePosition;
        public DateTime MousePosition
        {
            get { return _MousePosition; }
            set
            {
                _MousePosition = value;
                OnPropertyChanged();
            }
        }
        private DateTime _MouseClickPosition;
        public DateTime MouseClickPosition
        {
            get { return _MouseClickPosition; }
            set
            {
                _MouseClickPosition = value;
                OnPropertyChanged();
            }
        }
        private int _ExtentFactor = 1;
        /// <summary>
        /// Sets the max number of recursions of FindPerSet(). 
        /// 3 is default. 1 loads pers only in the current view. 0 for infinite. 
        /// This can greatly affect performance.
        /// </summary>
        public virtual int ExtentFactor
        {
            get { return _ExtentFactor; }
            set
            {
                _ExtentFactor = value;
                OnPropertyChanged();
            }
        }
        private bool _Max = true;
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
        private bool _ShowTimeTextMargin = true;
        public virtual bool ShowTimeTextMargin
        {
            get { return _ShowTimeTextMargin; }
            set
            {
                if (_ShowTimeTextMargin == value) return;
                _ShowTimeTextMargin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanShowTimeTextMargin));
            }
        }
        private bool _IsEnabled = true;
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
        public virtual bool CanMax => true;
        public virtual bool CanShowTimeTextMargin => true;
        #endregion
        #region Memento
        private class VMMemento : IMemento
        {
            private readonly CalendarViewModel Originator;
            private readonly IMemento TaskMemo;
            private readonly IMemento NoteMemo;
            private readonly IMemento CheckInMemo;
            public VMMemento(CalendarViewModel originator)
            {
                Originator = originator;
                TaskMemo = originator.TimeTasksVM.State;
                NoteMemo = originator.NotesVM.State;
                CheckInMemo = originator.CheckInsVM.State;
            }
            public void RestoreState()
            {
                TaskMemo.RestoreState();
                NoteMemo.RestoreState();
                CheckInMemo.RestoreState();
            }
        }
        public override IMemento State => new VMMemento(this);
        private Stack<IMemento> UndoStack = new Stack<IMemento>();
        private Stack<IMemento> RedoStack = new Stack<IMemento>();
        private Stack<IMemento> TempStack = new Stack<IMemento>();
        private ICommand _UndoCommand;
        private ICommand _RedoCommand;
        public ICommand UndoCommand => _UndoCommand
            ?? (_UndoCommand = new RelayCommand(async ap => await Undo(), pp => CanUndo));
        public ICommand RedoCommand => _RedoCommand
            ?? (_RedoCommand = new RelayCommand(async ap => await Redo(), pp => CanRedo));
        protected bool CanUndo => IsReady && UndoStack.Count > 0;
        protected bool CanRedo => IsReady && RedoStack.Count > 0;
        //Disables change tracking when true.
        public bool Managed { get; set; } = false;
        //Add this function to the beginning of commands to enable change tracking.
        protected void NewChange()
        {
            if (Managed) return;
            UndoStack.Push(State);
            TempStack = RedoStack;
            RedoStack = new Stack<IMemento>();
        }
        protected void ClearUndos()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            TempStack.Clear();
        }
        protected async Task Undo()
        {
            //save current state to RedoStack
            RedoStack.Push(State);
            //get previous state from UndoStack
            var prevState = UndoStack.Pop();
            prevState.RestoreState();
            await ReCreateAsync();
        }
        protected async Task Redo()
        {
            //save current state to UndoStack
            UndoStack.Push(State);
            //get next state from RedoStack
            var nextState = RedoStack.Pop();
            nextState.RestoreState();
            await ReCreateAsync();
        }
        #endregion
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
            ClearUndos();
            await ReCreateAsync();
        }
        protected virtual async Task NextAsync()
        {
            ClearUndos();
            await ReCreateAsync();
        }
        #endregion Navigate
        #region Orientation
        private Orientation _TimeOrientation = Orientation.Vertical;
        public virtual Orientation TimeOrientation
        {
            get { return _TimeOrientation; }
            set
            {
                if (_TimeOrientation == value) return;
                _TimeOrientation = value;
                OnPropertyChanged();
            }
        }
        private ICommand _TimeOrientationCommand;
        public ICommand TimeOrientationCommand => _TimeOrientationCommand
            ?? (_TimeOrientationCommand = new RelayCommand(ap => ToggleTimeOrientation(), pp => CanTimeOrientation));
        protected virtual bool CanTimeOrientation => true;
        protected virtual void ToggleTimeOrientation()
        {
            if (TimeOrientation == Orientation.Horizontal)
                TimeOrientation = Orientation.Vertical;
            else
                TimeOrientation = Orientation.Horizontal;
        }
        private Orientation _DateOrientation = Orientation.Horizontal;
        public virtual Orientation DateOrientation
        {
            get { return _DateOrientation; }
            set
            {
                if (_DateOrientation == value) return;
                _DateOrientation = value;
                OnPropertyChanged();
            }
        }
        private ICommand _DateOrientationCommand;
        public ICommand DateOrientationCommand => _DateOrientationCommand
            ?? (_DateOrientationCommand = new RelayCommand(ap => ToggleDateOrientation(), pp => CanDateOrientation));
        protected virtual bool CanDateOrientation => true;
        protected virtual void ToggleDateOrientation()
        {
            if (DateOrientation == Orientation.Horizontal)
                DateOrientation = Orientation.Vertical;
            else
                DateOrientation = Orientation.Horizontal;
        }
        #endregion
        #region Scale
        private int _ScaleSudoCommand;
        private ICommand _ScaleUpCommand;
        private ICommand _ScaleDownCommand;
        /// <summary>
        /// This is used to send a message to the CalendarView panel via databinds to trigger the panel's Scale commands.
        /// </summary>
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
        public string MonthString => Start.ToString("MMMM yyy");
        public string WeekString => Start.ToString("MMMM dd, yyy");
        #endregion Zone
        #region CRUDS
        private TimeTasksViewModel _TimeTasksVM = new TimeTasksViewModel();
        private CheckInsViewModel _CheckInsVM = new CheckInsViewModel();
        private NotesViewModel _NotesVM = new NotesViewModel();
        private CalendarObject _SelectedItem;
        private CalendarObject _CurrentEditItem;
        private CalendarObjectTypes _SelectedItemType;
        private CalendarObjectTypes _CurrentEditItemType;
        private string _SelectionString = "";
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
        private ICommand _SelectCommand;
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
        public ObservableCollection<CalendarObject> ItemsUnderMouse { get; set; } = new ObservableCollection<CalendarObject>();
        public CalendarObject SelectedItem
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
                if (_SelectedItem == null)
                {
                    HasSelected = false;
                    SelectionString = "";
                }
                else
                {
                    HasSelected = true;
                    SelectionString = _SelectedItem.BasicString;
                    Status = "Selected " + SelectedItemType;
                }
                OnPropertyChanged();
            }
        }
        public CalendarObject CurrentEditItem
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
        public string SelectionString
        {
            get { return _SelectionString; }
            protected set
            {
                _SelectionString = value;
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
            ?? (_CancelCommand = new RelayCommand(async ap => await Cancel(), pp => CanCancel));
        public ICommand CommitCommand => _CommitCommand
            ?? (_CommitCommand = new RelayCommand(async ap => await Commit(), pp => CanCommit));
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(async ap => await LoadData(), pp => CanGetData));
        public ICommand NewItemCommand => _NewItemCommand
            ?? (_NewItemCommand = new RelayCommand(ap => AddNew((CalendarObjectTypes)ap), pp =>
            pp is CalendarObjectTypes && CanAddNew((CalendarObjectTypes)pp)));
        public ICommand EditSelectedCommand => _EditSelectedCommand
            ?? (_EditSelectedCommand = new RelayCommand(async ap => await EditSelected(), pp => CanEditSelected));
        public ICommand DeleteSelectedCommand => _DeleteSelectedCommand
            ?? (_DeleteSelectedCommand = new RelayCommand(async ap => await DeleteSelected(), pp => CanDeleteSelected));
        public ICommand SelectCommand => _SelectCommand
            ?? (_SelectCommand = new RelayCommand(ap => Select((CalendarObject)ap), pp => CanSelect(pp)));
        public ICommand SaveAsCommand => _SaveAsCommand
            ?? (_SaveAsCommand = new RelayCommand(ap => SaveAs(), pp => CanSave));
        public ICommand NewNoteCommand => _NewNoteCommand
            ?? (_NewNoteCommand = new RelayCommand(ap => NewNote(ap), pp => CanAddNewNote(pp)));
        public ICommand NewCheckInCommand => _NewCheckInCommand
            ?? (_NewCheckInCommand = new RelayCommand(ap => NewCheckIn(ap), pp => CanAddNewCheckIn(pp)));
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
        protected virtual bool CanSelect(object pp)
        {
            return pp is UIElement;
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
        protected virtual bool CanAddNewNote(object ap)
        {
            return IsReady
                && (NotesVM?.NewItemCommand?.CanExecute(ap) ?? false);
        }
        protected virtual bool CanAddNewCheckIn(object ap)
        {
            return IsReady
                && (CheckInsVM?.NewItemCommand?.CanExecute(ap) ?? false);
        }
        protected virtual bool CanAddNewTimeTask => IsReady && (TimeTasksVM?.NewItemCommand?.CanExecute(null) ?? false);
        internal virtual async Task LoadData()
        {
            ClearUndos();
            IsEnabled = false;
            IsLoading = true;
            await Cancel();
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
        private async Task ReCreateAsync()
        {
            IsLoading = true;
            await CreateCalendarObjects();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        private async Task ReGetAsync()
        {
            IsLoading = true;
            await GetDataAsync();
            IsLoading = false;
            CommandManager.InvalidateRequerySuggested();
        }
        internal virtual void AddNew(CalendarObjectTypes type)
        {
            NewChange();
            switch (type)
            {
                case CalendarObjectTypes.CheckIn:
                    NewCheckIn(DateTime.Now);
                    break;
                case CalendarObjectTypes.Note:
                    NewNote(DateTime.Now);
                    break;
                case CalendarObjectTypes.Task:
                    NewTimeTask();
                    break;
            }
        }
        internal virtual void NewCheckIn(object ap)
        {
            NewChange();
            CurrentEditItemType = CalendarObjectTypes.CheckIn;
            CheckInsVM.NewItemCommand.Execute(ap);
            Status = "Adding new CheckIn";
            SelectedItem = null;
            IsAddingNew = true;
        }
        internal virtual void NewNote(object ap)
        {
            NewChange();
            CurrentEditItemType = CalendarObjectTypes.Note;
            NotesVM.NewItemCommand.Execute(ap);
            Status = "Adding new Note";
            SelectedItem = null;
            IsAddingNew = true;
        }
        internal virtual void NewTimeTask()
        {
            NewChange();
            CurrentEditItemType = CalendarObjectTypes.Task;
            TimeTasksVM.NewItemCommand.Execute(null);
            Status = "Adding new Task";
            SelectedItem = null;
            IsAddingNew = true;
        }
        internal virtual async Task EditSelected()
        {
            NewChange();
            CurrentEditItem = SelectedItem;
            SelectedItem = null;
            IsEditingItem = true;
            switch (SelectedItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    await Cancel();
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
        protected virtual void FinishEdit()
        {
            IsEditingItem = false;
            IsAddingNew = false;
            CurrentEditItem = null;
            SelectedItem = null;
            //Refresh all of the buttons
            CommandManager.InvalidateRequerySuggested();
        }
        internal virtual async Task Cancel()
        {
            await CheckInsVM?.Cancel();
            await NotesVM?.Cancel();
            await TimeTasksVM?.Cancel();
            Status = "Canceled";
            FinishEdit();
            if (CanUndo) await Undo();
            RedoStack = TempStack;
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
            FinishEdit();
            return success;
        }
        internal virtual async Task<bool> DeleteSelected()
        {
            NewChange();
            bool success = false;
            switch (SelectedItemType)
            {
                case CalendarObjectTypes.CheckIn:
                    success = await CheckInsVM.DeleteSelected();
                    if (!success) break;
                    await ReGetAsync();
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
                    await ReGetAsync();
                    Status = CalendarObjectTypes.Task + " Deleted";
                    break;
            }
            SelectedItem = null;
            return success;
        }
        internal virtual void Select(CalendarObject item)
        {
            SelectedItem = item;
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
                where CIO.Kind == CheckInKind.EventEnd || CIO.Kind == CheckInKind.EventStart || CIO.Kind == CheckInKind.Cancel
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
            {
                var CCI = new CalendarCheckInObject
                {
                    CheckIn = CI,
                    DimensionCount = DimensionCount,
                };
                switch (CI.Text)
                {
                    case "Start":
                        CCI.Kind = CheckInKind.EventStart;
                        break;
                    case "End":
                        CCI.Kind = CheckInKind.EventEnd;
                        break;
                    case "Cancel":
                        CCI.Kind = CheckInKind.Cancel;
                        break;
                    default:
                        break;
                }
                AddNewCheckInObject(CCI);
            }
            foreach (var CIO in mappedEventCheckInObjects)
                AddNewCheckInObject(CIO);
            OnPropertyChanged(nameof(CalCIObjsView));
        }
        private void AddNewCheckInObject(CalendarCheckInObject CIO)
        {
            CIO.DimensionCount = DimensionCount;
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
                DimensionCount = DimensionCount,
            };
            CalNoteObjsView.AddNewItem(NO);
            CalNoteObjsView.CommitNew();
        }
        #endregion CRUDS
        #region TaskMaps
        // See: Plans.xlsx - Calendar Passes, States, CheckIns, Collisions, Collisions2
        public HashSet<CalendarTimeTaskMap> TaskMaps;
        public int DimensionCount { get; private set; } = 1;
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
            var maps = new HashSet<CalendarTimeTaskMap>();
            foreach (var T in RelevantTasks)
            {
                //Create Maps with all PerZones; we will reduce this set later.
                await T.BuildPerZonesAsync(Start, End);
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
                maps.Add(M);
            }
            var RelevantPerZones = FindPerSet(maps, new HashSet<PerZone>(), Start, End, ExtentFactor);
            foreach (var M in maps)
            {
                //Reduce the PerZone set.
                M.PerZones = new HashSet<PerZone>(M.PerZones.Intersect(RelevantPerZones));

            }
            //Select the maps that have relevant PerZones and build them.
            foreach (var M in maps.Where(M => M.PerZones.Count > 0))
            {
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
                TaskMaps.Add(M);
            }
        }
        private IEnumerable<TimeTask> FindTaskSet(IEnumerable<TimeTask> tasks, 
            IEnumerable<TimeTask> accumulatedFinds, DateTime start, DateTime end)
        {
            //Recursively select the set of Tasks that intersect the calendar view or previously added tasks.
            var foundTasks = (from T in tasks
                              where T.Intersects(start, end)
                              select T).Except(accumulatedFinds);
            if (foundTasks.Count() == 0) return accumulatedFinds;
            accumulatedFinds = new HashSet<TimeTask>(accumulatedFinds
                .Union(foundTasks));
            foreach (var T in foundTasks)
            {
                accumulatedFinds = new HashSet<TimeTask>(accumulatedFinds
                    .Union(FindTaskSet(tasks, accumulatedFinds, T.Start, T.End)));
            }
            return accumulatedFinds;
        }
        private IEnumerable<PerZone> FindPerSet(IEnumerable<CalendarTimeTaskMap> maps, 
            IEnumerable<PerZone> accumulatedFinds, DateTime start, DateTime end, int extentFactor)
        {
            //Recursively select the set of PerZones that intersect the calendar view 
            //or PerZones that are marked critical and intersect any previously added PerZones.

            //Find the PerZones that are within the given range.
            var foundPers = (from M in maps
                             from P in M.PerZones
                             where P.Intersects(start, end)
                             select P).Except(accumulatedFinds);
            //Stop recursion if there are no more new PerZones.
            if (foundPers.Count() == 0) return accumulatedFinds;
            accumulatedFinds = new HashSet<PerZone>(accumulatedFinds
                .Union(foundPers));

            //Stop recursion based on the ExtentFactor. 0 for infinite.
            if (--extentFactor == 0) return accumulatedFinds;

            //Find additional PerZones intersecting the found PerZones.
            foreach (var P in foundPers)
            {
                accumulatedFinds = new HashSet<PerZone>(accumulatedFinds
                    .Union(FindPerSet(maps, accumulatedFinds, P.Start, P.End, extentFactor)));
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
                        ParentPerZone = P,
                    };
                    switch (CI.Text)
                    {
                        case "Start":
                            CCI.Kind = CheckInKind.EventStart;
                            break;
                        case "End":
                            CCI.Kind = CheckInKind.EventEnd;
                            break;
                        case "Cancel":
                            CCI.Kind = CheckInKind.Cancel;
                            break;
                        default:
                            break;
                    }
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
                        var CCI = new CalendarCheckInObject
                        {
                            CheckIn = CI,
                            ParentPerZone = P,
                        };
                        switch (CI.Text)
                        {
                            case "Start":
                                CCI.Kind = CheckInKind.EventStart;
                                break;
                            case "End":
                                CCI.Kind = CheckInKind.EventEnd;
                                break;
                            case "Cancel":
                                CCI.Kind = CheckInKind.Cancel;
                                break;
                            default:
                                break;
                        }
                        eventCheckIns.Add(CCI);
                    }
                    //map zone ends as CheckIns
                    perCheckIns.Add(new CalendarCheckInObject
                    {
                        CheckIn = new CheckIn
                        {
                            Text = "Start",
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
                            Text = "End",
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
                            if (!CI.CheckIn.IsWithin(Z)) continue;
                            CI.ParentInclusionZone = Z;
                        }
                        //map zone ends as CheckIns
                        inZoneCheckIns.Add(new CalendarCheckInObject
                        {
                            CheckIn = new CheckIn
                            {
                                Text = "Start",
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
                                Text = "End",
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
                    InclusionZone Z = null;
                    foreach (var CI in P.CheckIns)
                    {
                        // Refer to Plans.xlsx - CheckIns
                        switch (CI.Kind)
                        {
                            case CheckInKind.Cancel:
                                if (Z != null)
                                {
                                    P.InclusionZones.Remove(Z);
                                    Z.Cancelled = true;
                                }
                                break;
                            case CheckInKind.InclusionZoneStart:
                                // F
                                if (pCI.Kind == CheckInKind.EventStart && Z == null)
                                {
                                    // 4
                                    SetEnd(pC, CI);
                                }
                                Z = CI.ParentInclusionZone;
                                break;
                            case CheckInKind.EventStart:
                                // B D
                                if (Z != null)
                                {
                                    // B
                                    switch (pCI.Kind)
                                    {
                                        case CheckInKind.InclusionZoneStart:
                                            // 6
                                            pC = CreateStartingCalObj(P, CI, CalendarTaskObject.States.Confirmed);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 2
                                            SetEnd(pC, CI);
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
                                            pC = CreateStartingCalObj(P, CI, CalendarTaskObject.States.Unscheduled);
                                            break;
                                        case CheckInKind.EventStart:
                                            // 4
                                            SetEnd(pC, CI);
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
                                if (Z != null)
                                {
                                    // C
                                    switch (pCI.Kind)
                                    {
                                        case CheckInKind.InclusionZoneStart:
                                            // 6
                                            pC = CreateEndingCalObj(P, pCI, CI, CalendarTaskObject.States.Confirmed);
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
                                            pC = CreateEndingCalObj(P, pCI, CI, CalendarTaskObject.States.Unscheduled);
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
                                if (pCI.Kind == CheckInKind.EventStart && Z != null)
                                {
                                    // 2
                                    SetEnd(pC, CI);
                                }
                                Z = null;
                                break;
                            case CheckInKind.PerZoneEnd:
                                // H
                                if (pCI.Kind == CheckInKind.EventStart && Z == null)
                                {
                                    // 4
                                    SetEnd(pC, CI);
                                }
                                break;
                        }
                        pCI = CI;
                    }
                }
            }
        }
        private static CalendarTaskObject CreateEndingCalObj(PerZone P, CalendarCheckInObject pCI, CalendarCheckInObject CI, CalendarTaskObject.States state)
        {
            var minAlloc = new TimeSpan(Max(
                TimeTask.MinimumDuration.Ticks,
                P.ParentMap.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
            var C = new CalendarTaskObject
            {
                Start = new DateTime((CI.DateTime - minAlloc).Ticks.Within(P.Start.Ticks, pCI.DateTime.Ticks)),
                End = CI.DateTime,
                State = state,
                ParentInclusionZone = CI.ParentInclusionZone,
                ParentPerZone = CI.ParentPerZone,
                EndLock = true,
                StateLock = true,
            };
            if (C.ParentPerZone.TimeConsumption != null)
                C.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
            P.CalTaskObjs.Add(C);
            return C;
        }
        private static CalendarTaskObject CreateStartingCalObj(PerZone P, CalendarCheckInObject CI, CalendarTaskObject.States state)
        {
            var minAlloc = new TimeSpan(Max(
                TimeTask.MinimumDuration.Ticks,
                P.ParentMap.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
            var C = new CalendarTaskObject
            {
                Start = CI.DateTime,
                End = new DateTime(Min((CI.DateTime + minAlloc).Ticks, P.End.Ticks)),
                State = state,
                ParentInclusionZone = CI.ParentInclusionZone,
                ParentPerZone = CI.ParentPerZone,
                StartLock = true,
                StateLock = true,
            };
            if (C.ParentPerZone.TimeConsumption != null)
                C.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
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
        private static void SetEnd(CalendarTaskObject pC, CalendarCheckInObject CI)
        {
            var minAlloc = new TimeSpan(Max(
                TimeTask.MinimumDuration.Ticks,
                pC.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
            pC.End = new DateTime(Min(CI.DateTime.Ticks, pC.Start.Ticks + minAlloc.Ticks));
        }
        #endregion AllocateTimeFromCheckIns
        #region AllocateTimeFromFilters
        private void AllocateTimeFromFilters()
        {
            Status = "Allocating Time From Filters...";
            foreach (var M in TaskMaps)
            {
                if (M.TimeTask.TimeAllocation == null) continue;
                switch (M.TimeTask.TimeAllocation.Method)
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
        }
        private void EagerTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                // Fill the earliest zones first
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterAsc());
                var minAlloc = new TimeSpan(Max(
                    TimeTask.MinimumDuration.Ticks,
                    M.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
                foreach (var Z in P.InclusionZones)
                {
                    if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                    if (Z.Duration < minAlloc) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    var alloc = new TimeSpan(Z.Duration.Ticks
                        .Within(minAlloc.Ticks, P.TimeConsumption.Remaining))
                        .RoundUp(TimeTask.MinimumDuration);
                    var calObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.Start + alloc,
                        ParentInclusionZone = Z,
                        ParentPerZone = P,
                    };
                    P.CalTaskObjs.Add(calObj);
                    P.TimeConsumption.Remaining -= calObj.Duration.Ticks;
                    Z.SeedTaskObj = calObj;
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
                var minAlloc = new TimeSpan(Max(
                    TimeTask.MinimumDuration.Ticks,
                    M.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
                //First loop that creates CalendarObjects
                foreach (var Z in P.InclusionZones)
                {
                    if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                    if (Z.Duration < minAlloc) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    CalendarTaskObject calObj = new CalendarTaskObject
                    {
                        Start = Z.Start,
                        End = Z.Start + minAlloc,
                        ParentInclusionZone = Z,
                        ParentPerZone = P,
                    };
                    P.CalTaskObjs.Add(calObj);
                    P.TimeConsumption.Remaining -= minAlloc.Ticks;
                    Z.SeedTaskObj = calObj;
                }
                //Second loop that adds more time to CalendarObjects
                minAlloc = TimeTask.MinimumDuration;
                bool full = false;
                while (!full && P.TimeConsumption.CanAllocate(minAlloc.Ticks))
                {
                    full = true;
                    //add time to each CalObj until they are full or out of allocated time
                    foreach (var Z in P.InclusionZones)
                    {
                        if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                        if (Z.SeedTaskObj == null) continue;
                        if (Z.SeedTaskObj.End + minAlloc >= Z.End) continue;
                        Z.SeedTaskObj.End = (Z.SeedTaskObj.End + minAlloc);
                        P.TimeConsumption.Remaining -= minAlloc.Ticks;
                        full = false;
                    }
                }
                //align the objs to calendar chunks
                foreach (var C in P.CalTaskObjs)
                {
                    var dur = C.Duration;
                    C.End = C.End.RoundUp(TimeTask.MinimumDuration);
                    var diff = C.Duration - dur;
                    P.TimeConsumption.Remaining -= diff.Ticks;
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
                EvenCenteredInitialize(P);
                EvenCenteredExpand(P);
            }
        }
        private static void EvenCenteredInitialize(PerZone P)
        {
            var minAlloc = new TimeSpan(Max(
                TimeTask.MinimumDuration.Ticks,
                P.ParentMap.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
            foreach (var Z in P.InclusionZones)
            {
                if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                if (Z.Duration < minAlloc) continue;
                bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                if (hasCalObj) continue;
                DateTime MDT = new DateTime(Z.Start.Ticks + (Z.Duration.Ticks - minAlloc.Ticks) / 2).RoundDown(TimeTask.MinimumDuration);
                CalendarTaskObject CalObj = new CalendarTaskObject
                {
                    Start = MDT,
                    End = MDT + minAlloc,
                    ParentInclusionZone = Z,
                    ParentPerZone = P,
                };
                P.CalTaskObjs.Add(CalObj);
                P.TimeConsumption.Remaining -= minAlloc.Ticks;
                Z.SeedTaskObj = CalObj;
            }
        }
        private static void EvenCenteredExpand(PerZone P)
        {
            foreach (var Z in P.InclusionZones)
                Z.EvCenAllocFlag = false;
            bool full = false;
            while (!full && (P.TimeConsumption.Remaining > 0))
            {
                full = true;
                //add time to each CalObj until they are full or out of allocated time
                foreach (var Z in P.InclusionZones)
                {
                    bool allocated = false;
                    if (!P.TimeConsumption.CanAllocate(TimeTask.MinimumDuration.Ticks)) break;
                    if (Z.SeedTaskObj == null) continue;
                    //using EvCenAllocFlag to toggle between left and right each iteration
                    if (Z.EvCenAllocFlag)
                    {
                        //try to add to the left first, else try to add to the right
                        allocated = EvenCenteredTryAddTimeLeft(P, Z);
                        if (!allocated) allocated = EvenCenteredTryAddTimeRight(P, Z);
                    }
                    else
                    {
                        //try to add to the right first, else try to add to the left
                        allocated = EvenCenteredTryAddTimeRight(P, Z);
                        if (!allocated) allocated = EvenCenteredTryAddTimeLeft(P, Z);
                    }
                    full = !allocated;
                    Z.EvCenAllocFlag = !Z.EvCenAllocFlag;
                }
            }
        }
        private static bool EvenCenteredTryAddTimeRight(PerZone P, InclusionZone Z)
        {
            bool allocated = false;
            if ((Z.SeedTaskObj.End + TimeTask.MinimumDuration) < Z.End)
            {
                //Add some time to the right
                Z.SeedTaskObj.End += TimeTask.MinimumDuration;
                P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                allocated = true;
            }
            return allocated;
        }
        private static bool EvenCenteredTryAddTimeLeft(PerZone P, InclusionZone Z)
        {
            bool allocated = false;
            if (Z.SeedTaskObj.Start - TimeTask.MinimumDuration >= Z.Start)
            {
                //Add some time to the left
                Z.SeedTaskObj.Start -= TimeTask.MinimumDuration;
                P.TimeConsumption.Remaining -= TimeTask.MinimumDuration.Ticks;
                allocated = true;
            }
            return allocated;
        }
        private void EvenApatheticTimeAllocate(CalendarTimeTaskMap M)
        {
            foreach (var P in M.PerZones)
            {
                //Fill zones evenly, but late
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterDesc());
                var minAlloc = new TimeSpan(Max(
                    TimeTask.MinimumDuration.Ticks,
                    M.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
                //First loop that creates CalendarObjects
                foreach (var Z in P.InclusionZones)
                {
                    if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                    if (Z.Duration < minAlloc) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    CalendarTaskObject CalObj = new CalendarTaskObject
                    {
                        Start = Z.End - minAlloc,
                        End = Z.End,
                        ParentInclusionZone = Z,
                        ParentPerZone = P,
                    };
                    Z.SeedTaskObj = CalObj;
                    P.TimeConsumption.Remaining -= minAlloc.Ticks;
                    P.CalTaskObjs.Add(CalObj);
                }
                //Second loop that adds more time to CalendarObjects
                minAlloc = TimeTask.MinimumDuration;
                bool full = false;
                while (!full && (P.TimeConsumption.Remaining > 0))
                {
                    full = true;
                    //add a small amount of time to each CalObj until they are full or out of allocated time
                    foreach (var Z in P.InclusionZones)
                    {
                        if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                        if (Z.SeedTaskObj == null) continue;
                        if (Z.SeedTaskObj.Start - minAlloc < Z.Start) continue;
                        Z.SeedTaskObj.Start -= minAlloc;
                        P.TimeConsumption.Remaining -= minAlloc.Ticks;
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
                if (P.InclusionZones.Count == 0) continue;
                P.InclusionZones.Sort(new InclusionSorterDesc());
                var minAlloc = new TimeSpan(Max(
                    TimeTask.MinimumDuration.Ticks,
                    M.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
                foreach (var Z in P.InclusionZones)
                {
                    if (!P.TimeConsumption.CanAllocate(minAlloc.Ticks)) break;
                    if (Z.Duration < minAlloc) continue;
                    bool hasCalObj = P.CalTaskObjs.Count(C => C.Intersects(Z)) > 0;
                    if (hasCalObj) continue;
                    var alloc = new TimeSpan(Z.Duration.Ticks
                        .Within(minAlloc.Ticks, P.TimeConsumption.Remaining))
                        .RoundUp(TimeTask.MinimumDuration);
                    var calObj = new CalendarTaskObject
                    {
                        Start = Z.End - alloc,
                        End = Z.End,
                        ParentInclusionZone = Z,
                        ParentPerZone = P,
                    };
                    P.CalTaskObjs.Add(calObj);
                    P.TimeConsumption.Remaining -= calObj.Duration.Ticks;
                    Z.SeedTaskObj = calObj;
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
                    C.Step1IgnoreFlag = false;
                    C.ReDistFlag = C.CanReDist;
                    C.LeftTangent = null;
                    C.RightTangent = null;
                }
                bool hasCollisions = true;
                while(hasCollisions)
                {
                    hasCollisions = false;
                    CollisionsStep1(D);
                    if (CollisionsStep2(D))
                        hasCollisions = true;
                }
            }
        }
        private void CollisionsStep1(int D)
        {
            //Step 1 - push and split collisions
            var Cs = GetCalObjsUnOrdered(D);
            foreach (var C in Cs)
            {
                if (C.Start == C.End) C.ParentPerZone.CalTaskObjs.Remove(C);
                C.Step1IgnoreFlag = false;
            }
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                var calObjs = GetCalObjsByStart(D);
                var intersections = GetIntersections(calObjs);
                foreach (var i in intersections)
                {
                    if (i.Item1.Step1IgnoreFlag || i.Item2.Step1IgnoreFlag)
                    {
                        i.Item1.Step1IgnoreFlag = true;
                        i.Item2.Step1IgnoreFlag = true;
                        continue;
                    }
                    CalendarTaskObject newC = null;
                    if (i.Item1.IsInside(i.Item2)) newC = InsideCollision(i.Item1, i.Item2, ref hasChanges);
                    else if (i.Item2.IsInside(i.Item1)) newC = InsideCollision(i.Item2, i.Item1, ref hasChanges);
                    if (newC != null)
                    {
                        newC.ParentPerZone.CalTaskObjs.Add(newC);
                        hasChanges = true;
                        break;
                    }
                    if (Step1Collisions(i.Item1, i.Item2))
                    {
                        hasChanges = true;
                        break;
                    }
                }
            }
        }
        private static CalendarTaskObject InsideCollision(CalendarTaskObject insider, CalendarTaskObject outsider, ref bool hasChanges)
        {
            //outsider can split when not locked, and task CanSplit
            if (outsider.CanSplit)
            {
                //if there is room for insider outside of outsider, then push instead of split
                if (insider.ParentInclusionZone != null &&
                    (insider.Duration <= insider.ParentInclusionZone.End - outsider.End ||
                    insider.Duration <= outsider.Start - insider.ParentInclusionZone.Start))
                    return null;
                else return SplitOutsider(insider, outsider);
            }
            else return null;
        }
        private static CalendarTaskObject SplitOutsider(CalendarTaskObject insider, CalendarTaskObject outsider)
        {
            //Make sure there is room on each side of the split for outsider.InstanceMinimum
            var minAlloc = new TimeSpan(Max(
                TimeTask.MinimumDuration.Ticks,
                outsider.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan.Ticks));
            if (minAlloc + minAlloc > outsider.Duration) return null;
            //var MDTi = insider.Start + new TimeSpan(insider.Duration.Ticks / 2);
            var split = new DateTime(insider.Start.Ticks.Within((outsider.Start + minAlloc).Ticks, (outsider.End - minAlloc).Ticks));
            if (split < insider.Start || split > insider.End) return null;
            var Left = new CalendarTaskObject();
            var Right = outsider;
            Left.Mimic(outsider);
            Left.EndLock = false;
            Left.End = split;
            Right.Start = split;
            Right.StartLock = false;
            Left.RightTangent = null;
            Right.LeftTangent = null;
            if (insider.LeftTangent != null)
                insider.LeftTangent.RightTangent = null;
            insider.LeftTangent = Left;
            Left.RightTangent = insider;
            if (outsider.State == CalendarTaskObject.States.Confirmed)
            {
                if (!Left.StartLock) Left.State = CalendarTaskObject.States.Unconfirmed;
                if (!Right.EndLock) Right.State = CalendarTaskObject.States.Unconfirmed;
            }
            return Left;
        }
        private static bool Step1Collisions(CalendarTaskObject C1, CalendarTaskObject C2)
        {
            //returns true when there are changes
            var collision = DetermineCollision(C1, C2);
            if (collision == null) return false;
            switch (collision.Result)
            {
                case Collision.CollisionResult.PushLeft:
                    TimeSpan Lroom = collision.Left.Start - collision.Left.ParentInclusionZone.Start;
                    TimeSpan LmaxPush = collision.Left.End - collision.Right.Start;
                    TimeSpan Lpush = new TimeSpan(Min(LmaxPush.Ticks, Lroom.Ticks));
                    PushLeft(collision, Lpush);
                    return true;
                case Collision.CollisionResult.ShrinkLeft:
                    TimeSpan shrinkL = new TimeSpan(Min(collision.Overlap.Ticks, collision.Left.Duration.Ticks));
                    //if shrink is less than minimum and not locked, delete it
                    if (collision.Left.Duration - shrinkL < collision.Left.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan
                        && !collision.Left.StartLock)
                        shrinkL = collision.Left.Duration;
                    ShrinkLeft(collision, shrinkL);
                    return true;
                case Collision.CollisionResult.ReDistLeft:
                    collision.Left.Step1IgnoreFlag = true;
                    break;
                case Collision.CollisionResult.PushRight:
                    TimeSpan Rroom = collision.Right.ParentInclusionZone.End - collision.Right.End;
                    TimeSpan RmaxPush = collision.Left.End - collision.Right.Start;
                    TimeSpan Rpush = new TimeSpan(Min(RmaxPush.Ticks, Rroom.Ticks));
                    PushRight(collision, Rpush);
                    return true;
                case Collision.CollisionResult.ShrinkRight:
                    TimeSpan shrinkR = new TimeSpan(Min(collision.Overlap.Ticks, collision.Right.Duration.Ticks));
                    if (collision.Right.Duration - shrinkR < collision.Right.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan
                        && !collision.Right.EndLock)
                        shrinkR = collision.Right.Duration;
                    ShrinkRight(collision, shrinkR);
                    return true;
                case Collision.CollisionResult.ReDistRight:
                    collision.Right.Step1IgnoreFlag = true;
                    break;
                case Collision.CollisionResult.PushLeftWC:
                    CascadePushLeft(collision);
                    return true;
                case Collision.CollisionResult.ShrinkLeftWC:
                    CascadeShrinkLeft(collision);
                    return true;
                case Collision.CollisionResult.ReDistLeftWC:
                    CascadeIgnoreLeft(collision);
                    break;
                case Collision.CollisionResult.PushRightWC:
                    CascadePushRight(collision);
                    return true;
                case Collision.CollisionResult.ShrinkRightWC:
                    CascadeShrinkRight(collision);
                    return true;
                case Collision.CollisionResult.ReDistRightWC:
                    CascadeIgnoreRight(collision);
                    break;
            }
            return false;
        }
        private bool CollisionsStep2(int D)
        {
            //Step 2 - Redistributions
            //returns true if there are probably still resolvable collisions
            bool hasCollisions = false;
            var calObjs = GetCalObjsByStart(D);
            var intersections = GetIntersections(calObjs);
            foreach (var i in intersections)
            {
                var collision = DetermineCollision(i.Item1, i.Item2);
                if (collision == null) continue;
                hasCollisions = true;
                switch (collision.Result)
                {
                    case Collision.CollisionResult.ReDistLeft:
                        Redistribute(collision, ShrinkLeft);
                        break;
                    case Collision.CollisionResult.ReDistLeftWC:
                        CascadeReDistLeft(collision);
                        break;
                    case Collision.CollisionResult.ReDistRight:
                        Redistribute(collision, ShrinkRight);
                        break;
                    case Collision.CollisionResult.ReDistRightWC:
                        CascadeReDistRight(collision);
                        break;
                }
            }
            return hasCollisions;
        }
        private void Redistribute(Collision collision, Action<Collision, TimeSpan> ShrinkFunc)
        {
            bool hasEmptySpaces = true;
            bool hasLesserObjs = true;
            //hasLesserObjs = false; //debug
            bool hasChanges = true;
            while (hasChanges)
            {
                hasChanges = false;
                if (collision.Overlap.Ticks <= 0) break;
                
                //Fill empty spaces
                if (hasEmptySpaces)
                {
                    var spaces = GetEmptySpaces(collision.Loser.TimeTask.Dimension);
                    if (ReDistPart2(collision, spaces, ShrinkFunc))
                    {
                        hasChanges = true;
                        continue;
                    }
                    else
                    {
                        hasEmptySpaces = false;
                    }
                }

                //Replace lesser objects
                if (hasLesserObjs)
                {
                    var allCalObjs = GetCalObjsByStart(collision.Loser.TimeTask.Dimension);
                    var lesserCalObjs = new List<CalendarTaskObject>(
                        from C in allCalObjs
                        where C.Priority < collision.Loser.Priority
                        orderby C.Priority
                        select C);
                    if (ReDistPart3(collision, lesserCalObjs, ShrinkFunc))
                    {
                        hasChanges = true;
                        continue;
                    }
                    else
                    {
                        hasLesserObjs = false;
                    }
                }

                //we have exhausted all possibilities for redistributing 
                collision.Loser.ReDistFlag = false;
            }
        }
        private bool ReDistPart2(Collision collision, List<EmptyZone> spaces, Action<Collision, TimeSpan> ShrinkFunc)
        {
            //returns true when there is a redistribution to an empty space
            var minAlloc = collision.Loser.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan;
            foreach (var S in spaces)
            {
                foreach (var Z in collision.Loser.ParentPerZone.InclusionZones)
                {
                    if (!S.Intersects(Z)) continue;
                    var SZOverlap = S.GetOverlap(Z);
                    var shrink = new TimeSpan(Min(SZOverlap.Ticks, collision.Overlap.Ticks));
                    //if redistribution would cause the object to shrink below InstanceMinimum, 
                    //try to redistribute up to that point,
                    //then try to redistribute the rest of it,
                    //else dont redistribute here at all
                    if (collision.Loser.Duration == minAlloc)
                        shrink = collision.Loser.Duration;
                    else if (collision.Loser.Duration - shrink < minAlloc)
                        shrink = collision.Loser.Duration - minAlloc;
                    if (shrink > SZOverlap) continue;
                    if (shrink.Ticks <= 0) continue;
                    //try to fill the empty zone
                    collision.Loser.ParentPerZone.TimeConsumption.Remaining += shrink.Ticks;
                    if (FillEmptyWithInsuffZ(S, Z))
                    {
                        collision.Loser.ParentPerZone.TimeConsumption.Remaining -= shrink.Ticks;
                        ShrinkFunc(collision, shrink);
                    }
                    else
                    {
                        //if for some reason it fails to fill the empty zone, undo and continue
                        collision.Loser.ParentPerZone.TimeConsumption.Remaining -= shrink.Ticks;
                        continue;
                    }
                    //break out of the loops to update the spaces collection
                    return true;
                }
            }
            return false;
        }
        private bool ReDistPart3(Collision collision, List<CalendarTaskObject> lesserObjs, Action<Collision, TimeSpan> ShrinkFunc)
        {
            //returns true when a lesser object is overwritten by a redistribution
            var minAlloc = collision.Loser.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan;
            foreach (var C in lesserObjs)
            {
                //cannot redistribute to objects with locks
                if (C.StartLock || C.EndLock) continue;
                //check if the lesserObj to be overwritten is actually intersecting the object to be redistributed
                //if it is, then shrink the lesserObj instead of redistributing
                if (C.Intersects(collision.Loser))
                {
                    var CLOverlap = C.GetOverlap(collision.Loser);
                    var shrink = new TimeSpan(Min(CLOverlap.Ticks, collision.Overlap.Ticks));
                    if (C.Duration - shrink < C.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan)
                        shrink = C.Duration;
                    if (C.Start < collision.Loser.Start)
                    {
                        Collision reDistCol = new Collision
                        {
                            Left = C,
                            Right = collision.Loser,
                            Result = Collision.CollisionResult.ShrinkLeft,
                        };
                        ShrinkLeft(reDistCol, shrink);
                    }
                    else
                    {
                        Collision reDistCol = new Collision
                        {
                            Left = collision.Loser,
                            Right = C,
                            Result = Collision.CollisionResult.ShrinkRight,
                        };
                        ShrinkRight(reDistCol, shrink);
                    }
                    return true;
                }
                //find any lesser objects that intersect an inclusion zone of the object to be redistributed
                //if it is, then redistribute by overwriting the lesser object
                foreach (var Z in collision.Loser.ParentPerZone.InclusionZones)
                {
                    if (!C.Intersects(Z)) continue;
                    var CZOverlap = C.GetOverlap(Z);
                    var shrink = new TimeSpan(Min(CZOverlap.Ticks, collision.Overlap.Ticks));
                    //if redistribution would cause the object to shrink below InstanceMinimum, 
                    //try to redistribute up to that point,
                    //then try to redistribute the rest of it,
                    //else dont redistribute here at all
                    if (collision.Loser.Duration == minAlloc)
                        shrink = collision.Loser.Duration;
                    else if (collision.Loser.Duration - shrink < minAlloc)
                        shrink = collision.Loser.Duration - minAlloc;
                    if (shrink > CZOverlap) continue;
                    if (shrink.Ticks <= 0) continue;
                    //overwrite C
                    if (C.LeftTangent?.TimeTask.Id == collision.Loser.TimeTask.Id)
                    {
                        //The redistributing object has a sibling object to the left of lesser object C
                        //lets expand the sibling object into C
                        Collision reDistCol = new Collision
                        {
                            Left = C.LeftTangent,
                            Right = C,
                            Result = Collision.CollisionResult.ShrinkRight,
                        };
                        ExpandRight(C.LeftTangent, shrink);
                        //shrink C, respecting InstanceMinimum
                        ShrinkRight(reDistCol,
                            C.Duration - shrink < C.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan ?
                            C.Duration : shrink);
                    }
                    else if (C.RightTangent?.TimeTask.Id == collision.Loser.TimeTask.Id)
                    {
                        Collision reDistCol = new Collision
                        {
                            Left = C,
                            Right = C.RightTangent,
                            Result = Collision.CollisionResult.ShrinkLeft,
                        };
                        ExpandLeft(C.RightTangent, shrink);
                        ShrinkLeft(reDistCol,
                            C.Duration - shrink < C.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan ?
                            C.Duration : shrink);
                    }
                    else
                    {
                        //no usable tangents, overwrite C by creating a new instance
                        //new object must respect InstanceMinimum
                        if (shrink < minAlloc) continue;
                        CalendarTaskObject reDistObj = new CalendarTaskObject
                        {
                            Start = C.Start,
                            End = C.Start + shrink,
                            ParentInclusionZone = collision.Loser.ParentInclusionZone,
                            ParentPerZone = collision.Loser.ParentPerZone,
                            ReDistFlag = false,
                        };
                        if (reDistObj.ParentPerZone.TimeConsumption != null)
                            reDistObj.ParentPerZone.TimeConsumption.Remaining -= shrink.Ticks;
                        reDistObj.ParentPerZone.CalTaskObjs.Add(reDistObj);
                        reDistObj.LeftTangent = C.LeftTangent;
                        if (C.LeftTangent != null)
                            C.LeftTangent.RightTangent = reDistObj;
                        C.LeftTangent = null;
                        Collision reDistCol = new Collision
                        {
                            Left = reDistObj,
                            Right = C,
                            Result = Collision.CollisionResult.ShrinkRight,
                        };
                        ShrinkRight(reDistCol,
                            C.Duration - shrink < C.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan ?
                            C.Duration : shrink);
                    }
                    ShrinkFunc(collision, shrink);
                    return true;
                }
            }
            return false;
        }
        private static void PushLeft(Collision collision, TimeSpan push)
        {
            collision.Left.Start -= push;
            collision.Left.End -= push;
            if (collision.Left.LeftTangent != null)
                collision.Left.LeftTangent.RightTangent = null;
            collision.Left.LeftTangent = null;
            if (collision.Left.RightTangent != null)
                collision.Left.RightTangent.LeftTangent = null;
            collision.Left.RightTangent = null;
            if (collision.Left.End == collision.Right.Start)
            {
                if (collision.Right.LeftTangent != null)
                    collision.Right.LeftTangent.RightTangent = null;
                collision.Left.RightTangent = collision.Right;
                collision.Right.LeftTangent = collision.Left;
            }
        }
        private static void PushRight(Collision collision, TimeSpan push)
        {
            collision.Right.Start += push;
            collision.Right.End += push;
            if (collision.Right.RightTangent != null)
                collision.Right.RightTangent.LeftTangent = null;
            collision.Right.RightTangent = null;
            if (collision.Right.LeftTangent != null)
                collision.Right.LeftTangent.RightTangent = null;
            collision.Right.LeftTangent = null;
            if (collision.Left.End == collision.Right.Start)
            {
                if (collision.Left.RightTangent != null)
                    collision.Left.RightTangent.LeftTangent = null;
                collision.Right.LeftTangent = collision.Left;
                collision.Left.RightTangent = collision.Right;
            }
        }
        private static void ShrinkLeft(Collision collision, TimeSpan shrink)
        {
            collision.Left.End -= shrink;
            if (collision.Left.ParentPerZone.TimeConsumption != null)
                collision.Left.ParentPerZone.TimeConsumption.Remaining += shrink.Ticks;
            if (collision.Left.RightTangent != null)
                collision.Left.RightTangent.LeftTangent = null;
            collision.Left.RightTangent = null;
            if (collision.Left.End == collision.Right.Start)
            {
                if (collision.Right.LeftTangent != null)
                    collision.Right.LeftTangent.RightTangent = null;
                collision.Right.LeftTangent = collision.Left;
                collision.Left.RightTangent = collision.Right;
            }
            if (collision.Left.Start == collision.Left.End)
            {
                if (collision.Left.LeftTangent != null)
                    collision.Left.LeftTangent.RightTangent = null;
                if (collision.Left.RightTangent != null)
                    collision.Left.RightTangent.LeftTangent = null;
                collision.Left.ParentPerZone.CalTaskObjs.Remove(collision.Left);
            }
        }
        private static void ShrinkRight(Collision collision, TimeSpan shrink)
        {
            collision.Right.Start += shrink;
            if (collision.Right.ParentPerZone.TimeConsumption != null)
                collision.Right.ParentPerZone.TimeConsumption.Remaining += shrink.Ticks;
            if (collision.Right.LeftTangent != null)
                collision.Right.LeftTangent.RightTangent = null;
            collision.Right.LeftTangent = null;
            if (collision.Left.End == collision.Right.Start)
            {
                if (collision.Left.RightTangent != null)
                    collision.Left.RightTangent.LeftTangent = null;
                collision.Right.LeftTangent = collision.Left;
                collision.Left.RightTangent = collision.Right;
            }
            if (collision.Right.Start == collision.Right.End)
            {
                if (collision.Right.RightTangent != null)
                    collision.Right.RightTangent.LeftTangent = null;
                if (collision.Right.LeftTangent != null)
                    collision.Right.LeftTangent.RightTangent = null;
                collision.Right.ParentPerZone.CalTaskObjs.Remove(collision.Right);
            }
        }
        private static void ExpandLeft(CalendarTaskObject C, TimeSpan expand)
        {
            C.Start -= expand;
            C.ParentPerZone.TimeConsumption.Remaining -= expand.Ticks;
            if (C.LeftTangent != null)
                C.LeftTangent.RightTangent = null;
            C.LeftTangent = null;
        }
        private static void ExpandRight(CalendarTaskObject C, TimeSpan expand)
        {
            C.End += expand;
            C.ParentPerZone.TimeConsumption.Remaining -= expand.Ticks;
            if (C.RightTangent != null)
                C.RightTangent.LeftTangent = null;
            C.RightTangent = null;
        }
        private static void CascadeIgnoreLeft(Collision collision)
        {
            var LT = collision.Left;
            LT.Step1IgnoreFlag = true;
            while (LT != collision.LData.WeakestC)
            {
                LT.Step1IgnoreFlag = true;
                LT = LT.LeftTangent;
            }
        }
        private static void CascadeIgnoreRight(Collision collision)
        {
            var RT = collision.Right;
            RT.Step1IgnoreFlag = true;
            while (RT != collision.RData.WeakestC)
            {
                RT.Step1IgnoreFlag = true;
                RT = RT.RightTangent;
            }
        }
        private static void CascadePushLeft(Collision collision)
        {
            //cascade push then push WC
            TimeSpan pushWCL = FindSmallestLeftCascadeAmount(collision);
            Collision lastCol = CascadeLeft(collision, pushWCL);
            lastCol.Result = Collision.CollisionResult.PushLeft;
            PushLeft(lastCol, pushWCL);
        }
        private static void CascadePushRight(Collision collision)
        {
            //cascade push then push WC
            TimeSpan pushWCR = FindSmallestRightCascadeAmount(collision);
            Collision lastCol = CascadeRight(collision, pushWCR);
            lastCol.Result = Collision.CollisionResult.PushRight;
            PushRight(lastCol, pushWCR);
        }
        private static void CascadeShrinkLeft(Collision collision)
        {
            //cascade push then shrink WC
            TimeSpan shrinkWCL = FindSmallestLeftCascadeAmount(collision);
            Collision lastCol = CascadeLeft(collision, shrinkWCL);
            lastCol.Result = Collision.CollisionResult.ShrinkLeft;
            ShrinkLeft(lastCol, shrinkWCL);
        }
        private static void CascadeShrinkRight(Collision collision)
        {
            //cascade push then shrink WC
            TimeSpan shrinkWCR = FindSmallestRightCascadeAmount(collision);
            Collision lastCol = CascadeRight(collision, shrinkWCR);
            lastCol.Result = Collision.CollisionResult.ShrinkRight;
            ShrinkRight(lastCol, shrinkWCR);
        }
        private void CascadeReDistLeft(Collision collision)
        {
            //cascade push then redist WC
            TimeSpan shrinkWCL = FindSmallestLeftCascadeAmount(collision);
            Collision lastCol = CascadeLeft(collision, shrinkWCL);
            lastCol.Result = Collision.CollisionResult.ReDistLeft;
            Redistribute(lastCol, ShrinkLeft);
        }
        private void CascadeReDistRight(Collision collision)
        {
            //cascade push then redist WC
            TimeSpan shrinkWCR = FindSmallestRightCascadeAmount(collision);
            Collision lastCol = CascadeRight(collision, shrinkWCR);
            lastCol.Result = Collision.CollisionResult.ReDistRight;
            Redistribute(lastCol, ShrinkRight);
        }
        private static TimeSpan FindSmallestLeftCascadeAmount(Collision collision)
        {
            //find the smallest amount that can be pushed in the chain of LTs up to WC
            //Cannot go below the size of WC
            TimeSpan shrinkWCL = new TimeSpan(Min(collision.Overlap.Ticks, collision.LData.WeakestC.Duration.Ticks));
            var LT = collision.Left;
            while (LT != collision.LData.WeakestC)
            {
                if (LT.ParentInclusionZone != null)
                {
                    //Cannot push anything in the chain out of its zone
                    var room = LT.Start - LT.ParentInclusionZone.Start;
                    if (shrinkWCL > room)
                        shrinkWCL = room;
                }
                LT = LT.LeftTangent;
            }
            return shrinkWCL;
        }
        private static TimeSpan FindSmallestRightCascadeAmount(Collision collision)
        {
            TimeSpan shrinkWCR = new TimeSpan(Min(collision.Overlap.Ticks, collision.RData.WeakestC.Duration.Ticks));
            var RT = collision.Right;
            while (RT != collision.RData.WeakestC)
            {
                if (RT.ParentInclusionZone != null)
                {
                    var room = RT.ParentInclusionZone.End - RT.End;
                    if (shrinkWCR > room)
                        shrinkWCR = room;
                }
                RT = RT.RightTangent;
            } 
            return shrinkWCR;
        }
        private static Collision CascadeLeft(Collision collision, TimeSpan push)
        {
            var LT = collision.Left;
            var pLT = collision.Right;
            while (LT != collision.LData.WeakestC)
            {
                var col = new Collision
                {
                    Left = LT,
                    Right = pLT,
                };
                pLT = LT;
                LT = LT.LeftTangent;
                PushLeft(col, push);
            }
            var lastCol = new Collision
            {
                Left = LT,
                Right = pLT,
            };
            return lastCol;
        }
        private static Collision CascadeRight(Collision collision, TimeSpan push)
        {
            var RT = collision.Right;
            var pRT = collision.Left;
            while (RT != collision.RData.WeakestC)
            {
                var col = new Collision
                {
                    Left = pRT,
                    Right = RT,
                };
                pRT = RT;
                RT = RT.RightTangent;
                PushRight(col, push);
            }
            var lastCol = new Collision
            {
                Left = pRT,
                Right = RT,
            };
            return lastCol;
        }
        private static Collision DetermineCollision(CalendarTaskObject C1, CalendarTaskObject C2)
        {
            //returns false when this collision is ignored.
            //Refer to Plans.xlsx - Collisions
            //Left Right Intersection Collisions C1 ∩ C2
            CalendarTaskObject L;
            CalendarTaskObject R;
            //Refer to Plans.xlsx - LeftRight
            //AssignLR(C1, C2, out L, out R);
            AssignLR2(C1, C2, out L, out R);
            //Refer to Plans.xlsx - Redistributions
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
            if (R.StartLock) //N
            {
                if (L.EndLock) return null; //8
                if (LData.HasRoom) //2 3
                {
                    if (L != LData.WeakestC) //2
                    {
                        collision.cell = "N2";
                        collision.Result = Collision.CollisionResult.PushLeftWC;
                    }
                    else //3
                    {
                        collision.cell = "N3";
                        collision.Result = Collision.CollisionResult.PushLeft;
                    }
                }
                else if (LData.CanReDist) //4 5
                {
                    if (L != LData.WeakestC) //4
                    {
                        collision.Result = Collision.CollisionResult.ReDistLeftWC;
                        collision.cell = "N4";
                    }
                    else //5
                    {
                        collision.Result = Collision.CollisionResult.ReDistLeft;
                        collision.cell = "N5";
                    }
                }
                else if (L != LData.WeakestC) //6
                {
                    collision.Result = Collision.CollisionResult.ShrinkLeftWC;
                    collision.cell = "N6";
                }
                else //7
                {
                    collision.Result = Collision.CollisionResult.ShrinkLeft;
                    collision.cell = "N7";
                }
            }
            else if (RData.HasRoom) //B C D E
            {
                if (LData.WeakestC.Priority >= RData.WeakestC.Priority) //B C
                {
                    if (R != RData.WeakestC) //B
                    {
                        collision.Result = Collision.CollisionResult.PushRightWC;
                        collision.cell = "B";
                    }
                    else //C
                    {
                        collision.Result = Collision.CollisionResult.PushRight;
                        collision.cell = "C";
                    }
                }
                else //D E
                {
                    if (R != RData.WeakestC) //D
                    {
                        if (LData.HasRoom)
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.cell = "D2";
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                            }
                            else //3
                            {
                                collision.cell = "D3";
                                collision.Result = Collision.CollisionResult.PushLeft;
                            }
                        }
                        else //45678
                        {
                            collision.cell = "D45678";
                            collision.Result = Collision.CollisionResult.PushRightWC;
                        }
                    }
                    else //E
                    {
                        if (LData.HasRoom)
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.cell = "E2";
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                            }
                            else //3
                            {
                                collision.cell = "E3";
                                collision.Result = Collision.CollisionResult.PushLeft;
                            }
                        }
                        else //E45678
                        {
                            collision.cell = "E45678";
                            collision.Result = Collision.CollisionResult.PushRight;
                        }
                    }
                }
            }
            else if (RData.CanReDist) //F G H I
            {
                if (LData.WeakestC.Priority >= RData.WeakestC.Priority) //F G
                {
                    if (R != RData.WeakestC) //F
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "F2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "F3";
                            }
                        }
                        else //45678
                        {
                            collision.Result = Collision.CollisionResult.ReDistRightWC;
                            collision.cell = "F45678";
                        }
                    }
                    else //G
                    {
                        if (LData.HasRoom) //2
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "G2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "G3";
                            }
                        }
                        else //45678
                        {
                            collision.Result = Collision.CollisionResult.ReDistRight;
                            collision.cell = "G45678";
                        }
                    }
                }
                else //H I
                {
                    if (R != RData.WeakestC) //H
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "H2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "H3";
                            }
                        }
                        else //4 5 67 8
                        {
                            if (L.EndLock) //8
                            {
                                collision.Result = Collision.CollisionResult.ReDistRightWC;
                                collision.cell = "H8";
                            }
                            else if (LData.CanReDist) //4 5
                            {
                                if (L != LData.WeakestC) //4
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeftWC;
                                    collision.cell = "H4";
                                }
                                else //5
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "H4";
                                }
                            }
                            else //67
                            {
                                collision.Result = Collision.CollisionResult.ReDistRightWC;
                                collision.cell = "H67";
                            }
                        }
                    }
                    else //I
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "I2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "I3";
                            }
                        }
                        else //4 5 67 8
                        {
                            if (L.EndLock) //8
                            {
                                collision.Result = Collision.CollisionResult.ReDistRight;
                                collision.cell = "I8";
                            }
                            else if (LData.CanReDist) //4 5
                            {
                                if (L != LData.WeakestC) //4
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeftWC;
                                    collision.cell = "I4";
                                }
                                else //5
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "I5";
                                }
                            }
                            else //56
                            {
                                collision.Result = Collision.CollisionResult.ReDistRight;
                                collision.cell = "I67";
                            }
                        }
                    }
                }
            }
            else //J K L M
            {
                if (LData.WeakestC.Priority >= RData.WeakestC.Priority) //J K
                {
                    if (R != RData.WeakestC) //J
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "J2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "J3";
                            }
                        }
                        else //4 5 67 8
                        {
                            if (L.EndLock)
                            {
                                collision.Result = Collision.CollisionResult.ShrinkRightWC;
                                collision.cell = "J8";
                            }
                            else if (LData.CanReDist) //4 5
                            {
                                if (L != LData.WeakestC) //4
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeftWC;
                                    collision.cell = "J4";
                                }
                                else //5
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "J5";
                                }
                            }
                            else //67
                            {
                                collision.Result = Collision.CollisionResult.ShrinkRightWC;
                                collision.cell = "J67";
                            }
                        }
                    }
                    else //K
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "K2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "K3";
                            }
                        }
                        else //4 5 67 8
                        {
                            if (L.EndLock) //8
                            {
                                collision.Result = Collision.CollisionResult.ShrinkRight;
                                collision.cell = "K8";
                            }
                            else if (LData.CanReDist) //4 5
                            {
                                if (L != LData.WeakestC) //4
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeftWC;
                                    collision.cell = "K4";
                                }
                                else //5
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "K5";
                                }
                            }
                            else //67
                            {
                                collision.Result = Collision.CollisionResult.ShrinkRight;
                                collision.cell = "K67";
                            }
                        }
                    }
                }
                else //L M
                {
                    if (R != RData.WeakestC) //L
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "L2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "L3";
                            }
                        }
                        else //4 5 6 7 8
                        {
                            if (L.EndLock) //8
                            {
                                collision.Result = Collision.CollisionResult.ShrinkRightWC;
                                collision.cell = "L8";
                            }
                            else if (LData.CanReDist) //4 5
                            {
                                if (L != LData.WeakestC) //4
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeftWC;
                                    collision.cell = "L4";
                                }
                                else //5
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "L5";
                                }
                            }
                            else //6 7
                            {
                                if (L != LData.WeakestC) //6
                                {
                                    collision.Result = Collision.CollisionResult.ShrinkLeftWC;
                                    collision.cell = "L6";
                                }
                                else //7
                                {
                                    collision.Result = Collision.CollisionResult.ShrinkLeft;
                                    collision.cell = "L7";
                                }
                            }
                        }
                    }
                    else //M
                    {
                        if (LData.HasRoom) //2 3
                        {
                            if (L != LData.WeakestC) //2
                            {
                                collision.Result = Collision.CollisionResult.PushLeftWC;
                                collision.cell = "M2";
                            }
                            else //3
                            {
                                collision.Result = Collision.CollisionResult.PushLeft;
                                collision.cell = "M3";
                            }
                        }
                        else //4 5 6 7 8
                        {
                            if (L.EndLock) //8
                            {
                                collision.Result = Collision.CollisionResult.ShrinkRight;
                                collision.cell = "M8";
                            }
                            else if (LData.CanReDist)
                            {
                                if (L != LData.WeakestC) //4
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeftWC;
                                    collision.cell = "M4";
                                }
                                else //5
                                {
                                    collision.Result = Collision.CollisionResult.ReDistLeft;
                                    collision.cell = "M5";
                                }
                            }
                            else //6 7
                            {
                                if (L != LData.WeakestC)
                                {
                                    collision.Result = Collision.CollisionResult.ShrinkLeftWC;
                                    collision.cell = "M6";
                                }
                                else
                                {
                                    collision.Result = Collision.CollisionResult.ShrinkLeft;
                                    collision.cell = "M7";
                                }
                            }
                        }
                    }
                }
            }
            return collision;
        }
        private static void AssignLR(CalendarTaskObject C1, CalendarTaskObject C2, out CalendarTaskObject L, out CalendarTaskObject R)
        {
            //This version of LR assignment goes by tangents
            var C1CoM = C1.Start + new TimeSpan((long)(C1.Duration.Ticks / 2d));
            var C2CoM = C2.Start + new TimeSpan((long)(C2.Duration.Ticks / 2d));
            if (C2.LeftTangent != null) //B D
            {
                if (C2.RightTangent != null) //B
                {
                    if (C1.LeftTangent != null) //8 10
                    {
                        if (C1.RightTangent != null) //8
                        {
                            if (C1CoM <= C2CoM)
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
                        else //10
                        {
                            L = C1;
                            R = C2;
                        }
                    }
                    else //9 11
                    {
                        if (C1.RightTangent != null) //9
                        {
                            L = C2;
                            R = C1;
                        }
                        else //11
                        {
                            L = C1;
                            R = C2;
                        }
                    }
                }
                else //D
                {
                    if (C1.LeftTangent != null && C1.RightTangent == null) //10
                    {
                        if (C1CoM <= C2CoM)
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
                    else //8 9 11
                    {
                        L = C2;
                        R = C1;
                    }
                }
            }
            else //C E
            {
                if (C2.RightTangent != null) //C
                {
                    if (C1.LeftTangent == null && C1.RightTangent != null) //9
                    {
                        if (C1CoM <= C2CoM)
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
                    else //8 10 11
                    {
                        L = C1;
                        R = C2;
                    }
                }
                else //E
                {
                    if (C1.LeftTangent != null) //8 10
                    {
                        if (C1.RightTangent != null) //8
                        {
                            if (C1CoM <= C2CoM)
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
                        else //10
                        {
                            L = C1;
                            R = C2;
                        }
                    }
                    else //9 11
                    {
                        if (C1.RightTangent != null) //9
                        {
                            L = C2;
                            R = C1;
                        }
                        else //11
                        {
                            if (C1CoM <= C2CoM)
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
                    }
                }
            }
        }
        private static void AssignLR2(CalendarTaskObject C1, CalendarTaskObject C2, out CalendarTaskObject L, out CalendarTaskObject R)
        {
            //This version of LR assignment goes by center of mass, then by tangents
            var C1CoM = C1.Start + new TimeSpan((long)(C1.Duration.Ticks / 2d));
            var C2CoM = C2.Start + new TimeSpan((long)(C2.Duration.Ticks / 2d));
            if (C1CoM < C2CoM)
            {
                L = C1;
                R = C2;
            }
            else if (C1CoM == C2CoM)
            {
                if (C2.LeftTangent != null) //B D
                {
                    if (C2.RightTangent != null) //B
                    {
                        if (C1.LeftTangent == null &&
                            C1.RightTangent != null) //3
                        {
                            L = C2;
                            R = C1;
                        }
                        else //2 4 5
                        {
                            L = C1;
                            R = C2;
                        }
                    }
                    else //D
                    {
                        if (C1.LeftTangent != null &&
                            C1.RightTangent == null) //4
                        {
                            L = C1;
                            R = C2;
                        }
                        else //2 3 5
                        {
                            L = C2;
                            R = C1;
                        }
                    }
                }
                else //C E
                {
                    if (C2.RightTangent != null) //C
                    {
                        L = C1;
                        R = C2;
                    }
                    else //E
                    {
                        if (C1.LeftTangent == null &&
                            C1.RightTangent != null) //3
                        {
                            L = C2;
                            R = C1;
                        }
                        else //2 4 5
                        {
                            L = C1;
                            R = C2;
                        }
                    }
                }
            }
            else //C1CoM > C2CoM
            {
                L = C2;
                R = C1;
            }
        }
        private class Collision
        {
            public enum CollisionResult
            {
                PushLeft, ShrinkLeft, ReDistLeft,
                PushRight, ShrinkRight, ReDistRight,
                PushLeftWC, ShrinkLeftWC, ReDistLeftWC,
                PushRightWC, ShrinkRightWC, ReDistRightWC,
            }
            public CollisionResult Result;
            public string cell = "manual collision";
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
                        case CollisionResult.ReDistLeftWC:
                            return Left;
                        case CollisionResult.PushRight:
                        case CollisionResult.ShrinkRight:
                        case CollisionResult.ReDistRight:
                        case CollisionResult.ReDistRightWC:
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
                        case CollisionResult.ReDistLeftWC:
                            return Right;
                        case CollisionResult.PushRight:
                        case CollisionResult.ShrinkRight:
                        case CollisionResult.ReDistRight:
                        case CollisionResult.ReDistRightWC:
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
            public CalendarTaskObject WeakestC;
            public override string ToString()
            {
                string s = String.Format("{0} {1} WC[{2}] {3}",
                    Direction,
                    HasRoom ? "" : "🔒",
                    WeakestC.ParentPerZone.ParentMap.TimeTask.Name,
                    CanReDist ? "CanReDist" : "");
                return s;
            }
        }
        private static PushData GetLeftPushData(CalendarTaskObject C)
        {
            bool hasRoom = true;
            //The current Left Tangent we're looking at
            var LT = C; 
            //Weakest CalObj on the left side
            var WC = C; 
            //If at least one CalObj on the left side CanReDist
            bool canReDist = C.ReDistFlag;
            //Prefer a WC that is Limited
            bool isLimited = C.TimeTask.TimeAllocation?.Limited ?? false;
            while (true)
            {
                //If C.HasRoom == F, LC is the nearest LT where LT.S🔒 or LT.Z.S >= LT.S or LT.LT.E🔒. 
                //LP = WC.P where WC is the LT with the lowest priority from C to LC.
                //LmaxRoom is the amount of room WC has to be pushed
                //Also if any LT CanReDist, then WC must also have CanReDist
                //Also if any LT is Limited, the WC must also be Limited
                //Prefer WC that is redistributable over Limited
                //See Plans.xlsx - Redistributions
                PushDataPart2(C, LT, ref WC, ref canReDist, ref isLimited);
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
                    WC = LT;
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
                WeakestC = WC,
                CanReDist = canReDist,
            };
        }
        private static PushData GetRightPushData(CalendarTaskObject C)
        {
            bool hasRoom = true;
            var RT = C;
            var WC = C;
            bool canReDist = C.ReDistFlag;
            bool isLimited = C.TimeTask.TimeAllocation?.Limited ?? false;
            while (true)
            {
                PushDataPart2(C, RT, ref WC, ref canReDist, ref isLimited);
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
                    WC = RT;
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
                WeakestC = WC,
                CanReDist = canReDist,
            };
        }
        private static void PushDataPart2(CalendarTaskObject C, CalendarTaskObject T, ref CalendarTaskObject WC, ref bool canReDist, ref bool isLimited)
        {
            if (T.ReDistFlag) //B C D E
            {
                if (C.TimeTask.TimeAllocation?.Limited ?? false) //B C
                {
                    if (T.Priority < WC.Priority) //B
                    {
                        WC = T;
                        canReDist = true;
                        isLimited = true;
                    }
                    else //C
                    {
                        if (!canReDist || !isLimited) //11 12 13
                        {
                            WC = T;
                            canReDist = true;
                            isLimited = true;
                        }
                    }
                }
                else //D E
                {
                    if (T.Priority < WC.Priority) //D
                    {
                        if (!canReDist || !isLimited) //11 12 13
                        {
                            WC = T;
                            canReDist = true;
                        }
                    }
                    else //E
                    {
                        if (!canReDist) //12 13
                        {
                            WC = T;
                            canReDist = true;
                        }
                    }
                }
            }
            else //F G H I
            {
                if (C.TimeTask.TimeAllocation?.Limited ?? false) //F G
                {
                    if (T.Priority < WC.Priority) //F
                    {
                        if (!canReDist) //12 13
                        {
                            WC = T;
                            isLimited = true;
                        }
                    }
                    else //G
                    {
                        if (!canReDist && !isLimited) //13
                        {
                            WC = T;
                            isLimited = true;
                        }
                    }
                }
                else //H I
                {
                    if (T.Priority < WC.Priority) //H
                    {
                        if (!canReDist && !isLimited) //13
                        {
                            WC = T;
                        }
                    }
                }
            }
        }
        private static List<Tuple<CalendarTaskObject, CalendarTaskObject>> GetIntersections(List<CalendarTaskObject> calObjs)
        {
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
                            where Z.ParentPerZone.TimeConsumption?.Allocation.Limited == false
                            orderby Z.Priority descending
                            select Z;
                        var insuffZone = insuffZones.FirstOrDefault();
                        if (insuffZone != null)
                        {
                            if (FillEmptyWithInsuffZ(S, insuffZone))
                                hasChanges = true;
                        }
                        else
                        {
                            //no insufficient zone found
                            //look for CanFill zones
                            var fillZones =
                                from Z in zones
                                where Z.ParentPerZone.ParentMap.TimeTask.CanFill
                                orderby Z.Priority descending
                                select Z;
                            var fillZone = fillZones.FirstOrDefault();
                            if (fillZone != null)
                            {
                                if (FillEmptyWithFiller(S, fillZone))
                                    hasChanges = true;
                            }
                        }
                    }
                }
            }
        }
        private static bool FillEmptyWithInsuffZ(EmptyZone S, InclusionZone Z)
        {
            //returns true when a change is made that fills empty space
            //Check if the CalObjs on the left or right of the space can be used to fill
            //otherwise, create a new CalObj to fill
            if (Z.Start <= S.Start &&
                S.LeftTangent?.ParentInclusionZone == Z &&
                !S.LeftTangent.EndLock)
            {
                var newEnd = new DateTime(Min(Z.End.Ticks, Min(S.End.Ticks,
                S.LeftTangent.End.Ticks + (long)Max(S.LeftTangent.ParentPerZone.TimeConsumption.Remaining, 0))));
                var diff = newEnd - S.LeftTangent.End;
                ExpandRight(S.LeftTangent, diff);
            }
            else
            if (Z.End >= S.End &&
                S.RightTangent?.ParentInclusionZone == Z &&
                !S.RightTangent.StartLock)
            {
                var newStart = new DateTime(Max(Z.Start.Ticks, Max(S.Start.Ticks,
                S.RightTangent.Start.Ticks - (long)Max(S.RightTangent.ParentPerZone.TimeConsumption.Remaining, 0))));
                var diff = S.RightTangent.Start - newStart;
                ExpandLeft(S.RightTangent, diff);
            }
            else
            {
                var minAlloc = new TimeSpan((long)Max(
                    Z.ParentPerZone.TimeConsumption.Remaining, 
                    Z.ParentPerZone.ParentMap.TimeTask.TimeAllocation.InstanceMinimum));
                var start = new DateTime(Max(Z.Start.Ticks, S.Start.Ticks));
                var end = new DateTime(Min(Z.End.Ticks, Min(S.End.Ticks, start.Ticks + minAlloc.Ticks)));
                var C = new CalendarTaskObject
                {
                    Start = start,
                    End = end,
                    ParentPerZone = Z.ParentPerZone,
                    ParentInclusionZone = Z,
                    ReDistFlag = false,
                };
                if (C.Duration < C.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan) return false;
                Z.ParentPerZone.CalTaskObjs.Add(C);
                Z.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
            }
            return true;
        }
        private static bool FillEmptyWithFiller(EmptyZone S, InclusionZone Z)
        {
            //Check if the CalObjs on the left or right of the space can be used to fill
            //otherwise, create a new CalObj to fill
            if (Z.Start <= S.Start &&
                S.LeftTangent?.ParentInclusionZone == Z &&
                !S.LeftTangent.EndLock)
            {
                var newEnd = new DateTime(Min(Z.End.Ticks, S.End.Ticks));
                var diff = newEnd - S.LeftTangent.End;
                S.LeftTangent.End = newEnd;
                if (S.LeftTangent.ParentPerZone.TimeConsumption != null)
                    S.LeftTangent.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
            }
            else
            if (Z.End >= S.End &&
                S.RightTangent?.ParentInclusionZone == Z &&
                !S.RightTangent.StartLock)
            {
                var newStart = new DateTime(Max(Z.Start.Ticks, S.Start.Ticks));
                var diff = S.RightTangent.Start - newStart;
                S.RightTangent.Start = newStart;
                if (S.RightTangent.ParentPerZone.TimeConsumption != null)
                    S.RightTangent.ParentPerZone.TimeConsumption.Remaining -= diff.Ticks;
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
                if (C.Duration < C.TimeTask.TimeAllocation.InstanceMinimumAsTimeSpan) return false;
                Z.ParentPerZone.CalTaskObjs.Add(C);
                if (Z.ParentPerZone.TimeConsumption != null)
                    Z.ParentPerZone.TimeConsumption.Remaining -= C.Duration.Ticks;
            }
            return true;
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
                        LeftTangent = prev,
                        RightTangent = C,
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
                    LeftTangent = prev,
                    RightTangent = null
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
            FixMisalignments();
            MergeDumbSplits();
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
                                if (C.Intersects(Z) && (!C.IsWithin(Z) || C.ParentInclusionZone != Z))
                                {
                                    if (C.Start < Z.Start)
                                    {
                                        var split = new CalendarTaskObject();
                                        split.Mimic(C);
                                        split.End = Z.Start;
                                        split.EndLock = false;
                                        C.Start = Z.Start;
                                        C.StartLock = false;
                                        if (C.EndLock)
                                            C.State = CalendarTaskObject.States.Confirmed;
                                        else
                                            C.State = CalendarTaskObject.States.Unconfirmed;
                                        split.State = CalendarTaskObject.States.Unscheduled;
                                        newTaskObjs.Add(split);
                                    }
                                    if (C.End > Z.End)
                                    {
                                        var split = new CalendarTaskObject();
                                        split.Mimic(C);
                                        split.Start = Z.End;
                                        split.StartLock = false;
                                        C.End = Z.End;
                                        C.EndLock = false;
                                        if (C.StartLock)
                                            C.State = CalendarTaskObject.States.Confirmed;
                                        else
                                            C.State = CalendarTaskObject.States.Unconfirmed;
                                        split.State = CalendarTaskObject.States.Unscheduled;
                                        newTaskObjs.Add(split);
                                    }
                                    C.ParentInclusionZone = Z;
                                }
                                if (C.ParentInclusionZone != null)
                                {
                                    if (!C.IsWithin(C.ParentInclusionZone))
                                    {
                                        C.ParentInclusionZone = null;
                                        C.State = CalendarTaskObject.States.Unscheduled;
                                    }
                                    else if (C.ParentInclusionZone.Cancelled)
                                        C.State = CalendarTaskObject.States.Unscheduled;
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
        private void FlagInsufficients()
        {
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    //Flag Insufficients
                    if (P.TimeConsumption != null)
                    {
                        if (P.ParentMap.TimeTask.TimeAllocation.Limited)
                        {
                            if (P.TimeConsumption.Remaining < 0)
                                foreach (var C in P.CalTaskObjs)
                                    C.State = CalendarTaskObject.States.OverTime;
                        }
                        else
                        {
                            if (P.TimeConsumption.Remaining > 0)
                                foreach (var C in P.CalTaskObjs)
                                    C.State = CalendarTaskObject.States.Insufficient;
                        }
                    }
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
            DimensionCount = GetMaxDimension() + 1;
            foreach (var M in TaskMaps)
            {
                foreach (var P in M.PerZones)
                {
                    foreach (var C in P.CalTaskObjs)
                    {
                        C.DimensionCount = DimensionCount;
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
        private int GetMaxDimension()
        {
            var dims = GetDimensions();
            var count = dims.Count();
            return count > 0 ? dims.Max() : 0;
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
        private List<CalendarTaskObject> GetCalObjsByStart(int dimension)
        {
            return new List<CalendarTaskObject>(
                from M in TaskMaps
                where M.TimeTask.Dimension == dimension
                from P in M.PerZones
                from C in P.CalTaskObjs
                orderby C.Start, C.End, 
                (C.ParentInclusionZone?.Start ?? C.End),
                (C.ParentInclusionZone?.End ?? C.End),
                C.Priority descending
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
