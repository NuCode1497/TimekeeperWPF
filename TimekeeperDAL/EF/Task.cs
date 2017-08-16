using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Task
    {
        public Task()
        {
            TaskLabelings = new HashSet<TaskLabeling>();
            TaskIncludings = new HashSet<TaskIncluding>();
            TaskExcludings = new HashSet<TaskExcluding>();
        }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        public int TaskTypeId { get; set; }

        [ForeignKey("TaskTypeId")]
        public virtual TaskType TaskType { get; set; }

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

        public virtual ICollection<TaskLabeling> TaskLabelings { get; set; }

        public virtual ICollection<TaskIncluding> TaskIncludings { get; set; }

        public virtual ICollection<TaskExcluding> TaskExcludings { get; set; }

    }
}
