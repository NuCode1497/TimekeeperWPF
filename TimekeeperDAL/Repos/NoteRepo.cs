using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Models;

namespace TimekeeperDAL.Repos
{
    public class NoteRepo : BaseRepo<Note>, IRepo<Note>
    {
        public NoteRepo()
        {
            Table = Context.Notes;
        }
        public int Delete(int id, byte[] timeStamp)
        {
            Context.Entry(new Note() {
                NoteID = id,
                RowVersion = timeStamp
            }).State = EntityState.Deleted;
            return SaveChanges();
        }
        public Task<int> DeleteAsync(int id, byte[] timeStamp)
        {
            Context.Entry(new Note() {
                NoteID = id,
                RowVersion = timeStamp
            }).State = EntityState.Deleted;
            return SaveChangesAsync();
        }
    }
}
