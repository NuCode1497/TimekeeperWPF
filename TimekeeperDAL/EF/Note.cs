using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    [Table("Notes")]
    public partial class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime DateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string Text { get; set; }

        public virtual TimeTask TimeTask { get; set; }
    }
}
