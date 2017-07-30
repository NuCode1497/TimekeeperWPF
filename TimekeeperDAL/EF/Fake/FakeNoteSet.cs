using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;

namespace TimekeeperDAL.EF
{
    public class FakeNoteSet : FakeDbSet<Note>
    {
        public override Note Find(params object[] keyValues)
        {
            return this.SingleOrDefault(d => d.NoteID == (int)keyValues.Single());
        }
    }
}
