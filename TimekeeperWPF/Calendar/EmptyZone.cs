﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimekeeperWPF.Calendar
{
    public class EmptyZone : Zone
    {
        public CalendarTaskObject Left { get; set; }
        public CalendarTaskObject Right { get; set; }
    }
}