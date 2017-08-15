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
        [Key]
        public int Id { get; set; }

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



        public virtual ICollection<Label> Labels { get; set; }

        public virtual ICollection<TimePattern> IncludedPatterns { get; set; }

        public virtual ICollection<TimePattern> ExcludedPatterns { get; set; }

        [Column(TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [NotMapped]
        public TimeSpan Duration => End - Start;
    }
}
