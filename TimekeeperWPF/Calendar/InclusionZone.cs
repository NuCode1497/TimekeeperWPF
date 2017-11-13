using System;
using System.Collections.Generic;

namespace TimekeeperWPF.Calendar
{
    public class InclusionZone
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration => End - Start;
        public List<CalendarObject> CalendarObjects { get; set; }

        public bool Intersects(DateTime start, DateTime end)
        {
            return (start >= Start && start <= End)
                || (end >= Start && end <= End);
        }
    }
}
