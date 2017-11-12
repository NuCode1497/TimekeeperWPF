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
    }
}
