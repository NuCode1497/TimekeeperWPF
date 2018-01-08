using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public class InclusionZone : Zone
    {
        public PerZone ParentPerZone { get; set; }
        public CalendarTaskObject SeedTaskObj { get; set; }
        public double Priority => ParentPerZone.ParentMap.TimeTask.Priority;
    }
}
