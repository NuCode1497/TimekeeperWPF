using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimekeeperDAL.Models;
using TimekeeperDAL.Repos;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public class NotesViewModel : ObservableObject, IPage
    {
        private ObservableCollection<Note> _notes;
        private Note _selectedNote;
        private ICommand _AddNoteCommand = null;
        private ICommand _GetDataCommand = null;

        public NotesViewModel()
        {
            GetData();
        }

        public string Name => nameof(Notes);

        public ObservableCollection<Note> Notes { get; set; }
        public Note SelectedNote
        {
            get
            {
                return _selectedNote;
            }
            set
            {
                if(value != _selectedNote) _selectedNote = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddNoteCommand => _AddNoteCommand 
            ?? (_AddNoteCommand = new RelayCommand( ap => AddNote(), pp => Notes != null));
        public ICommand GetDataCommand => _GetDataCommand 
            ?? (_GetDataCommand = new RelayCommand( ap => GetData(), pp => true));

        private void AddNote()
        {
            //Get the last ID
            var maxCount = Notes?.Select(sp => sp.NoteID).DefaultIfEmpty().Max() ?? 0;
            //Add after last ID
            Notes?.Add(new Note
            {
                NoteID = ++maxCount,
                NoteDateTime = DateTime.Now,
                NoteText = "Your text here.",
                IsChanged = false
            });
        }

        private async void GetData()
        {
            //Show a busy signal
            Notes = new ObservableCollection<Note>(await new NoteRepo().GetAllAsync());
            OnPropertyChanged(nameof(Notes));
            //Hide the busy signal
        }
    }
}
