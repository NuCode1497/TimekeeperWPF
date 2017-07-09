namespace TimekeeperWPF.EF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using TimekeeperWPF.Models;

    public partial class TimeKeeperEntities : DbContext
    {
        public TimeKeeperEntities()
            : base("name=TimeKeeperEntities")
        {
        }

        public virtual DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>()
                .Property(e => e.NoteText)
                .IsUnicode(false);
        }
    }
}
