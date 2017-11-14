using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimekeeperDAL.EF
{
    public partial class Resource : Filterable
    {
        public override bool HasDateTime(DateTime dt)
        {
            bool result = false;
            foreach (TimeTaskAllocation A in TimeTaskAllocations)
            {
                result = A.TimeTask.HasDateTime(dt);
                //if at least one task allocates this resource at time, return true
                if (result) return true;
            }
            return false;
        }

        [NotMapped]
        public static readonly List<string> TimeResourceChoices = new List<string>()
        {
            "Minute",
            "Hour",
            "Day",
            "Week",
            "Month",
            "Year"
        };

        [NotMapped]
        public bool IsTimeResource => TimeResourceChoices.Contains(Name);

        /// <summary>
        /// A minimum estimate of the duration of the time resource. e.g. Month = 28
        /// </summary>
        public TimeSpan AsTimeSpan()
        {
            TimeSpan allocatedTime = new TimeSpan();
            switch (Name)
            {
                case "Minute":
                    allocatedTime = new TimeSpan(0, 1, 0);
                    break;
                case "Hour":
                    allocatedTime = new TimeSpan(1, 0, 0);
                    break;
                case "Day":
                    allocatedTime = new TimeSpan(1, 0, 0, 0);
                    break;
                case "Week":
                    allocatedTime = new TimeSpan(7, 0, 0, 0);
                    break;
                case "Month":
                    allocatedTime = new TimeSpan(28, 0, 0, 0);
                    break;
                case "Year":
                    allocatedTime = new TimeSpan(365, 0, 0, 0);
                    break;
            }
            return allocatedTime;
        }
    }
}
