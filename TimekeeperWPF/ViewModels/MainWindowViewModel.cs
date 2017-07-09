using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperWPF.Models;
using TimekeeperWPF.Repos;

namespace TimekeeperWPF.ViewModels
{
    public class MainWindowViewModel
    {
        public IList<Note> Notes { get; set; }

        public MainWindowViewModel()
        {
            Notes = new ObservableCollection<Note>(new NoteRepo().GetAll());
        }
    }
}
