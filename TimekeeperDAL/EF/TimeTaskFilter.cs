using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TimeTaskFilters")]
    public partial class TimeTaskFilter
    {
        [Required]
        public bool Include { get; set; }

        [Key]
        [Column(Order = 1)]
        public int TimeTask_Id { get; set; }

        [Key]
        [Column(Order = 2)]
        public int Filterable_Id { get; set; }

        [Required]
        [ForeignKey("TimeTask_Id")]
        public virtual TimeTask TimeTask { get; set; }

        [Required]
        [ForeignKey("Filterable_Id")]
        public virtual Filterable Filterable { get; set; }
    }
}
