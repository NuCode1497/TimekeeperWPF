using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TimeTask
    {
        public TimeTask()
        {
            Labels = new HashSet<Label>();
            IncludedPatterns = new HashSet<TimePattern>();
            ExcludedPatterns = new HashSet<TimePattern>();
        }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Start { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime End { get; set; }

        [StringLength(150)]
        public string Description { get; set; }

        public int Priority { get; set; }

        public bool RaiseOnReschedule { get; set; }

        public bool AsksForReschedule { get; set; }

        public bool CanReschedule { get; set; }

        public bool AsksForCheckin { get; set; }

        public bool CanBePushed { get; set; }

        public bool CanInflate { get; set; }

        public bool CanDeflate { get; set; }

        public bool CanFill { get; set; }

        public bool CanBeEarly { get; set; }

        public bool CanBeLate { get; set; }

        public int Dimension { get; set; }

        public int PowerLevel { get; set; }

        #region Navigation

        [Required]
        public virtual TaskType TaskType { get; set; }

        public virtual ICollection<Label> Labels { get; set; }

        [InverseProperty("Inclusions")]
        public virtual ICollection<TimePattern> IncludedPatterns { get; set; }

        [InverseProperty("Exclusions")]
        public virtual ICollection<TimePattern> ExcludedPatterns { get; set; }

        #endregion
    }
}
