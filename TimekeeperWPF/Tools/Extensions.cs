// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Windows;
using System.Windows.Media;

namespace TimekeeperWPF.Tools
{
    public static class Extensions
    {
        //Reminder: Exensions are:
        //public static [type] [MethodName](this [type2] [variableName])
        
        //https://stackoverflow.com/questions/2499479/how-to-round-off-hours-based-on-minuteshours0-if-min30-hours1-otherwise
        public static DateTime RoundToHour(this DateTime dt)
        {
            long ticks = dt.Ticks + 18000000000;
            return new DateTime(ticks - ticks % 36000000000, dt.Kind);
        }

        public static double Within(this double d, double min, double max) { return Math.Max(min, Math.Min(d, max)); }
        public static DateTime WeekStart(this DateTime d) { return d.Date.AddDays(-(int)d.DayOfWeek).Date; }
        public static DateTime MonthStart(this DateTime d) { return new DateTime(d.Year, d.Month, 1); }
        public static int DaySeconds(this DateTime d)
        {
            DateTime date = d.Date;
            return (int)(date.AddDays(1) - date).TotalSeconds;
        }
        public static int WeekSeconds(this DateTime d)
        {
            DateTime weekStart = d.WeekStart();
            return (int)(weekStart.AddDays(7) - weekStart).TotalSeconds;
        }
        public static int MonthDays(this DateTime d)
        {
            DateTime monthStart = d.MonthStart();
            return (int)(monthStart.AddMonths(1) - monthStart).TotalDays;
        }
        public static int MonthWeeks(this DateTime d)
        {
            return (int)Math.Ceiling((d.MonthDays() + (int)d.MonthStart().DayOfWeek) / 7d);
        }

        public static T FindAncestor<T>(this DependencyObject d)
            where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(d);
            if (parent == null) return null;
            T found = parent as T;
            return found ?? parent.FindAncestor<T>();
        }

    }
}
