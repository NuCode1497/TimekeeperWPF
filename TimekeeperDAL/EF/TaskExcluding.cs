using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TaskExcluding
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int TimePatternId { get; set; }

        [ForeignKey("TaskId")]
        public virtual Task Task { get; set; }

        [ForeignKey("TimePatternId")]
        public virtual TimePattern TimePattern { get; set; }
    }
}
