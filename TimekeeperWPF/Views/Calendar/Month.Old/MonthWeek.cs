using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF.Old
{
    public class MonthWeek : ObservableObject
    {
        private List<MonthDay> _Days;
        public MonthWeek()
        {
            _Days = new List<MonthDay>();
        }
        public List<MonthDay> Days
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
