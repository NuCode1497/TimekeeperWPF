using System;
using System.Data.Entity;
using TimekeeperDAL.Models;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data.Entity.Infrastructure.Interception;
using TimekeeperDAL.Interception;

namespace TimekeeperDAL.EF
{
    public partial class TimeKeeperContext : DbContext, ITimeKeeperContext
    {
        public TimeKeeperContext()
            : base("TimeKeeperConnection")
        {
            var context = (this as IObjectContextAdapter).ObjectContext;
            context.ObjectMaterialized += Context_ObjectMaterialized;
            context.SavingChanges += Context_SavingChanges;
        }

        public IDbSet<Note> Notes { get; set; }

        private void Context_SavingChanges(object sender, EventArgs e)
        {
            //Sender is of type ObjectContext.  Can get current and original values, and 
            //cancel/modify the save operation as desired.
            var context = sender as ObjectContext;
            if (context == null) return;
            foreach (ObjectStateEntry item in context.ObjectStateManager.GetObjectStateEntries(EntityState.Modified | EntityState.Added))
            {
                //Do something important here
                //if ((item.Entity as Car) != null)
                //{
                //    var entity = (Car)item.Entity;
                //    if (entity.Color == "Red")
                //    {
                //        item.RejectPropertyChanges(nameof(entity.Color));
                //    }
                //}

            }
        }
        private void Context_ObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
            //EF materializes each record by setting the properties, which fires PropertyChanged, 
            //which flags IsChanged, therefore we set IsChanged to false here.
            var model = (e.Entity as EntityBase);
            if (model != null) model.IsChanged = false;
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>()
                .Property(e => e.NoteText)
                .IsUnicode(false);
        }
        protected override void Dispose(bool disposing)
        {
            //DbInterception.Remove(loggo);
            //loggo.StopLogging();
            base.Dispose(disposing);
        }
    }
}
