// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    /// <summary>
    /// Generic ViewModel handles CRUD logic for Views. Bind a collection control's ItemsSource to View.
    /// </summary>
    /// <typeparam name="ModelType">EntityBase type</typeparam>
    public abstract class ViewModel<ModelType> : EditableObject, IView, IDisposable 
        where ModelType: EntityBase, new() 
    {
        #region Fields
        internal IView Parent;
        protected ITimeKeeperContext Context;
        protected IComparer Sorter;
        protected static IComparer NameSorter = new NameSorter();
        private string _status = "Ready";
        private string _SelectionString = "";
        private bool _IsEnabled = true;
        private bool _IsLoading = false;
        private bool _IsEditingItem = false;
        private bool _IsAddingNew = false;
        private bool _HasSelected = false;
        private bool _IsSaving = false;
        private ModelType _SelectedItem;
        private ModelType _CurrentEditItem;
        private ICommand _GetDataCommand;
        private ICommand _NewItemCommand;
        private ICommand _CancelCommand;
        private ICommand _CommitCommand;
        private ICommand _EditSelectedCommand;
        private ICommand _DeleteSelectedCommand;
        private ICommand _SaveAsCommand;
        #endregion
        #region Memento
        private Stack<IEnumerable<IMemento>> UndoStack = new Stack<IEnumerable<IMemento>>();
        private Stack<IEnumerable<IMemento>> RedoStack = new Stack<IEnumerable<IMemento>>();
        private ICommand _UndoCommand;
        private ICommand _RedoCommand;
        public ICommand UndoCommand => _UndoCommand
            ?? (_UndoCommand = new RelayCommand(ap => Undo(), pp => CanUndo));
        public ICommand RedoCommand => _RedoCommand
            ?? (_RedoCommand = new RelayCommand(ap => Redo(), pp => CanRedo));
        protected bool CanUndo => IsReady && UndoStack.Count > 0;
        protected bool CanRedo => IsReady && RedoStack.Count > 0;
        public virtual IEnumerable<IMemento> SaveStates()
        {
            List<IMemento> states = new List<IMemento>();
            states.Add(State);
            foreach (var item in Source) states.Add(item.State);
            return states;
        }
        public bool Managed { get; set; } = false;
        //Add this function to the beginning of commands to enable undo/redo
        protected void NewChange()
        {
            if (Managed) return;
            UndoStack.Push(SaveStates());
            RedoStack.Clear();
        }
        protected void ClearUndos()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }
        protected void Undo()
        {
            //save current state to RedoStack
            RedoStack.Push(SaveStates());
            //get previous state from UndoStack
            var states = UndoStack.Pop();
            foreach (IMemento state in states) state.RestoreState();
        }
        protected void Redo()
        {
            //save current state to UndoStack
            UndoStack.Push(SaveStates());
            //get next state from RedoStack
            var states = RedoStack.Pop();
            foreach (IMemento state in states) state.RestoreState();
        }
        #endregion
        #region Properties
        public abstract string Name { get; }
        [NotMapped]
        public string Status
        {
            get { return _status; }
            protected set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public CollectionViewSource Items { get; protected set; }
        public ObservableCollection<ModelType> Source => 
            Items?.Source as ObservableCollection<ModelType>;
        public ListCollectionView View => Items?.View as ListCollectionView;
        /// <summary>
        /// Bind DataGrid.CurrentItem otherwise Selector.SelectedItem to this property
        /// </summary>
        public ModelType SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                if (IsEditingItemOrAddingNew)
                {
                    OnPropertyChanged();
                    return;
                }
                //Item must not be itself and must be in Source
                if ((value == _SelectedItem) || (value != null && (!Source?.Contains(value) ?? false))) return;
                NewChange();
                _SelectedItem = value;
                if (SelectedItem == null)
                {
                    HasSelected = false;
                    SelectionString = "";
                }
                else
                {
                    HasSelected = true;
                    SelectionString = _SelectedItem.ToString();
                    Status = "Selected " + SelectedItem.GetTypeName();
                }
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Bind EditView to this property
        /// </summary>
        public ModelType CurrentEditItem
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
                if (value == null)
                    _CurrentEditItem.PropertyChanged -= CurrentEditItemPropertyChanged;
                else
                    value.PropertyChanged += CurrentEditItemPropertyChanged;
                _CurrentEditItem = value;
                OnPropertyChanged();
            }
        }
        private void CurrentEditItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
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
        #endregion
        #region Conditions
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
        public bool IsNotEnabled => !IsEnabled;
        public bool IsNotLoading => !IsLoading;
        public bool IsNotEditingItem => !IsEditingItem;
        public bool IsNotAddingNew => !IsAddingNew;
        public bool IsNotSaving => !IsSaving;
        public bool HasNotSelected => !HasSelected;
        private bool IsReady => IsNotSaving && IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew;
        #endregion
        #region Commands
        public ICommand CancelCommand => _CancelCommand
            ?? (_CancelCommand = new RelayCommand(ap => Cancel(), pp => CanCancel));
        public ICommand CommitCommand => _CommitCommand
            ?? (_CommitCommand = new RelayCommand(async ap => await Commit(), pp => CanCommit));
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(async ap => await LoadDataAsync(), pp => CanGetData));
        public ICommand NewItemCommand => _NewItemCommand
            ?? (_NewItemCommand = new RelayCommand(ap => AddNew(ap), pp => CanAddNew(pp)));
        public ICommand EditSelectedCommand => _EditSelectedCommand
            ?? (_EditSelectedCommand = new RelayCommand(ap => EditSelected(), pp => CanEditSelected));
        public ICommand DeleteSelectedCommand => _DeleteSelectedCommand
            ?? (_DeleteSelectedCommand = new RelayCommand(async ap => await DeleteSelected(), pp => CanDeleteSelected));
        public ICommand SaveAsCommand => _SaveAsCommand
            ?? (_SaveAsCommand = new RelayCommand(ap => SaveAs(), pp => CanSave));
        #endregion
        #region Predicates
        protected virtual bool CanCancel => IsAddingNew || (IsEditingItem && (View?.CanCancelEdit ?? false)); //CanCancelEdit requires IEditableItem on model
        protected virtual bool CanCommit => IsNotSaving && IsEditingItemOrAddingNew && !(CurrentEditItem?.HasErrors ?? true);
        protected virtual bool CanGetData => IsNotSaving && IsNotLoading;
        protected virtual bool CanAddNew(object pp)
        {
            return IsReady
                && (View?.CanAddNew ?? false);
        }
        protected virtual bool CanEditSelected => IsReady && HasSelected;
        protected virtual bool CanDeleteSelected => IsReady && HasSelected && (View?.CanRemove ?? false);
        protected virtual bool CanSave => IsReady;
        #endregion
        #region Actions
        protected abstract Task GetDataAsync();
        internal abstract void SaveAs();
        internal virtual async Task LoadDataAsync()
        {
            ClearUndos();
            IsEnabled = false;
            IsLoading = true;
            Cancel();
            SelectedItem = null;
            Status = "Loading Data...";
            Items = new CollectionViewSource();
            OnPropertyChanged(nameof(View));
            Dispose();
            try
            {
                await GetDataAsync();
                View.CustomSort = Sorter;
                View.GroupDescriptions.Add(new PropertyGroupDescription(nameof(EntityBase.IsEditable)));
                OnPropertyChanged(nameof(View));
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
        internal virtual void AddNew(object ap)
        {
            SelectedItem = null;
            IsAddingNew = true;
            Status = "Adding new " + CurrentEditItem.GetTypeName();
        }
        internal virtual void EditSelected()
        {
            CurrentEditItem = SelectedItem;
            SelectedItem = null;
            IsEditingItem = true; //before view.edit
            View.EditItem(CurrentEditItem);
            CurrentEditItem.IsEditing = true; //after view.edit
            Status = "Editing " + CurrentEditItem.GetTypeName();
        }
        protected virtual void FinishEdit()
        {
            IsEditingItem = false;
            IsAddingNew = false;
            if (CurrentEditItem != null) CurrentEditItem.IsEditing = false;
            CurrentEditItem = null;
            SelectedItem = null;
            //Refresh all of the buttons
            CommandManager.InvalidateRequerySuggested();
        }
        internal virtual void Cancel()
        {
            if (IsEditingItem)
            {
                View?.CancelEdit();
                Status = "Canceled";
            }
            else if (IsAddingNew)
            {
                View?.CancelNew();
                Status = "Canceled";
            }
            FinishEdit();
        }
        internal virtual async Task<bool> Commit()
        {
            NewChange();
            bool success = await SaveChangesAsync();
            if (success)
            {
                if (IsAddingNew)
                {
                    Status = CurrentEditItem.GetTypeName() + " Added";
                    View.CommitNew();
                }
                else if (IsEditingItem)
                {
                    Status = CurrentEditItem.GetTypeName() + " Modified";
                    View.CommitEdit();
                }
                FinishEdit();
            }
            return success;
        }
        internal virtual async Task<bool> DeleteSelected()
        {
            NewChange();
            string status = SelectedItem.GetTypeName() + " Deleted";
            View.Remove(SelectedItem);
            bool success = await SaveChangesAsync();
            if (success) Status = status;
            SelectedItem = null;
            return success;
        }
        internal virtual async Task<bool> SaveChangesAsync()
        {
            bool success = false;
            try
            {
                int x = await Context.SaveChangesAsync();
                success = true;
                Status = String.Format("{0} {1} affected.", x, x == 1 ? "row" : "rows");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Status = "There was a problem updating the database";
                ExceptionViewer ev = new ExceptionViewer(Status + ": Concurrency Error", ex);
                ev.ShowDialog();
            }
            catch (DbUpdateException ex)
            {
                Status = "There was a problem updating the database";
                ExceptionViewer ev = new ExceptionViewer(Status + ": Database Update Failed", ex);
                ev.ShowDialog();
            }
            catch (CommitFailedException ex)
            {
                Status = "There was a problem updating the database";
                ExceptionViewer ev = new ExceptionViewer(Status + ": Transaction Failure", ex);
                ev.ShowDialog();
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string s = "";
                foreach (var e in ex.EntityValidationErrors)
                {
                    var entity = e.Entry.Entity;
                    s += entity + ": \n";
                    foreach (var ve in e.ValidationErrors)
                    {
                        s += "\t" + ve.PropertyName + ": " + ve.ErrorMessage + "\n";
                    }
                }
                
                Status = "There was a problem updating the database";
                ExceptionViewer ev = new ExceptionViewer(Status + ":\nValidation Errors on: \n" + s, ex);
                ev.ShowDialog();
            }
            catch (Exception ex)
            {
                Status = "There was a problem updating the database";
                ExceptionViewer ev = new ExceptionViewer(Status, ex);
                ev.ShowDialog();
            }
            return success;
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
                    if (Context is IDisposable)
                    {
                        ((IDisposable)Context).Dispose();
                    }
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
