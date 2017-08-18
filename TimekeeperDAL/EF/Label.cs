using System.Collections.Generic;

namespace TimekeeperDAL.EF
{
    public partial class Label
    {
        #region Navigation

        public virtual ICollection<Note> Notes { get; set; }

        public virtual ICollection<TimeTask> Tasks { get; set; }

        public virtual ICollection<TimePattern> Patterns { get; set; }

        #endregion
    }
}
