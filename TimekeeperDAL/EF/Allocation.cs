using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class Allocation
    {
        [Required]
        public long Amount { get; set; }
        [Required]
        public virtual Resource Resource { get; set; }
        [Required]
        public TimeTask TimeTask { get; set; }
    }
}
