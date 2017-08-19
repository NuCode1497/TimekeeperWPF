using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class Resource
    {
        public Resource()
        {
            Allocations = new HashSet<Allocation>();
        }
        #region Navigation
        public virtual ICollection<Allocation> Allocations { get; set; }

        #endregion
    }
}
