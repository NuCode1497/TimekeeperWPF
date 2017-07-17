using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;
using TimekeeperWPF.Views;
using TimekeeperWPF.ViewModels;

namespace TimekeeperWPF.Commands
{
    public class AddNoteCommand : TKCommand
    {
        private readonly IList<Note> _notes;
        public AddNoteCommand(IList<Note> notes)
        {
            _notes = notes;
        }
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            //Get the last ID
            var maxCount = _notes?.Select(x => x.NoteID).DefaultIfEmpty().Max() ?? 0;
            //Add after last ID
            _notes?.Add(new Note
            {
                NoteID = ++maxCount,
                NoteDateTime = DateTime.Now,
                NoteText = "Your text here.",
                IsChanged = false
            });
        }
    }
}
