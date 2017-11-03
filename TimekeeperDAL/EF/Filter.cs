using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class Filter
    {
        [Required]
        public bool Include { get; set; }

        [Required]
        public virtual Filterable Filterable { get; set; }

        public virtual TimeTask TimeTask { get; set; }
    }
}
