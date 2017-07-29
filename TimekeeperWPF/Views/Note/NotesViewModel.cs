using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TimekeeperDAL.Models;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;
using System.Data.Entity;
using System.Diagnostics;
using System.Windows;
using System.Data.Entity.Infrastructure;
using System.Collections;

namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note>
    {
        #region Fields
        private bool _IsEnabled = true;
        private bool _IsLoading = false;
        private bool _IsEditingItem = false;
        private bool _IsAddingNew = false;
        private bool _HasSelected = false;
        private Note _SelectedItem;
        private Note _CurrentEditItem;
        private Note _CurrentAddItem;
        private ICommand _GetDataCommand = null;
        private ICommand _NewItemCommand = null;
        private ICommand _CancelCommand = null;
        private ICommand _CommitCommand = null;
        private ICommand _DeselectCommand = null;
        private ICommand _EditSelectedCommand = null;
        private ICommand _DeleteSelectedCommand = null;
        #endregion
        protected IComparer Sorter = new Comparer(System.Globalization.CultureInfo.CurrentCulture);
        public NotesViewModel()
        {
            Sorter = new NoteDateTimeSorter();
            GetData();
        }
        #region Properties
        public override string Name => "Notes";
        public CollectionViewSource Items { get; private set; } = new CollectionViewSource();
        public ObservableCollection<Note> Source => Items.Source as ObservableCollection<Note>;
        public ListCollectionView View => Items.View as ListCollectionView;
        public Note SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
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
                    Status = nameof(Note) + " Selected";
                }
                OnPropertyChanged();
            }
        }
        public Note CurrentEditItem
        {
            get
            {
                return _CurrentEditItem;
            }
            private set
            {
                if (value == _CurrentEditItem) return;
                _CurrentEditItem = value;
                if (CurrentEditItem == null)
                {
                    //Exit Edit Mode
                    View.CancelEdit();
                    IsEditingItem = false;
                }
                else
                {
                    //Enter Edit Mode
                    IsEditingItem = true;
                    View.EditItem(CurrentEditItem);
                    Status = "Editing " + nameof(Note);
                }
                OnPropertyChanged();
            }
        }
        public Note CurrentAddItem
        {
            get
            {
                return _CurrentAddItem;
            }
            private set
            {
                if (value == _CurrentAddItem) return;
                _CurrentAddItem = value;
                if (CurrentAddItem == null)
                {
                    //Exit AddNew Mode
                    View.CancelNew();
                    IsAddingNew = false;
                }
                else
                {
                    //Enter AddNew Mode
                    IsAddingNew = true;
                    Status = "Adding new " + nameof(Note);
                }
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
        private bool CanGetData => IsNotLoading && IsNotEditingItemOrAddingNew;
        private bool CanAddNew => IsEnabled && IsNotLoading && IsNotEditingItemOrAddingNew && (View?.CanAddNew ?? false);
        private bool CanCancel => IsEnabled && (IsAddingNew || (IsEditingItem && (View?.CanCancelEdit ?? false))); //CanCancelEdit requires IEditableItem on model
        private bool CanCommit => IsEnabled && ((IsAddingNew && !CurrentAddItem.HasErrors) || (IsEditingItem && !CurrentEditItem.HasErrors));
        private bool CanDeselect => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew;
        private bool CanEditSelected => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew;
        private bool CanDeleteSelected => IsEnabled && HasSelected && IsNotEditingItemOrAddingNew && (View?.CanRemove ?? false);
        #endregion
        #region Actions
        private async void GetData()
        {
            //TODO: Show a busy signal
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
                Context = new TimeKeeperContext();
                await Context.Set(typeof(Note)).LoadAsync();
                Items.Source = Context.Set(typeof(Note)).Local;
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
            //TODO: Hide the busy signal
        }
        private void AddNew()
        {
            Deselect();
            CurrentAddItem = View.AddNew() as Note;
        }
        private void Cancel()
        {
            if (IsAddingNew) CurrentAddItem = null;
            if (IsEditingItem) CurrentEditItem = null;
            Status = "Cancelled";
        }
        private async void Commit()
        {
            if(IsAddingNew)
            {
                Context.Entry(CurrentAddItem).State = EntityState.Added;
                if (await SaveChangesAsync())
                {
                    View.CommitNew();
                    SelectedItem = Context.Entry(CurrentAddItem).Entity;
                    CurrentAddItem = null;
                    Status = nameof(Note) + " Added";
                }
            }
            if(IsEditingItem)
            {
                Context.Entry(CurrentEditItem).State = EntityState.Modified;
                if (await SaveChangesAsync())
                {
                    View.CommitEdit();
                    SelectedItem = Context.Entry(CurrentEditItem).Entity;
                    CurrentEditItem = null;
                    Status = nameof(Note) + " Modified";
                }
            }
        }
        private async Task<bool> SaveChangesAsync()
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
        private void Deselect()
        {
            SelectedItem = null;
            Status = "Ready";
        }
        private void EditSelected()
        {
            CurrentEditItem = SelectedItem;
        }
        private async void DeleteSelected()
        {
            Context.Entry(SelectedItem).State = EntityState.Deleted;
            Deselect();
            if (await SaveChangesAsync()) Status = nameof(Note) + " Deleted";
        }
        #endregion
    }
}
