using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TypedLabeledEntities")]
    public abstract partial class TypedLabeledEntity : LabeledEntity
    {
        public int TaskTypeId { get; set; }

        [Required]
        [ForeignKey("TaskTypeId")]
        public virtual TaskType TaskType { get; set; }
    }
}
