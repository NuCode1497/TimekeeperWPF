using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TimeTasks")]
    public partial class TimeTask : TypedLabeledEntity
    {
        public TimeTask() : base()
        {
            Allocations = new HashSet<TimeTaskAllocation>();
            Filters = new HashSet<TimeTaskFilter>();
        }

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

        [Required]
        public string AllocationMethod { get; set; }

        public virtual ICollection<TimeTaskAllocation> Allocations { get; set; }

        public virtual ICollection<TimeTaskFilter> Filters { get; set; }
    }
}
