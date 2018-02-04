using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimekeeperDAL.EF;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public abstract class CalendarObject : UserControl, IEquatable<CalendarObject>
    {
        public virtual string BasicString => ToString();
        public virtual bool Equals(CalendarObject other)
        {
            return BasicString.Equals(other.BasicString);
        }
    }
}
