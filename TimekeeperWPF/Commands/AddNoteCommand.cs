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
        private readonly DateTime _noteDateTime;
        private readonly string _noteText = "local";
        public AddNoteCommand(IList<Note> notes, DateTime noteDateTime, string noteText)
        {
            _notes = notes;
            _noteDateTime = noteDateTime;
            _noteText = noteText;
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
                NoteDateTime = _noteDateTime,
                NoteText = _noteText,
                IsChanged = false
            });
        }
    }
}
