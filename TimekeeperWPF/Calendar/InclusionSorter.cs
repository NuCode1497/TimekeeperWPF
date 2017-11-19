using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF.Calendar
{
    public class InclusionSorterAsc : IComparer<InclusionZone>
    {
        public int Compare(InclusionZone x, InclusionZone y)
        {
            return x.Start.CompareTo(y.Start);
        }
    }
    public class InclusionSorterDesc : IComparer<InclusionZone>
    {
        public int Compare(InclusionZone x, InclusionZone y)
        {
            return y.Start.CompareTo(x.Start);
        }
    }
}
