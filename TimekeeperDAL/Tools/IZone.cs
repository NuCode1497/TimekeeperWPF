using System;

namespace TimekeeperDAL.Tools
{
    public interface IZone
    {
        DateTime Start { get; set; }
        DateTime End { get; set; }
        TimeSpan Duration { get; }
    }
}
