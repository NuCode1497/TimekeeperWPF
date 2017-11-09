using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("TaskTypes")]
    public partial class TaskType : Filterable
    {
    }
}
