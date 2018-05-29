using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Windows.Data;
using TimekeeperDAL.EF;
namespace TimekeeperWPF
{
    public class FakeNotesViewModel : NotesViewModel
    {
        public FakeNotesViewModel() : base()
        {
        }
        public override string Name => "Fake " + base.Name;
        protected override async Task GetDataAsync()
        {
            ClearUndos();
            await Task.Delay(0);
            Context = new FakeTimeKeeperContext();
            Items.Source = Context.Notes.Local;
        }
    }
}
