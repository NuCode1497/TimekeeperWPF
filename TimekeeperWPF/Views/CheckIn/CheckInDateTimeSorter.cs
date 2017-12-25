using System.Collections;
using TimekeeperDAL.EF;

namespace TimekeeperWPF
{
    public class CheckInDateTimeSorterAsc : IComparer
    {
        public int Compare(object x, object y)
        {
            CheckIn CIX = x as CheckIn;
            CheckIn CIY = y as CheckIn;
            return CIX.DateTime.CompareTo(CIY.DateTime);
        }
    }
    public class CheckInDateTimeSorterDesc : IComparer
    {
        public int Compare(object x, object y)
        {
            CheckIn CIX = x as CheckIn;
            CheckIn CIY = y as CheckIn;
            return CIY.DateTime.CompareTo(CIX.DateTime);
        }
    }
}
