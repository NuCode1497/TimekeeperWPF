using System.Data.Entity;

namespace TimekeeperDAL.EF
{
    public partial class TimeKeeperContext : DbContext, ITimeKeeperContext
    {
        public TimeKeeperContext()
            : base("name=TimeKeeperContext")
        {
        }

        public virtual IDbSet<Allocation> Allocations { get; set; }
        public virtual IDbSet<Label> Labels { get; set; }
        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<Resource> Resources { get; set; }
        public virtual IDbSet<TimeTask> Tasks { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
        public virtual IDbSet<TimePattern> TimePatterns { get; set; }
        public virtual IDbSet<TimePoint> TimePoints { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeTask>()
                .HasMany(task => task.IncludedPatterns)
                .WithMany(pattern => pattern.Inclusions)
                .Map(c =>
                {
                    c.ToTable("Includes");
                    c.MapLeftKey("TimeTaskId");
                    c.MapRightKey("TimePatternId");
                });

            modelBuilder.Entity<TimeTask>()
                .HasMany(task => task.ExcludedPatterns)
                .WithMany(pattern => pattern.Exclusions)
                .Map(c =>
                {
                    c.ToTable("Excludes");
                    c.MapLeftKey("TimeTaskId");
                    c.MapRightKey("TimePatternId");
                });
        }
    }
}
