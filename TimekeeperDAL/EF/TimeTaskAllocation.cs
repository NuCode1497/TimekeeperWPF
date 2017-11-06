using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimeTaskAllocation
    {
        [Required]
        public long Amount { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ResourceId { get; set; }

        [Required]
        [ForeignKey("ResourceId")]
        public virtual Resource Resource { get; set; }

        [Key]
        [Column(Order = 2)]
        public int TimeTaskId { get; set; }

        [Required]
        [ForeignKey("TimeTaskId")]
        public virtual TimeTask TimeTask { get; set; }
    }
}
