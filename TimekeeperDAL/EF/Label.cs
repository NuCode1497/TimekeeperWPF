using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("Labels")]
    public partial class Label : Filterable
    {
        public Label()
        {
            Labellings = new HashSet<Labelling>();
        }
        public virtual ICollection<Labelling> Labellings { get; set; }
    }
}
