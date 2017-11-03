using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Allocation
    {
        [Required]
        public long Amount { get; set; }
        [Required]
        public virtual Resource Resource { get; set; }
        [Required]
        public virtual TimeTask TimeTask { get; set; }
    }
}
