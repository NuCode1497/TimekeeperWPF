using System;
using System.Collections.Generic;
using System.Linq;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public class PerZone : Zone
    {
        public CalendarTimeTaskMap ParentMap { get; set; }
        public Consumption TimeConsumption { get; set; }
        public List<InclusionZone> InclusionZones { get; set; }
        public HashSet<CalendarTaskObject> CalTaskObjs { get; set; }
        public List<CalendarCheckIn> CheckIns { get; set; }
        
        public bool IsAffirmedBeforePoint(DateTime point)
        {
            return CalTaskObjs.Count(C => C.Affirmed == false && C.Start < point) == 0;
        }
    }
}
