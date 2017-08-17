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
        protected override async System.Threading.Tasks.Task<ObservableCollection<Note>> GetDataAsync()
        {
            await System.Threading.Tasks.Task.Delay(0);
            //throw new Exception("testing get data error");
            Context = new FakeTimeKeeperContext();
            NoteTypesCollection = new CollectionViewSource();
            NoteTypesCollection.Source = Context.TaskTypes.Local;
            OnPropertyChanged(nameof(NoteTypesView));
            return Context.Notes.Local;
        }
    }
}
