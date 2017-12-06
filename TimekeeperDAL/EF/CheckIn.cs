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
        [StringLength(10)]
        public string Text { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        public TimeTask TimeTask { get; set; }
    }
}
