using System.Data.Entity;

namespace TimekeeperDAL.EF
{
    public partial class TimeKeeperContext : DbContext, ITimeKeeperContext
    {
        public TimeKeeperContext()
            : base("name=TimeKeeperContext")
        {
        }

        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<Task> Tasks { get; set; }
        public virtual IDbSet<Label> Labels { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
        public virtual IDbSet<TimePattern> TimePatterns { get; set; }
        public virtual IDbSet<TimePoint> TimePoints { get; set; }
        public virtual IDbSet<NoteLabeling> NoteLabelings { get; set; }
        public virtual IDbSet<TaskLabeling> TaskLabelings { get; set; }
        public virtual IDbSet<TimePatternLabeling> TimePatternLabelings { get; set; }
        public virtual IDbSet<TaskInclusion> TaskIncludings { get; set; }
        public virtual IDbSet<TaskExclusion> TaskExcludings { get; set; }
        public virtual IDbSet<Allocation> Allocations { get; set; }
        public virtual IDbSet<Resource> Resources { get; set; }
    }
}
