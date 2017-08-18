﻿using System;
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
        IDbSet<Allocation> Allocations { get; set; }
        IDbSet<Label> Labels { get; set; }
        IDbSet<Note> Notes { get; set; }
        IDbSet<Resource> Resource { get; set; }
        IDbSet<TimeTask> Tasks { get; set; }
        IDbSet<TaskType> TaskTypes { get; set; }
        IDbSet<TimePattern> TimePatterns { get; set; }
        IDbSet<TimePoint> TimePoints { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
