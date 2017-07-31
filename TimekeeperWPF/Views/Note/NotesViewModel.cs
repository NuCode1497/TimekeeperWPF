using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperDAL.Models;
namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note,TimeKeeperContext>
    {
        public NotesViewModel() : base()
        {
            Sorter = new NoteDateTimeSorter();
        }
        public override string Name => "Notes";
        protected override async Task<ObservableCollection<Note>> GetDataAsync()
        {
            await Task.Delay(6000);
            Context = new TimeKeeperContext();
            await Context.Notes.LoadAsync();
            return Context.Notes.Local;
        }
    }
}
