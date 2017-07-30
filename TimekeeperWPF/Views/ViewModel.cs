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
using TimekeeperDAL.Models;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public abstract class ViewModel<ModelType,ContextType> : ObservableObject, IPage, IDisposable 
        where ModelType: EntityBase, new() 
        where ContextType: DbContext, new()
    {
        #region Fields
        private String _status = "Ready";
        protected ContextType Context;
        private bool _IsEnabled = true;
        private bool _IsLoading = false;
        private bool _IsEditingItem = false;
        private bool _IsAddingNew = false;
        private bool _HasSelected = false;
        private ModelType _SelectedItem;
        private ModelType _CurrentEditItem;
        private ICommand _GetDataCommand = null;
        private ICommand _NewItemCommand = null;
        private ICommand _CancelCommand = null;
        private ICommand _CommitCommand = null;
        private ICommand _DeselectCommand = null;
        private ICommand _EditSelectedCommand = null;
        private ICommand _DeleteSelectedCommand = null;
        protected IComparer Sorter;
        #endregion
        public ViewModel()
        {
        }
        #region Properties
        public abstract string Name { get; }
        public String Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public CollectionViewSource Items { get; private set; } = new CollectionViewSource();
        public ObservableCollection<ModelType> Source => Items.Source as ObservableCollection<ModelType>;
        public ListCollectionView View => Items.View as ListCollectionView;
        public ModelType SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                //Selecting something else is allowed while editing, however the CurrentEditItem will not change
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
                    Status = nameof(ModelType) + " Selected";
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
            private set
            {
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
            private set
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
            private set
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
            private set
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
            private set
            {
                _IsAddingNew = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotAddingNew));
                OnPropertyChanged(nameof(IsEditingItemOrAddingNew));
                OnPropertyChanged(nameof(IsNotEditingItemOrAddingNew));
            }
        }
        public bool HasSelected
        {
            get
            {
                return _HasSelected;
            }
            set
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
        public bool HasNotSelected => !HasSelected;
        #endregion
        #region Commands
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(ap => GetData(), pp => CanGetData));
        public ICommand NewItemCommand => _NewItemCommand
            ?? (_NewItemCommand = new RelayCommand(ap => AddNew(), pp => CanAddNew));
        public ICommand CancelCommand => _CancelCommand
            ?? (_CancelCommand = new RelayCommand(ap => Cancel(), pp => CanCancel));
        public ICommand CommitCommand => _CommitCommand
            ?? (_CommitCommand = new RelayCommand(ap => Commit(), pp => CanCommit));
        public ICommand DeselectCommand => _DeselectCommand
            ?? (_DeselectCommand = new RelayCommand(ap => Deselect(), pp => CanDeselect));
        public ICommand EditSelectedCommand => _EditSelectedCommand
            ?? (_EditSelectedCommand = new RelayCommand(ap => EditSelected(), pp => CanEditSelected));
        public ICommand DeleteSelectedCommand => _DeleteSelectedCommand
            ?? (_DeleteSelectedCommand = new RelayCommand(ap => DeleteSelected(), pp => CanDeleteSelected));
        #endregion
        #region Predicates
        protected bool CanGetData => IsNotLoading && IsNotEditingItemOrAddingNew;
        protected bool CanAddNew => IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew && (View?.CanAddNew ?? false);
        protected bool CanCancel => IsAddingNew || (IsEditingItem && (View?.CanCancelEdit ?? false)); //CanCancelEdit requires IEditableItem on model
        protected bool CanCommit => IsEditingItemOrAddingNew && !CurrentEditItem.HasErrors;
        protected bool CanDeselect => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew;
        protected bool CanEditSelected => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew;
        protected bool CanDeleteSelected => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew && (View?.CanRemove ?? false);
        #endregion
        #region Actions
        protected async void GetData()
        {
            IsEnabled = false;
            IsLoading = true;
            Cancel();
            Deselect();
            Status = "Loading Data...";
            Items = new CollectionViewSource();
            OnPropertyChanged(nameof(View));
            Context?.Dispose();
            try
            {
                //await Task.Delay(3000);
                //throw new Exception("testing get data error");
                Context = new ContextType();
                await Context.Set(typeof(ModelType)).LoadAsync();
                Items.Source = Context.Set(typeof(ModelType)).Local;
                View.CustomSort = Sorter;
                OnPropertyChanged(nameof(View));
                Deselect();
                IsEnabled = true;
                Status = "Ready";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Data", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Failed to get data!";
            }
            IsLoading = false;
        }
        protected void AddNew()
        {
            CurrentEditItem = View.AddNew() as ModelType;
            IsAddingNew = true;
            Status = "Adding new " + nameof(ModelType);
        }
        protected void Cancel()
        {
            if(IsEditingItem)
            {
                View?.CancelEdit();
                IsEditingItem = false;
            }
            if(IsAddingNew)
            {
                View?.CancelNew();
                IsAddingNew = false;
            }
            CurrentEditItem = null;
            Status = "Cancelled";
        }
        protected async void Commit()
        {
            if(IsAddingNew)
            {
                Context.Entry(CurrentEditItem).State = EntityState.Added;
                if (await SaveChangesAsync())
                {
                    View.CommitNew();
                    CurrentEditItem = null;
                    IsAddingNew = false;
                    Status = nameof(ModelType) + " Added";
                }
            }
            if(IsEditingItem)
            {
                Context.Entry(CurrentEditItem).State = EntityState.Modified;
                if (await SaveChangesAsync())
                {
                    View.CommitEdit();
                    CurrentEditItem = null;
                    IsEditingItem = false;
                    Status = nameof(ModelType) + " Modified";
                }
            }
            CommandManager.InvalidateRequerySuggested();
        }
        protected async Task<bool> SaveChangesAsync()
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
        protected void Deselect()
        {
            SelectedItem = null;
            Status = "Ready";
        }
        protected void EditSelected()
        {
            CurrentEditItem = SelectedItem;
            View.EditItem(CurrentEditItem);
            IsEditingItem = true;
            Status = "Editing " + nameof(ModelType);
        }
        protected async void DeleteSelected()
        {
            Context.Entry(SelectedItem).State = EntityState.Deleted;
            if (await SaveChangesAsync())
            {
                Deselect();
                Status = nameof(ModelType) + " Deleted";
            }
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
                    Context.Dispose();
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
