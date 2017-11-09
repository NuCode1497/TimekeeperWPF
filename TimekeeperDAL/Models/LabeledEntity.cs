using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.EF
{
    public abstract partial class LabeledEntity : Filterable
    {
        [NotMapped]
        public string LabelsToString
        {
            get
            {
                string s = "";
                foreach (Labelling l in Labellings)
                {
                    s += l.Label + ", ";
                }
                if (s.Length >= 2)
                    s = s.Substring(0, s.Length - 2);
                return s;
            }
        }
    }
}
