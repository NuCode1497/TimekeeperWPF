using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Resource : Filterable
    {
        public Resource()
        {
            Allocations = new HashSet<Allocation>();
        }

        public virtual ICollection<Allocation> Allocations { get; set; }
    }
}
