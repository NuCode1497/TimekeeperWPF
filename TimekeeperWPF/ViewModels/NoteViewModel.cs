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

        private ICommand _AddNoteCommand = null;
        private ICommand _GetDataCommand = null;
        public ICommand AddNoteCmd => _AddNoteCommand ?? (_AddNoteCommand = new RelayCommand(
            x => AddNote(), x => Notes != null));
        public ICommand GetDataCmd => _GetDataCommand ?? (_GetDataCommand = new RelayCommand(
            x => { Notes = new ObservableCollection<Note>(new NoteRepo().GetAll()); }, x => true));

        private void AddNote()
        {
            //Get the last ID
            var maxCount = Notes?.Select(x => x.NoteID).DefaultIfEmpty().Max() ?? 0;
            //Add after last ID
            Notes?.Add(new Note
            {
                NoteID = ++maxCount,
                NoteDateTime = DateTime.Now,
                NoteText = "Your text here.",
                IsChanged = false
            });
        }
    }
}
