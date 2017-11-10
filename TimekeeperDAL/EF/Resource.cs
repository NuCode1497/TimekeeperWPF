using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("Resources")]
    public partial class Resource : Filterable
    {
        public Resource()
        {
            TimeTaskAllocations = new HashSet<TimeTaskAllocation>();
        }

        [InverseProperty("Resource")]
        public virtual ICollection<TimeTaskAllocation> TimeTaskAllocations { get; set; }
    }
}
