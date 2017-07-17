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
        public NoteViewModel()
        {
            //Notes = new ObservableCollection<Note>(new NoteRepo().GetAll());
            Notes = new ObservableCollection<Note>();
        }

        private ICommand _AddNoteCommand = null;
        public ICommand AddNoteCmd => _AddNoteCommand ?? (_AddNoteCommand = new AddNoteCommand(Notes));
        private ICommand _GetDataCommand = null;
        public ICommand GetDataCmd => _GetDataCommand ?? (_GetDataCommand = new GetDataCommand(this));
    }
}
