using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF.Tools
{
    public class Grouping<TKey, TElement> : List<TElement>, IGrouping<TKey, TElement>
    {
        public TKey Key { get; set; }
    }
}
