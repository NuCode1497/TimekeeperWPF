using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TimekeeperWPF.Calendar
{
    public abstract class CalendarFlairObject : CalendarObject
    {
        public abstract DateTime DateTime { get; }
        public Orientation Orientation { get; set; }
        public int DimensionCount { get; set; }
        public abstract int Dimension { get; }
    }
}
