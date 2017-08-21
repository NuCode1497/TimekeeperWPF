using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimePattern
    {
        public TimePattern()
        {
            Allocations = new HashSet<Allocation>();
            Labels = new HashSet<Label>();
            Inclusions = new HashSet<TimeTask>();
            Exclusions = new HashSet<TimeTask>();
        }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public long Duration { get; set; }

        public int ForX { get; set; }

        public int ForNth { get; set; }

        public long ForSkipDuration { get; set; }

        #region Navigation.
        public virtual TimePoint ForTimePoint { get; set; }

        public virtual TimePattern Child { get; set; }

        public virtual ICollection<Allocation> Allocations { get; set; }

        [InverseProperty("IncludedPatterns")]
        public virtual ICollection<TimeTask> Inclusions { get; set; }

        [InverseProperty("ExcludedPatterns")]
        public virtual ICollection<TimeTask> Exclusions { get; set; }

        #endregion

    }
}
