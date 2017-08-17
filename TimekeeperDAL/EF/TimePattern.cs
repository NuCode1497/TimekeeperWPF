using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class TimePattern
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public long Duration { get; set; }

        public virtual TimePattern Child { get; set; }

        public virtual ICollection<Allocation> Allocations { get; set; }

        public virtual ICollection<Label> Labels { get; set; }

        public int ForX { get; set; }

        public virtual TimePoint ForTimePoint { get; set; }

        public int ForNth { get; set; }

        public long ForSkipDuration { get; set; }
    }
}
