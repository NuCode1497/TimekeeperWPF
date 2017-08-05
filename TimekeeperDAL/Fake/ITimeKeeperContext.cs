using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace TimekeeperDAL.EF
{
    //https://romiller.com/2012/02/14/testing-with-a-fake-dbcontext/
    public interface ITimeKeeperContext
    {
        IDbSet<Note> Notes { get; }
        IDbSet<TaskType> TaskTypes { get; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
