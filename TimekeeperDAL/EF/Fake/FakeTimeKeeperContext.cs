using System.Data.Entity;
using TimekeeperDAL.Models;

namespace TimekeeperDAL.EF
{
    public class FakeTimeKeeperContext : FakeDbContext, ITimeKeeperContext
    {
        public FakeTimeKeeperContext()
        {
            this.Notes = new FakeNoteSet();
        }
        public IDbSet<Note> Notes { get; private set; }
    }
}
