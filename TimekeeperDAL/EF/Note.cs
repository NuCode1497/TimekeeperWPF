namespace TimekeeperDAL.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Note
    {
        public Note()
        {
            NoteLabelings = new HashSet<NoteLabeling>();
        }

        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }

        [Required] 
        public int TaskTypeId { get; set; }

        [ForeignKey("TaskTypeId")]
        public virtual TaskType TaskType { get; set; }

        public virtual ICollection<NoteLabeling> NoteLabelings { get; set; }

    }
}
