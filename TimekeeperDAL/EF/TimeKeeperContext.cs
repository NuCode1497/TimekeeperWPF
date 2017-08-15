namespace TimekeeperDAL.EF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TimeKeeperContext : DbContext, ITimeKeeperContext
    {
        public TimeKeeperContext()
            : base("name=TimeKeeperContext")
        {
        }

        public virtual IDbSet<Note> Notes { get; set; }
        public virtual IDbSet<TaskType> TaskTypes { get; set; }
        public virtual IDbSet<Task> Tasks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>()
                .Property(e => e.Text)
                .IsUnicode(false);

            modelBuilder.Entity<Note>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<Note>()
                .Property(e => e.RowVersion)
                .IsFixedLength();

            modelBuilder.Entity<TaskType>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<TaskType>()
                .Property(e => e.RowVersion)
                .IsFixedLength();
        }
    }
}
