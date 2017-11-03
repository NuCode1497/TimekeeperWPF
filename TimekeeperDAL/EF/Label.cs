using System.Collections.Generic;

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
