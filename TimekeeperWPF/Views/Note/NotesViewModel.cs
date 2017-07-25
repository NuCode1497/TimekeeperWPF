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

namespace TimekeeperWPF
{
    public class NotesViewModel : ObservableObject, IPage
    {
        private TimeKeeperContext Context;
        private Note _selectedNote;
        private ICommand _AddNoteCommand = null;
        private ICommand _GetDataCommand = null;
        private ICommand _CommitUpdatesCommand = null;

        public NotesViewModel()
        {
            Context = new TimeKeeperContext();
            GetData();
        }
        public string Name => nameof(Notes);
        public ObservableCollection<Note> Notes { get; private set; }
        public Note SelectedNote
        {
            get
            {
                return _selectedNote;
            }
            set
            {
                if (value != _selectedNote) _selectedNote = value;
                OnPropertyChanged();
            }
        }
        public ICommand AddNoteCommand => _AddNoteCommand
            ?? (_AddNoteCommand = new RelayCommand(ap => AddNote(), pp => Notes != null));
        public ICommand GetDataCommand => _GetDataCommand
            ?? (_GetDataCommand = new RelayCommand(ap => GetData(), pp => true));
        public ICommand CommitUpdatesCommand => _CommitUpdatesCommand
            ?? (_CommitUpdatesCommand = new RelayCommand(ap => CommitUpdates(), pp => true));

        private void CommitUpdates()
        {
        }

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
            Notes = new ObservableCollection<Note>(
                await (from n in Context.Notes
                       orderby n.NoteDateTime
                       select n)
                       .ToListAsync());

            OnPropertyChanged(nameof(Notes));
            //Hide the busy signal
        }
    }
}
