using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.EF
{
    public abstract partial class TypedLabeledEntity : LabeledEntity
    {
        [Required]
        public virtual TaskType TaskType { get; set; }
    }
}
