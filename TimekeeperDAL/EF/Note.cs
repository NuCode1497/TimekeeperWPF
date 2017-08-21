using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
   public partial class Note
    {
        public Note()
        {
            Labels = new HashSet<Label>();
        }
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }
        
        #region Navigation

        [Required]
        public virtual TaskType TaskType { get; set; }

        #endregion
    }
}
