using System.ComponentModel.DataAnnotations;

namespace TimekeeperDAL.EF
{
    public abstract partial class Filterable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }
    }
}
