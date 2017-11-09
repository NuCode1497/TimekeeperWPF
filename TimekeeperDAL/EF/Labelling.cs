using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("Labellings")]
    public partial class Labelling
    {
        [Key]
        [Column(Order = 1)]
        public int LabeledEntity_Id { get; set; }

        [Key]
        [Column(Order = 2)]
        public int Label_Id { get; set; }

        [Required]
        [ForeignKey("LabeledEntity_Id")]
        public virtual LabeledEntity LabeledEntity { get; set; }

        [Required]
        [ForeignKey("Label_Id")]
        public virtual Label Label { get; set; }
    }
}
