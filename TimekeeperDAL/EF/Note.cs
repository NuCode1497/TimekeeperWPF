using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
   public partial class Note : TypedLabeledEntity
    {
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }
    }
}
