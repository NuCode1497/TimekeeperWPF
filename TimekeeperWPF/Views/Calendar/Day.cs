using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TimekeeperWPF
{
    public class Day : ObservableObject
    {
        private DateTime _dateTime;
        public Day()
        {
            _dateTime = DateTime.Now;
            IsNotInMonth = false;
        }
        public int DayOfMonth => CultureInfo.CurrentCulture.Calendar.GetDayOfMonth(_dateTime);
        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }
        public bool IsNotInMonth { get; set; }
    }
}
