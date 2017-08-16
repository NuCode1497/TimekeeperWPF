﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Label
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<NoteLabeling> NoteLabelings { get; set; }
    }
}
