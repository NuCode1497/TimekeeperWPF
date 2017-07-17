using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;
using TimekeeperDAL.Repos;
using System.Windows.Input;
using System.Collections.ObjectModel;
using TimekeeperWPF.ViewModels;

namespace TimekeeperWPF.Commands
{
    public class GetDataCommand : TKCommand
    {
        private readonly NoteViewModel _nvm; 
        public GetDataCommand(NoteViewModel nvm)
        {
            _nvm = nvm;
        }
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            _nvm.Notes = new ObservableCollection<Note>(new NoteRepo().GetAll());
        }
    }
}
