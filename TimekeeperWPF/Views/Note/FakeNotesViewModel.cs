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
            Context = new FakeTimeKeeperContext();
            await Context.Notes.LoadAsync();
            Items.Source = Context.Notes.Local;

            //Load TaskTypes stuff
            NoteTypesCollection = new CollectionViewSource();
            await Context.TaskTypes.LoadAsync();
            NoteTypesCollection.Source = Context.TaskTypes.Local;
            NoteTypesView.CustomSort = BasicSorter;
            OnPropertyChanged(nameof(NoteTypesView));

        }
    }
}
