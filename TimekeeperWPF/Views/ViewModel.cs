using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperWPF.Tools;

namespace TimekeeperWPF
{
    public abstract class ViewModel<T> : ObservableObject, IPage where T : class, new()
    {
        protected TimeKeeperContext Context;
        public virtual string Name => throw new NotImplementedException();

        //TODO: Move stuff from NoteViewModel here
    }
}
