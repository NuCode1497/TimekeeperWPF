using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using TimekeeperDAL.EF;
namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note,TimeKeeperContext>
    {
        public NotesViewModel() : base()
        {
            Sorter = new DateTimeSorter();
            LoadData();
        }
        public override string Name => nameof(Context.Notes);
        public CollectionViewSource NoteTypesCollection { get; set; }
        public ObservableCollection<TaskType> NoteTypesSource => NoteTypesCollection?.Source as ObservableCollection<TaskType>;
        public ListCollectionView NoteTypesView => NoteTypesCollection?.View as ListCollectionView;
        protected override async Task<ObservableCollection<Note>> GetDataAsync()
        {
            //await Task.Delay(6000);
            Context = new TimeKeeperContext();
            NoteTypesCollection = new CollectionViewSource();
            await Context.Notes.LoadAsync();
            await Context.TaskTypes.LoadAsync();
            NoteTypesCollection.Source = Context.TaskTypes.Local;
            OnPropertyChanged(nameof(NoteTypesView));
            return Context.Notes.Local;
        }
        protected override void EditSelected()
        {
            base.EditSelected();
            PreSelectNoteType();
        }
        protected override void AddNew()
        {
            base.AddNew();
            PreSelectNoteType();
        }
        private void PreSelectNoteType()
        {
            NoteTypesView?.MoveCurrentTo(NoteTypesSource.DefaultIfEmpty(NoteTypesSource[0]).First(t => t.Type == CurrentEditItem?.Type));
        }
    }
}
