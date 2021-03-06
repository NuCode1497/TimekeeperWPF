﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;

namespace TimekeeperDAL.Tools
{
    public class Zone : IZone
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration => End - Start;
        public override string ToString()
        {
            string s = String.Format("{0} S{1} E{2}",
                Start.ToString("y-M-d"),
                Start.ToShortTimeString(),
                End.ToShortTimeString());
            return s;
        }
    }
}
