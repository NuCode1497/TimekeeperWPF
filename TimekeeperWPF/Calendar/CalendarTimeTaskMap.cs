using System.Collections.Generic;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public class CalendarTimeTaskMap
    {
        public TimeTask TimeTask { get; set; }
        public List<PerZone> PerZones { get; set; }
    }
}
