using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class NoteLabeling
    {
        [Required]
        public int NoteId { get; set; }

        [Required]
        public int LabelId { get; set; }

        [ForeignKey("NoteId")]
        public virtual Note Note { get; set; }

        [ForeignKey("LabelId")]
        public virtual Label Label { get; set; }
    }
}
