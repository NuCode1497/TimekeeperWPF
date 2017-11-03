using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Label : Filterable
    {
        public Label()
        {
            LabeledEntities = new HashSet<LabeledEntity>();
        }
        public virtual ICollection<LabeledEntity> LabeledEntities { get; set; }
    }
}
