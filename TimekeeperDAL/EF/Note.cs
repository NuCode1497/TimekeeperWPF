namespace TimekeeperDAL.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Note
    {
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }

        [Required] 
        public int TaskTypeId { get; set; }

        [ForeignKey("TaskTypeId")]
        public TaskType TaskType { get; set; }

        public ICollection<Label> Labels { get; set; }
    }
}
