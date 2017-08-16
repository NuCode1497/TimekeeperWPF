namespace TimekeeperDAL.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Note
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "timestamp")]
        [MaxLength(8)]
        [Timestamp]
        public byte[] RowVersion { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }

        [Required]
        public int TaskTypeId { get; set; }

        [ForeignKey("TaskTypeId")]
        public virtual TaskType TaskType { get; set; }

    }
}
