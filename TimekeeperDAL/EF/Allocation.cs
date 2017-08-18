using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class Allocation
    {
        [Required]
        public long minAmount { get; set; }

        [Required]
        public long maxAmount { get; set; }

        #region Navigation

        [Required]
        public virtual Resource Resource { get; set; }

        [Required]
        public TimePattern TimePattern { get; set; }

        #endregion
    }
}
