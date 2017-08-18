using System;
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
        public virtual IDbSet<Allocation> Allocations { get; set; }
        public virtual IDbSet<Label> Labels { get; set; }
        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<Resource> Resource { get; set; }
        public virtual IDbSet<TimeTask> Tasks { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
        public virtual IDbSet<TimePattern> TimePatterns { get; set; }
        public virtual IDbSet<TimePoint> TimePoints { get; set; }
    }
}
