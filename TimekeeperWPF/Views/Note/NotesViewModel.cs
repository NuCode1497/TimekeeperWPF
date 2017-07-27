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

namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note>
    {
        private bool _IsEnabled = true;
        private bool _IsLoading = false;
        private bool _IsEditing = false;
        private bool _HasSelected = false;
        private Note _SelectedNote;
        private Note _EditingNote;
        private EditMode _editMode = EditMode.None;
        private enum EditMode
        {
            New, Update, None
        }
        private ICommand _NewNoteCommand = null;
        private ICommand _GetDataCommand = null;
        private ICommand _SaveEditCommand = null;
        private ICommand _CancelEditCommand = null;
        private ICommand _EditSelectedCommand = null;
        private ICommand _DeleteSelectedCommand = null;
        private ICommand _UnSelectCommand = null;


        public NotesViewModel()
        {
            GetData();
        }
        public override string Name => "Notes";
        public CollectionViewSource Notes { get; private set; } = new CollectionViewSource();
        public ObservableCollection<Note> Source => Notes.Source as ObservableCollection<Note>;
        public ListCollectionView View => Notes.View as ListCollectionView;
        public Note SelectedNote
        {
            get
            {
                return _SelectedNote;
            }
            set
            {
                //Item must not be itself and must be in Source
                if ((value == _SelectedNote) || (!Source?.Contains(value) ?? false)) return;
                _SelectedNote = value;
                if (SelectedNote == null)
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
        public Note EditingNote
        {
            get
            {
                return _EditingNote;
            }
            private set
            {
                if (value == _EditingNote) return;
                _EditingNote = value;
                if (EditingNote == null)
                {
                    IsEditing = false;
                    _editMode = EditMode.None;
                }
                else
                {
                    IsEditing = true;
                    _editMode = EditMode.Update;
                    Status = "Editing " + nameof(Note);
                }
                OnPropertyChanged();
            }
        }
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
        public bool IsEditing
        {
            get
            {
                return _IsEditing;
            }
            private set
            {
                _IsEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing));
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
        public bool IsNotEnabled => !IsEnabled;
        public bool IsNotLoading => !IsLoading;
        public bool IsNotEditing => !IsEditing;
        public bool HasNotSelected => !HasSelected;
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(ap => GetData(), pp => CanGetData()));
        public ICommand NewNoteCommand => _NewNoteCommand
            ?? (_NewNoteCommand = new RelayCommand(ap => NewNote(), pp => CanCreateNew()));
        public ICommand SaveEditCommand => _SaveEditCommand
            ?? (_SaveEditCommand = new RelayCommand(ap => SaveEdit(), pp => CanSave()));
        public ICommand CancelEditCommand => _CancelEditCommand
            ?? (_CancelEditCommand = new RelayCommand(ap => StopEditing(), pp => CanCancelEdit()));
        public ICommand EditSelectedCommand => _EditSelectedCommand
            ?? (_EditSelectedCommand = new RelayCommand(ap => EditSelected(), pp => CanEditSelected()));
        public ICommand DeleteSelectedCommand => _DeleteSelectedCommand
            ?? (_DeleteSelectedCommand = new RelayCommand(ap => DeleteSelected(), pp => CanEditSelected()));
        public ICommand UnSelectCommand => _UnSelectCommand
            ?? (_UnSelectCommand = new RelayCommand(ap => StopSelecting(), pp => CanUnSelect()));

        private bool CanGetData()
        {
            return IsNotLoading;
        }
        private bool CanCreateNew()
        {
            return IsEnabled && IsNotEditing && IsNotLoading && Source != null;
        }
        private bool CanSave()
        {
            return IsEnabled && IsEditing && IsNotLoading && !EditingNote.HasErrors;
        }
        private bool CanCancelEdit()
        {
            return IsEnabled && IsEditing;
        }
        private bool CanEditSelected()
        {
            return HasSelected && IsEnabled && IsNotEditing && IsNotLoading;
        }
        private bool CanUnSelect()
        {
            return IsEnabled && HasSelected;
        }
        private async void GetData()
        {
            //TODO: Show a busy signal
            IsEnabled = false;
            IsLoading = true;
            StopSelecting();
            StopEditing();
            Notes = new CollectionViewSource();
            OnPropertyChanged(nameof(View));
            Context?.Dispose();
            try
            {
                //await Task.Delay(3000);
                //throw new Exception("testing get data error");
                Context = new TimeKeeperContext();
                await Context.Notes.LoadAsync();
                Notes.Source = Context.Notes.Local;
                //Notes.SortDescriptions.Add(new SortDescription(nameof(Note.NoteDateTime), ListSortDirection.Ascending));
                View.CustomSort = new NoteDateTimeSorter();
                OnPropertyChanged(nameof(View));
                StopSelecting();
                IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Loading Data",MessageBoxButton.OK,MessageBoxImage.Error);
                Status = "Failed to get data!";
            }
            IsLoading = false;
            //TODO: Hide the busy signal
        }
        private void NewNote()
        {
            StopSelecting();
            //Get the last ID
            var maxCount = Source?.Select(sp => sp.NoteID).DefaultIfEmpty().Max() ?? 0;
            EditingNote = new Note
            {
                NoteID = ++maxCount,
                IsChanged = false
            };
            _editMode = EditMode.New;
        }
        private void SaveEdit()
        {
            switch (_editMode)
            {
                case EditMode.New:
                    Context.Entry(EditingNote).State = EntityState.Added;
                    SaveChanges();
                    break;
                case EditMode.Update:
                    Context.Entry(EditingNote).State = EntityState.Modified;
                    SaveChanges();
                    break;
                case EditMode.None:
                    break;
            }
            SelectedNote = Context.Entry(EditingNote).Entity;
            StopEditing();
        }
        private void StopEditing()
        {
            EditingNote = null;
        }
        private async void SaveChanges()
        {
            try
            {
                int x = await Context.SaveChangesAsync();
                Status = String.Format("{0} {1} affected.", x , x == 1 ? "row" : "rows");
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
                foreach(var e in ex.EntityValidationErrors)
                {
                    foreach(var err in e.ValidationErrors)
                    {
                        result += err.ErrorMessage + "\n";
                    }
                }
                MessageBox.Show(result,"Validation Error",MessageBoxButton.OK,MessageBoxImage.Error);
                Status = "There was a problem updating the database.";
            }
            catch(Exception ex)
            {
                Status = "There was a problem updating the database.";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EditSelected()
        {
            EditingNote = new Note();
            EditingNote.NoteID = SelectedNote.NoteID;
            EditingNote.NoteDateTime = SelectedNote.NoteDateTime;
            EditingNote.NoteText = SelectedNote.NoteText;
            EditingNote.RowVersion = SelectedNote.RowVersion;
            EditingNote.IsChanged = false;
            _editMode = EditMode.Update;
        }
        private void DeleteSelected()
        {
            //Context.Notes.Remove(SelectedNote);
            Context.Entry(SelectedNote).State = EntityState.Deleted;
            StopSelecting();
            SaveChanges();
        }
        private void StopSelecting()
        {
            SelectedNote = null;
        }
    }
}
