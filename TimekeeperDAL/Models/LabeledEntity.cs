using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.EF
{
    public abstract class LabeledEntity : EntityBase
    {
        public virtual ICollection<Label> Labels { get; set; }

        [NotMapped]
        public string LabelsToString
        {
            get
            {
                string s = "";
                foreach (Label l in Labels)
                {
                    s += l.ToString() + ", ";
                }
                if (s.Length >= 2)
                    s = s.Substring(0, s.Length - 2);
                return s;
            }
        }

    }
}
