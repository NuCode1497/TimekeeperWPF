using System;
using System.Collections.Generic;
using System.Linq;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public class PerZone
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration => End - Start;
        public List<InclusionZone> InclusionZones { get; set; }
        //public List<Consumption> Consumptions { get; set; }
        public Consumption TimeConsumption { get; set; }
        public List<CalendarTaskObject> CalTaskObjs { get; set; }
        public List<CalendarNoteObject> CalNoteObjs { get; set; }

        public bool Intersects(DateTime dt) { return Start <= dt && dt <= End; }
        public bool Intersects(Note N) { return Intersects(N.DateTime); }
        public bool Intersects(CalendarNoteObject C) { return Intersects(C.DateTime); }
        public bool Intersects(DateTime start, DateTime end) { return start < End && Start < end; }
        public bool Intersects(InclusionZone Z) { return Intersects(Z.Start, Z.End); }
        public bool Intersects(TimeTask T) { return Intersects(T.Start, T.End); }
        public bool Intersects(CalendarTaskObject C) { return Intersects(C.Start, C.End); }

        public bool IsAffirmedBeforePoint(DateTime point)
        {
            return CalTaskObjs.Count(C => C.Affirmed == false && C.Start < point) == 0;
        }
    }
}
