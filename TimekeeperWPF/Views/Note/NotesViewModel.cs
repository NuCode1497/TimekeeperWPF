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
        private Note _selectedNote;
        private ICommand _NewNoteCommand = null;
        private ICommand _GetDataCommand = null;
        private ICommand _SaveCommand = null;
        private ICommand _CancelCommand = null;
        private ICommand _DeleteCommand = null;
        private String _status = "Ready";
        private enum EditMode
        {
            Update,
            New,
            None
        }
        private EditMode _editMode = EditMode.None;

        public NotesViewModel()
        {
            Context = new TimeKeeperContext();
            GetData();
        }
        public override string Name => nameof(Notes);
        public CollectionViewSource Notes { get; private set; } = new CollectionViewSource();
        public ObservableCollection<Note> Source => Notes.Source as ObservableCollection<Note>;
        public ListCollectionView View => Notes.View as ListCollectionView;
        public Note SelectedNote
        {
            get
            {
                return _selectedNote;
            }
            set
            {
                if (value == _selectedNote) return;
                _selectedNote = value;
                if (SelectedNote == null) _editMode = EditMode.None;
                else _editMode = EditMode.Update;
                Status = "Ready";
                OnPropertyChanged();
            }
        }
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
        public ICommand NewNoteCommand => _NewNoteCommand
            ?? (_NewNoteCommand = new RelayCommand(ap => NewNote(), pp => Notes != null));
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(ap => GetData(), pp => true));
        public ICommand SaveCommand => _SaveCommand
            ?? (_SaveCommand = new RelayCommand(ap => Save(), pp => CanSave()));
        public ICommand CancelCommand => _CancelCommand
            ?? (_CancelCommand = new RelayCommand(ap => SelectedNote = null, pp => _editMode != EditMode.None));
        public ICommand DeleteCommand => _DeleteCommand
            ?? (_DeleteCommand = new RelayCommand(ap => Delete(), pp => _editMode == EditMode.Update));

        private void Delete()
        {
            Context.Notes.Remove(SelectedNote);
            Save();
        }
        private void Save()
        {
            switch (_editMode)
            {
                case EditMode.New:
                    Context.Notes.Add(SelectedNote);
                    SaveChanges();
                    break;
                case EditMode.Update:
                    SaveChanges();
                    break;
                case EditMode.None:
                    break;
            }
            SelectedNote = null;
        }
        private bool CanSave()
        {
            if (_editMode != EditMode.None && !SelectedNote.HasErrors) return true;
            else return false;
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
                _editMode = EditMode.None;
            }
            catch(Exception ex)
            {
                Status = "There was a problem updating the database.";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _editMode = EditMode.None;
            }
        }

        private void NewNote(string text = "Your text here.")
        {
            //Get the last ID
            var maxCount = Source?.Select(sp => sp.NoteID).DefaultIfEmpty().Max() ?? 0;
            SelectedNote = new Note
            {
                NoteID = ++maxCount,
                NoteDateTime = DateTime.Now,
                NoteText = text,
                IsChanged = false
            };
            _editMode = EditMode.New;
        }

        private async void GetData()
        {
            //TODO: Show a busy signal
            await Context.Notes.LoadAsync();
            Notes.Source = Context.Notes.Local;
            //Notes.SortDescriptions.Add(new SortDescription(nameof(Note.NoteDateTime), ListSortDirection.Ascending));
            View.CustomSort = new NoteSorter();
            var a = Context.Notes.ToList();
            OnPropertyChanged(nameof(Notes));
            //TODO: Hide the busy signal
        }
    }
}
