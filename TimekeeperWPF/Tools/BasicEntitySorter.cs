using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Tools
{
    public class BasicEntitySorter : IComparer
    {
        public int Compare(object x, object y)
        {
            BasicEntity Ex = x as BasicEntity;
            BasicEntity Ey = y as BasicEntity;
            return Ex.Name.CompareTo(Ey.Name);
        }
    }
}
