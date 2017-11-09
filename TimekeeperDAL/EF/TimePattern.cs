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
        
        public virtual ICollection<TimePatternClause> Query { get; set; }
    }
}
