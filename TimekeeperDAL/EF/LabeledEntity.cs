using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("LabelledEntities")]
    public abstract partial class LabeledEntity : Filterable
    {
        public LabeledEntity() : base()
        {
            Labellings = new HashSet<Labelling>();
        }

        public virtual ICollection<Labelling> Labellings { get; set; }
    }
}
