namespace TimekeeperDAL.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Note
    {
        public int NoteID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime NoteDateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string NoteText { get; set; }
    }
}
