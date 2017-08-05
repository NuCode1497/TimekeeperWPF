using System.Collections;
using TimekeeperDAL.EF;

namespace TimekeeperWPF
{
    public class DateTimeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            Note noteX = x as Note;
            Note noteY = y as Note;
            return noteX.DateTime.CompareTo(noteY.DateTime);
        }
    }
}
