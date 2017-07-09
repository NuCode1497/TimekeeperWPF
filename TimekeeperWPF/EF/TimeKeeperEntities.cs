namespace TimekeeperWPF.EF
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using TimekeeperWPF.Models;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Interception;
    using TimekeeperWPF.Interception;

    public partial class TimeKeeperEntities : DbContext
    {
        static readonly DatabaseLogger loggo = new DatabaseLogger("sqllob.txt", true);
        public TimeKeeperEntities()
            : base("name=TimeKeeperEntities")
        {
            //print sql queries to the console
            //DbInterception.Add(new Interceptor());

            //loggo.StartLogging();
            //DbInterception.Add(loggo);


            //These can save me from having to create an interceptor?
            var context = (this as IObjectContextAdapter).ObjectContext;
            context.ObjectMaterialized += Context_ObjectMaterialized;
            context.SavingChanges += Context_SavingChanges;
        }

        private void Context_SavingChanges(object sender, EventArgs e)
        {
        }

        private void Context_ObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
            var model = (e.Entity as EntityBase);
            if (model != null) model.IsChanged = false;
        }

        public virtual DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>()
                .Property(e => e.NoteText)
                .IsUnicode(false);
        }

        protected override void Dispose(bool disposing)
        {
            DbInterception.Remove(loggo);
            loggo.StopLogging();
            base.Dispose(disposing);
        }
    }
}
