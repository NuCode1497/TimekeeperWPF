using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public class Zone : IZone
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration => End - Start;

        public bool Intersects(DateTime dt) { return Start <= dt && dt <= End; }
        public bool Intersects(Note N) { return Intersects(N.DateTime); }
        public bool Intersects(CalendarNoteObject C) { return Intersects(C.DateTime); }
        public bool Intersects(CheckIn CI) { return Intersects(CI.DateTime); }
        public bool Intersects(CalendarCheckIn CI) { return Intersects(CI.DateTime); }
        public bool Intersects(DateTime start, DateTime end) { return start < End && Start < end; }
        public bool Intersects(IZone Z) { return Intersects(Z.Start, Z.End); }
        public bool IsInside(DateTime start, DateTime end) { return start < Start && End < end; }
        public bool IsInside(IZone Z) { return Z.Start < Start && End < Z.End; }
    }
}
