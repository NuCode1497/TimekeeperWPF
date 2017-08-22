using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.EF
{
    public abstract class TypedLabeledEntity : LabeledEntity
    {
        [Required]
        public virtual TaskType TaskType { get; set; }

    }
}
