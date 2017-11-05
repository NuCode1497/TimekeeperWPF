using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Resource : Filterable
    {
        public Resource()
        {
            AllocatedBy = new HashSet<TimeTaskAllocation>();
        }

        public virtual ICollection<TimeTaskAllocation> AllocatedBy { get; set; }
    }
}
