using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TimePatterns")]
    public partial class TimePattern : LabeledEntity
    {
        public TimePattern() : base()
        {
            Query = new HashSet<TimePatternClause>();
        }

        [Required]
        public bool Any { get; set; }
        
        public virtual ICollection<TimePatternClause> Query { get; set; }
    }
}
