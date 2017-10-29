using System.Collections.Generic;

namespace TimekeeperDAL.EF
{
    public partial class Label
    {
        public Label()
        {
            Notes = new HashSet<Note>();
            Tasks = new HashSet<TimeTask>();
            Patterns = new HashSet<TimePattern>();
        }
        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<TimeTask> Tasks { get; set; }
        public virtual ICollection<TimePattern> Patterns { get; set; }
    }
}
