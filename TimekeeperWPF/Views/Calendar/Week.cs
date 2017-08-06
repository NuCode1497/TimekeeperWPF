using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF
{
    public class Week : ObservableObject
    {
        private List<Day> _Days;
        public List<Day> Days
        {
            get
            {
                return _Days;
            }
            set
            {
                _Days = value;
                OnPropertyChanged();
            }
        }
    }
}
