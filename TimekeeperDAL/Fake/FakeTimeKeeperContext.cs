﻿using System;
using System.Data.Entity;

namespace TimekeeperDAL.EF
{
    public class FakeTimeKeeperContext : FakeDbContext, ITimeKeeperContext
    {
        public FakeTimeKeeperContext()
        {
            Notes = new FakeNoteSet()
            {
                new Note { DateTime = DateTime.Now, Text = "This is fake test data." },
                new Note { DateTime = DateTime.Now, Text = "Testing 1 2 3." },
                new Note { DateTime = DateTime.Now, Text = "Did you ever hear the tragedy of Darth Plagueis The Wise? I thought not. It’s not a story the Jedi would tell you. It’s a Sith legend." },
                new Note { DateTime = DateTime.Now, Text = "Testing 4 5 6." }
            };
            TaskTypes = new FakeTaskTypeSet()
            {
                new TaskType { Name = "Note" },
                new TaskType { Name = "Work" },
                new TaskType { Name = "Play" },
                new TaskType { Name = "Chore" },
                new TaskType { Name = "Eat" },
                new TaskType { Name = "Sleep" },
            };
        }
        public virtual IDbSet<TimeTaskAllocation> Allocations { get; set; }
        public virtual IDbSet<Label> Labels { get; set; }
        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<Resource> Resources { get; set; }
        public virtual IDbSet<TimeTask> TimeTasks { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
        public virtual IDbSet<TimePattern> TimePatterns { get; set; }
        public virtual IDbSet<TimeTaskFilter> Filters { get; set; }
        public virtual IDbSet<Filterable> Filterables { get; set; }
        public virtual IDbSet<LabeledEntity> LabeledEntities { get; set; }
        public virtual IDbSet<TypedLabeledEntity> TypedLabeledEntities { get; set; }
        public virtual IDbSet<Labelling> Labellings { get; set; }
        public virtual IDbSet<CheckIn> CheckIns { get; set; }
    }
}
