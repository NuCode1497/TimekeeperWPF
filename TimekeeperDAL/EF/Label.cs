using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("Labels")]
    public partial class Label : Filterable
    {
    }
}
