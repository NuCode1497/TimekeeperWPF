using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TaskTypes")]
    public partial class TaskType : Filterable
    {
        public TaskType()
        {
            TypedEntities = new HashSet<TypedLabeledEntity>();
        }
        public virtual ICollection<TypedLabeledEntity> TypedEntities { get; set; }
    }
}
