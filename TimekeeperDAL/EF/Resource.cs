using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("Resources")]
    public partial class Resource : Filterable
    {
    }
}
