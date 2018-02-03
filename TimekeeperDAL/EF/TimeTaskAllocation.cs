using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TimeTaskAllocations")]
    public partial class TimeTaskAllocation
    {
        public double Amount { get; set; }

        public double PerOffset { get; set; }

        public bool Limited { get; set; }

        public double InstanceMinimum { get; set; }

        [Required]
        public string Method { get; set; }

        [Key]
        [Column(Order = 1)]
        public int TimeTask_Id { get; set; }

        [Key]
        [Column(Order = 2)]
        public int Resource_Id { get; set; }

        public int? PerId { get; set; }

        [Required]
        [ForeignKey("TimeTask_Id")]
        public virtual TimeTask TimeTask { get; set; }

        [Required]
        [ForeignKey("Resource_Id")]
        public virtual Resource Resource { get; set; }

        [ForeignKey("PerId")]
        public virtual Resource Per { get; set; }
    }
}
