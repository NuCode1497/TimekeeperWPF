using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF.Tools
{
    public delegate void RequestViewChangeEventHandler(object sender, RequestViewChangeEventArgs e);
    public enum CalendarViewType { Day, Month, Week, Year}
    public class RequestViewChangeEventArgs : EventArgs
    {
        public RequestViewChangeEventArgs(CalendarViewType type, DateTime date)
        {
            Type = type;
            Date = date;
        }
        public CalendarViewType Type { get; set; }
        public DateTime Date { get; set; }
    }
}
