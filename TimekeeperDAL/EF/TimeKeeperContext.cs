using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace TimekeeperDAL.EF
{
    public partial class TimeKeeperContext : DbContext, ITimeKeeperContext
    {
        public TimeKeeperContext()
            : base("name=TimeKeeperContext")
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());
        }

        public virtual IDbSet<TimeTaskAllocation> Allocations { get; set; }
        public virtual IDbSet<Label> Labels { get; set; }
        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<Resource> Resources { get; set; }
        public virtual IDbSet<TimeTask> TimeTasks { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
        public virtual IDbSet<TimePattern> TimePatterns { get; set; }
        public virtual IDbSet<TimePatternClause> TimePatternClauses { get; set; }
        public virtual IDbSet<TimeTaskFilter> Filters { get; set; }
        public virtual IDbSet<Filterable> Filterables { get; set; }
        public virtual IDbSet<LabeledEntity> LabeledEntities { get; set; }
        public virtual IDbSet<TypedLabeledEntity> TypedLabeledEntities { get; set; }
        public virtual IDbSet<Labelling> Labellings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //I've manually added "cascadeDelete: true" to the foreign keys for allocations and labellings
            //in the InitialCreate migration
            //modelBuilder.Entity<Labelling>()
            //    .HasRequired(l => l.LabeledEntity)
            //    .WithMany()
            //    .HasForeignKey(l => l.LabeledEntity_Id)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<TimeTaskAllocation>()
            //    .HasRequired(a => a.Resource)
            //    .WithMany()
            //    .HasForeignKey(a => a.Resource_Id)
            //    .WillCascadeOnDelete(true);

            //modelBuilder.Entity<TimeTaskFilter>()
            //    .HasRequired(f => f.Filterable)
            //    .WithMany()
            //    .HasForeignKey(f => f.Filterable_Id)
            //    .WillCascadeOnDelete(true);
        }
    }
}
