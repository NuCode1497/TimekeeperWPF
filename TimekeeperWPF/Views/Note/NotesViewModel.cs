using TimekeeperDAL.EF;
using TimekeeperDAL.Models;

namespace TimekeeperWPF
{
    public class NotesViewModel : ViewModel<Note,TimeKeeperContext>
    {
        public NotesViewModel() : base()
        {
            Sorter = new NoteDateTimeSorter();
            GetData();
        }
        public override string Name => "Notes";
    }
}
