using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperDAL.Tools
{
    public interface INamedObject
    {
        string Name { get; set; }
    }
}
