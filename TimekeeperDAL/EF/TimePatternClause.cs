using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class TimePatternClause
    {
        [Required]
        public string TimeProperty { get; set; }
        [Required]
        public string Equivalency { get; set; }
        [Required]
        public string TimePropertyValue { get; set; }
        [Required]
        public TimePattern TimePattern { get; set; }
    }
}
