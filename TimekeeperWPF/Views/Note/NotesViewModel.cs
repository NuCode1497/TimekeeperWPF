using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimekeeperDAL.Models;
using TimekeeperDAL.Repos;

namespace TimekeeperWPF
{
    public class NotesViewModel
    {
        public IList<Note> Notes { get; set; }
        public Note SelectedNote { get; set; }

        private ICommand _AddNoteCommand = null;
        public ICommand AddNoteCmd => _AddNoteCommand ?? (_AddNoteCommand = new RelayCommand( ap =>
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
        }, pp => Notes != null));

        private ICommand _GetDataCommand = null;
        public ICommand GetDataCmd => _GetDataCommand ?? (_GetDataCommand = new RelayCommand( ap => 
        {
            Notes = new ObservableCollection<Note>(new NoteRepo().GetAll());
        }, pp => true));
    }
}
