using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TimekeeperDAL.Models;
using TimekeeperDAL.Repos;
using TimekeeperWPF.Commands;

namespace TimekeeperWPF.ViewModels
{
    public class NoteViewModel
    {
        public IList<Note> Notes { get; set; }
        public DateTime NoteDateTime { get; set; }
        public string NoteText { get; set; }
        public NoteViewModel()
        {
            //Notes = new ObservableCollection<Note>(new NoteRepo().GetAll());
            Notes = new ObservableCollection<Note>();
            NoteDateTime = DateTime.Now;
            NoteText = "Something.";
        }

        private ICommand _AddNoteCommand = null;
        public ICommand AddNoteCmd => _AddNoteCommand ?? (_AddNoteCommand = new AddNoteCommand(Notes, NoteDateTime, NoteText));
    }
}
