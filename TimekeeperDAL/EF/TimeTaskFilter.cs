using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimeTaskFilter
    {
        [Required]
        public bool Include { get; set; }

        [Key]
        [Column(Order = 1)]
        public int FilterableId { get; set; }

        [Required]
        [ForeignKey("FilterableId")]
        public virtual Filterable Filterable { get; set; }

        [Key]
        [Column(Order = 2)]
        public int TimeTaskId { get; set; }

        [Required]
        [ForeignKey("TimeTaskId")]
        public virtual TimeTask TimeTask { get; set; }
    }
}
