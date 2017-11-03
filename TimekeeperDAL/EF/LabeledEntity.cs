using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.EF
{
    public abstract partial class LabeledEntity : Filterable
    {
        public LabeledEntity()
        {
            Labels = new HashSet<Label>();
        }

        public virtual ICollection<Label> Labels { get; set; }
    }
}
