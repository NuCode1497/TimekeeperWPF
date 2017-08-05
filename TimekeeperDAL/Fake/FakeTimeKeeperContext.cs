using System.Data.Entity;

namespace TimekeeperDAL.EF
{
    public class FakeTimeKeeperContext : FakeDbContext, ITimeKeeperContext
    {
        public FakeTimeKeeperContext()
        {
        }
        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
    }
}
