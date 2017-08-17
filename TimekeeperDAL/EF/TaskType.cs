using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class TaskType
    {
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
