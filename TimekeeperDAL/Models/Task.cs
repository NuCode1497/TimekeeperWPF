using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Task
    {
        public override string ToString()
        {
            return Name + " - " + Description;
        }
        [NotMapped]
        public TimeSpan Duration => End - Start;
    }
}
