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
            Notes = new HashSet<Note>();
            CheckIns = new HashSet<CheckIn>();
        }

        [Column(TypeName = "datetime2")]
        public DateTime Start { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime End { get; set; }

        [StringLength(150)]
        public string Description { get; set; }

        public double Priority { get; set; }
        
        public bool AutoCheckIn { get; set; }
        
        public bool CanFill { get; set; }
        
        public int Dimension { get; set; }

        public bool CanReDist { get; set; }
        
        public bool CanSplit { get; set; }
        
        public virtual ICollection<TimeTaskAllocation> Allocations { get; set; }
        public virtual ICollection<TimeTaskFilter> Filters { get; set; }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<CheckIn> CheckIns { get; set; }
    }
}
