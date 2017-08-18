using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class Resource
    {
        #region Navigation
        public virtual ICollection<Allocation> Allocations { get; set; }

        #endregion
    }
}
