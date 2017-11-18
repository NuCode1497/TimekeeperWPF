using System;
using System.Collections.Generic;
using TimekeeperDAL.EF;

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
            return start < End && Start < end;
        }

        public bool Intersects(InclusionZone Z)
        {
            return Intersects(Z.Start, Z.End);
        }

        public bool Intersects(TimeTask T)
        {
            return Intersects(T.Start, T.End);
        }

        public bool Intersects(CalendarObject C)
        {
            return Intersects(C.Start, C.End);
        }

    }
}
