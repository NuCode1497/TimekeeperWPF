using System.Collections.Generic;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public class CalendarTimeTaskMap
    {
        public TimeTask TimeTask { get; set; }
        public HashSet<PerZone> PerZones { get; set; }
        public LinkedList<CalendarCheckIn> CheckIns { get; set; }
    }
}
