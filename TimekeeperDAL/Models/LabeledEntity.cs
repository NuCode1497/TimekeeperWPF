using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if(s.Length >= 2) s.Substring(s.Length - 2);
                return s;
            }
        }

    }
}
