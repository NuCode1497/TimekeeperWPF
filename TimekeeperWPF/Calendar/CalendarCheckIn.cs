using System;

namespace TimekeeperWPF.Calendar
{
    //The order here is important for proper sorting
    public enum CheckInKind
    {
        PerZoneStart, InclusionZoneStart, EventStart, EventEnd, InclusionZoneEnd, PerZoneEnd
    }

    public class CalendarCheckIn
    {
        public CalendarTimeTaskMap ParentMap { get; set; }
        public PerZone ParentPerZone { get; set; }
        public InclusionZone ParentInclusionZone { get; set; }
        public DateTime DateTime { get; set; }
        public CheckInKind Kind { get; set; }
    }
}
