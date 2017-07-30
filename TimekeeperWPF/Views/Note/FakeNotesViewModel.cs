using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperDAL.Models;
namespace TimekeeperWPF
{
    public class FakeNotesViewModel : ViewModel<Note, FakeTimeKeeperContext>
    {
        public FakeNotesViewModel() : base()
        {
        }
        public override string Name => "Notes";
        protected override async Task<ObservableCollection<Note>> GetDataAsync()
        {
            await Task.Delay(0);
            //await Task.Delay(3000);
            //throw new Exception("testing get data error");
            if (Context == null)
            {
                Context = new FakeTimeKeeperContext()
                {
                    Notes =
                    {
                        new Note { NoteDateTime = DateTime.Now, NoteText = "This is fake test data."},
                        new Note { NoteDateTime = DateTime.Now, NoteText = "Testing 1 2 3."},
                        new Note { NoteDateTime = DateTime.Now, NoteText = "Did you ever hear the tragedy of Darth Plagueis The Wise? I thought not. It’s not a story the Jedi would tell you. It’s a Sith legend."},
                    }
                };
            }
            return Context.Notes.Local;
        }
    }
}
