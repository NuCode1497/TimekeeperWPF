﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public class EmptyZone : Zone
    {
        public CalendarTaskObject LeftTangent { get; set; }
        public CalendarTaskObject RightTangent { get; set; }
    }
}
