using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;
using TimekeeperDAL.EF;

namespace TimekeeperWPF
{
    public class FakeNotesViewModel : ViewModel<Note, TimeKeeperContext>
    {
        public override string Name => throw new NotImplementedException();
    }
}
