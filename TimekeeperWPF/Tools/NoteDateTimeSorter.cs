using System.Collections;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Tools
{
    public class NoteDateTimeSorterAsc : IComparer
    {
        public int Compare(object x, object y)
        {
            Note noteX = x as Note;
            Note noteY = y as Note;
            return noteX.DateTime.CompareTo(noteY.DateTime);
        }
    }
    public class NoteDateTimeSorterDesc : IComparer
    {
        public int Compare(object x, object y)
        {
            Note noteX = x as Note;
            Note noteY = y as Note;
            return noteY.DateTime.CompareTo(noteX.DateTime);
        }
    }
}
