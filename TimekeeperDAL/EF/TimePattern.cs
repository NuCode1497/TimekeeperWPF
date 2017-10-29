using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimePattern
    {
        public TimePattern()
        {
            Query = new HashSet<TimePatternClause>();
            Labels = new HashSet<Label>();
            IncludedBy = new HashSet<TimeTask>();
            ExcludedBy = new HashSet<TimeTask>();
        }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public virtual ICollection<TimePatternClause> Query { get; set; }
        [InverseProperty(nameof(TimeTask.IncludedPatterns))]
        public virtual ICollection<TimeTask> IncludedBy { get; set; }
        [InverseProperty(nameof(TimeTask.ExcludedPatterns))]
        public virtual ICollection<TimeTask> ExcludedBy { get; set; }
    }
}
