using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TimePatternClauses")]
    public partial class TimePatternClause
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        [Column(Order = 1)]
        public int Id { get; set; }

        [Key]
        [Column(Order = 2)]
        public int TimePattern_Id { get; set; }
        
        [Required]
        public string TimeProperty { get; set; }

        [Required]
        public string Equivalency { get; set; }

        [Required]
        public string TimePropertyValue { get; set; }

        [Required]
        [ForeignKey("TimePattern_Id")]
        public virtual TimePattern TimePattern { get; set; }
    }
}
