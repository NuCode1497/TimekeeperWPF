using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Tools
{
    public class NameSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            INamedObject Ex = x as INamedObject;
            INamedObject Ey = y as INamedObject;
            return Ex.Name.CompareTo(Ey.Name);
        }
    }
}
