using System;
using System.Data.Entity;
using System.Data.Entity.Design.PluralizationServices;
using System.Threading.Tasks;

namespace TimekeeperDAL.EF
{
    public class FakeDbContext : IDisposable
    {

        public int SaveChanges()
        {
            return 0;
        }
        public Task<int> SaveChangesAsync()
        {
            return Task.Run(() =>
            {
                //Thread.Sleep(3000);
                return 0;
            });
        }

        public DbSet Set(Type entityType)
        {
            //Assuming the set is named by default as the plural form of the entity name,
            //using reflection to get the property by name
            PluralizationService PS = PluralizationService.CreateService(System.Globalization.CultureInfo.CurrentCulture);
            return GetType().GetProperty(PS.Pluralize(entityType.Name)).GetValue(this) as DbSet;
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return Set(typeof(TEntity)) as DbSet<TEntity>;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FakeTimeKeeperContext() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
