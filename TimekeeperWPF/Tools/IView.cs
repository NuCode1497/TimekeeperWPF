using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimekeeperWPF.Tools
{
    public interface IView
    {
        string Name { get; }
        ICommand SaveAsCommand { get; }
        ICommand GetDataCommand { get; }
    }
}
