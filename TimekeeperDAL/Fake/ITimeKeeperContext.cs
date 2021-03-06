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
        IDbSet<TimeTaskAllocation> Allocations { get; set; }
        IDbSet<Label> Labels { get; set; }
        IDbSet<Note> Notes { get; set; }
        IDbSet<Resource> Resources { get; set; }
        IDbSet<TimeTask> TimeTasks { get; set; }
        IDbSet<TaskType> TaskTypes { get; set; }
        IDbSet<TimePattern> TimePatterns { get; set; }
        IDbSet<TimeTaskFilter> Filters { get; set; }
        IDbSet<Filterable> Filterables { get; set; }
        IDbSet<LabeledEntity> LabeledEntities { get; set; }
        IDbSet<TypedLabeledEntity> TypedLabeledEntities { get; set; }
        IDbSet<Labelling> Labellings { get; set; }
        IDbSet<CheckIn> CheckIns { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}
