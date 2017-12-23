using System;

namespace TimekeeperDAL.Tools
{
    public interface IZone
    {
        DateTime Start { get; set; }
        DateTime End { get; set; }
        TimeSpan Duration { get; }
        bool Intersects(IZone Z);
        bool Intersects(DateTime dt);
        bool Intersects(DateTime start, DateTime end);
        bool IsInside(DateTime start, DateTime end);
        bool IsInside(IZone Z);
    }
}
