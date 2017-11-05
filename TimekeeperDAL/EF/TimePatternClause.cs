using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimePatternClause
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string TimeProperty { get; set; }

        [Required]
        public string Equivalency { get; set; }

        [Required]
        public string TimePropertyValue { get; set; }

        [Required]
        public virtual TimePattern TimePattern { get; set; }
    }
}
