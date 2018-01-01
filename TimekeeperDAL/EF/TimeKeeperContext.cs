using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.ModelConfiguration.Conventions;
using TimekeeperDAL.Interception;

namespace TimekeeperDAL.EF
{
    public partial class TimeKeeperContext : DbContext, ITimeKeeperContext
    {
        //static readonly DatabaseLogger loggo = new DatabaseLogger("sqllog.txt", true);
        static TimeKeeperContext()
        {
            //print sql queries to the console
            DbInterception.Add(new Interceptor());

            ////print sql queries to a file sqllog.txt
            //loggo.StartLogging();
            //DbInterception.Add(loggo);
        }
        public TimeKeeperContext()
            : base("name=TimeKeeperContext")
        {
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
        public virtual IDbSet<CheckIn> CheckIns { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //I've manually added "cascadeDelete: true" to the foreign keys for allocations and labellings
            //in the InitialCreate migration, because it doesn't work if I do it like this idk why
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

            //modelBuilder.Entity<CheckIn>()
            //    .HasRequired(CI => CI.TimeTask)
            //    .WithMany()
            //    .WillCascadeOnDelete();

            //Add a blank migration then change it to something like this:
            //Notice the 5th param is cascade set to true
            //public override void Up()
            //{
            //    DropForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks");
            //    AddForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks", "Id", true);
            //}

            //public override void Down()
            //{
            //    DropForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks");
            //    AddForeignKey("dbo.CheckIns", "TimeTask_Id", "dbo.TimeTasks", "Id");
            //}
        }
    }
}
