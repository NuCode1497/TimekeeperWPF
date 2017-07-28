using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperDAL.Models;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public abstract class ViewModel<T> : ObservableObject, IPage, IDisposable where T : EntityBase, new()
    {
        private String _status = "Ready";
        protected TimeKeeperContext Context;
        public abstract string Name { get; }
        public String Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        //TODO: Move stuff from NoteViewModel here

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ViewModel() {
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
