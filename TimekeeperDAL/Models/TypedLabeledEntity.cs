using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public abstract class TypedLabeledEntity : LabeledEntity
    {
        [Required]
        public virtual TaskType TaskType { get; set; }

    }
}
