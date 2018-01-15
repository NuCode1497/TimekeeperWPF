using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;

namespace TimekeeperWPF.Calendar
{
    public class Consumption
    {
        public TimeTaskAllocation Allocation { get; set; }
        public double Remaining;
        public TimeSpan RemainingAsTimeSpan => new TimeSpan((long)Remaining);
        public bool CanAllocate(double alloc)
        {
            return (!Allocation.Limited && Remaining > 0)
                || (Allocation.Limited && Remaining - alloc > 0);
        }
    }
}
