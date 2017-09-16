// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    /// <summary>
    /// Generic ViewModel handles CRUD logic for Views. Bind a collection control's ItemsSource to View.
    /// </summary>
    /// <typeparam name="ModelType">EntityBase type</typeparam>
    public abstract class EntityViewModel<ModelType> : ObservableObject, IView, IDisposable 
        where ModelType: EntityBase, new() 
    {
        #region Fields
        protected ITimeKeeperContext Context;
        protected IComparer Sorter;
        protected IComparer NameSorter;
        private String _status = "Ready";
        private bool _IsEnabled = true;
        private bool _IsLoading = false;
        private bool _IsEditingItem = false;
        private bool _IsAddingNew = false;
        private bool _HasSelected = false;
        private bool _IsSaving = false;
        private ModelType _SelectedItem;
        private ModelType _CurrentEditItem;
        private ICommand _GetDataCommand = null;
        private ICommand _NewItemCommand = null;
        private ICommand _CancelCommand = null;
        private ICommand _CommitCommand = null;
        private ICommand _EditSelectedCommand = null;
        private ICommand _DeleteSelectedCommand = null;
        private ICommand _SaveAsCommand = null;
        #endregion
        public EntityViewModel()
        {
            NameSorter = new NameSorter();
        }
        #region Properties
        public abstract string Name { get; }
        public String Status
        {
            get
            {
                return _status;
            }
            protected set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public CollectionViewSource Items { get; protected set; } = new CollectionViewSource();
        public ObservableCollection<ModelType> Source => Items?.Source as ObservableCollection<ModelType>;
        public ListCollectionView View => Items?.View as ListCollectionView;
        /// <summary>
        /// Bind DataGrid.CurrentItem otherwise Selector.SelectedItem to this property
        /// </summary>
        public ModelType SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                if (IsEditingItemOrAddingNew)
                {
                    OnPropertyChanged();
                    return;
                }
                //Item must not be itself and must be in Source
                if ((value == _SelectedItem) || (value != null && (!Source?.Contains(value) ?? false))) return;
                _SelectedItem = value;
                if (SelectedItem == null)
                {
                    HasSelected = false;
                }
                else
                {
                    HasSelected = true;
                    Status = SelectedItem.GetTypeName() + " Selected";
                }
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Bind EditView to this property
        /// </summary>
        public ModelType CurrentEditItem
        {
            get
            {
                return _CurrentEditItem;
            }
            protected set
            {
                if (IsEditingItemOrAddingNew)
                {
                    OnPropertyChanged();
                    return;
                }
                if (value == _CurrentEditItem) return;
                _CurrentEditItem = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #region Conditions
        public bool IsEnabled
        {
            get
            {
                return _IsEnabled;
            }
            protected set
            {
                _IsEnabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEnabled));
            }
        }
        public bool IsLoading
        {
            get
            {
                return _IsLoading;
            }
            protected set
            {
                _IsLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
        public bool IsEditingItem
        {
            get
            {
                return _IsEditingItem;
            }
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
            get
            {
                return _IsAddingNew;
            }
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
            get
            {
                return _IsSaving;
            }
            set
            {
                _IsSaving = value;
                OnPropertyChanged();
            }
        }
        public bool HasSelected
        {
            get
            {
                return _HasSelected;
            }
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
        #endregion
        #region Commands
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(ap => LoadData(), pp => CanGetData));
        public ICommand NewItemCommand => _NewItemCommand
            ?? (_NewItemCommand = new RelayCommand(ap => AddNew(), pp => CanAddNew));
        public ICommand CancelCommand => _CancelCommand
            ?? (_CancelCommand = new RelayCommand(ap => Cancel(), pp => CanCancel));
        public ICommand CommitCommand => _CommitCommand
            ?? (_CommitCommand = new RelayCommand(ap => Commit(), pp => CanCommit));
        public ICommand EditSelectedCommand => _EditSelectedCommand
            ?? (_EditSelectedCommand = new RelayCommand(ap => EditSelected(), pp => CanEditSelected));
        public ICommand DeleteSelectedCommand => _DeleteSelectedCommand
            ?? (_DeleteSelectedCommand = new RelayCommand(ap => DeleteSelected(), pp => CanDeleteSelected));
        public ICommand SaveAsCommand => _SaveAsCommand
            ?? (_SaveAsCommand = new RelayCommand(ap => SaveAs(), pp => CanSave));
        #endregion
        #region Predicates
        protected virtual bool CanGetData => IsNotSaving && IsNotLoading;
        protected virtual bool CanAddNew => IsNotSaving && IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew && (View?.CanAddNew ?? false);
        protected virtual bool CanCancel => IsAddingNew || (IsEditingItem && (View?.CanCancelEdit ?? false)); //CanCancelEdit requires IEditableItem on model
        protected virtual bool CanCommit => IsNotSaving && IsEditingItemOrAddingNew && !CurrentEditItem.HasErrors;
        protected virtual bool CanDeselect => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew;
        protected virtual bool CanEditSelected => IsNotSaving && IsEnabled && HasSelected && IsNotEditingItemOrAddingNew;
        protected virtual bool CanDeleteSelected => IsNotSaving && IsEnabled && HasSelected && IsNotEditingItemOrAddingNew && (View?.CanRemove ?? false);
        protected virtual bool CanSave => IsNotSaving && IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew;
        #endregion
        #region Actions
        protected abstract Task GetDataAsync();
        protected abstract void SaveAs();
        protected virtual async void LoadData()
        {
            IsEnabled = false;
            IsLoading = true;
            Cancel();
            Status = "Loading Data...";
            Items = new CollectionViewSource();
            OnPropertyChanged(nameof(View));
            Dispose();
            try
            {
                await GetDataAsync();
                View.CustomSort = Sorter;
                OnPropertyChanged(nameof(View));
                IsEnabled = true;
                Status = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, String.Format("Error Loading {0} Data", Name), MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Failed to get data!";
            }
            IsLoading = false;
        }
        protected virtual void AddNew()
        {
            SelectedItem = null;
            IsAddingNew = true; //before view.addnew
            try
            {
                ModelType newItem = View.AddNew() as ModelType;
                _CurrentEditItem = newItem; //Some wack exception happens here sometimes
                OnPropertyChanged(nameof(CurrentEditItem));
                Status = "Adding new " + CurrentEditItem.GetTypeName();
            }
            catch (Exception ex)
            {
                IsAddingNew = false;
                string text = "Uh, I don't know what happened, but maybe try that again...";
                Status = text;
                ExceptionViewer ev = new ExceptionViewer(text, ex);
                ev.Show();
            }
        }
        protected virtual void EditSelected()
        {
            CurrentEditItem = SelectedItem;
            SelectedItem = null;
            IsEditingItem = true; //before view.edit
            View.EditItem(CurrentEditItem);
            CurrentEditItem.IsEditing = true; //after view.edit
            Status = "Editing " + CurrentEditItem.GetTypeName();
        }
        protected virtual void EndEdit()
        {
            IsEditingItem = false;
            IsAddingNew = false;
            CurrentEditItem.IsEditing = false;
            CurrentEditItem = null;
            SelectedItem = null;
            //Refresh all of the buttons
            CommandManager.InvalidateRequerySuggested();
        }
        protected virtual void Cancel()
        {
            if (IsEditingItem)
            {
                View?.CancelEdit();
                EndEdit();
                Status = "Canceled";
            }
            else if (IsAddingNew)
            {
                View?.CancelNew();
                EndEdit();
                Status = "Canceled";
            }
        }
        protected virtual async void Commit()
        {
            if (await SaveChangesAsync())
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
                EndEdit();
            }
        }
        protected virtual async Task<bool> SaveChangesAsync()
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
                //Thrown when there is a concurrency error
                //for now, just rethrow
                throw ex;
            }
            catch (DbUpdateException ex)
            {
                //Thrown when db update fails
                //Examine the intter exceptions for more details and affected objects
                //for now, just rethrow
                throw ex;
            }
            catch (CommitFailedException ex)
            {
                //handle transaction failures here
                throw ex;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                string result = ex.Message + "\n";
                foreach (var e in ex.EntityValidationErrors)
                {
                    foreach (var err in e.ValidationErrors)
                    {
                        result += err.ErrorMessage + "\n";
                    }
                }
                MessageBox.Show(result, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "There was a problem updating the database.";
            }
            catch (Exception ex)
            {
                Status = "There was a problem updating the database.";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return success;
        }
        protected virtual async void DeleteSelected()
        {
            string status = SelectedItem?.GetTypeName() + " Deleted";
            View.Remove(SelectedItem);
            if (await SaveChangesAsync()) Status = status;
            SelectedItem = null;
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
