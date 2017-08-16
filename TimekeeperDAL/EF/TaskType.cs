namespace TimekeeperDAL.EF
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class TaskType
    {
        [Index(IsUnique = true)]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
