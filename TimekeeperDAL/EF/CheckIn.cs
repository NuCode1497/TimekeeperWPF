using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("CheckIns")]
    public partial class CheckIn
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }
        
        public bool Start { get; set; }
        
        [Required]
        public virtual TimeTask TimeTask { get; set; }
    }
}
