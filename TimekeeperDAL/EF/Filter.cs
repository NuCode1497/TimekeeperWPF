using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public partial class Filter
    {
        [Required]
        public bool Include { get; set; }

        [Required]
        public Filterable Filterable { get; set; }

        public TimeTask TimeTask { get; set; }
    }
}
