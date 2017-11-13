﻿// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;

namespace TimekeeperDAL.Tools
{
    public static class Extensions
    {
        //Reminder: Exensions are:
        //public static [type] [MethodName](this [type2] [variableName])

        public static string GetTypeName(this object o) { return o.GetType().IgnoreProxy().Name; }

        //EF does some wierd stuff. When using EF Code First, it will create proxy classes at
        //runtime with some bizarre name ending in a string of letters and numbers used for lazy loading. 
        //Doesn't matter most of the time, but if you use nameof(entity.GetType()), you get the proxy class.
        public static Type IgnoreProxy(this Type entityType)
        {
            if (entityType.BaseType != null && entityType.Namespace == "System.Data.Entity.DynamicProxies")
                entityType = entityType.BaseType;
            return entityType;
        }

        public static string Pluralize(this string s)
        {
            return PluralizationService.CreateService(CultureInfo.CurrentCulture).Pluralize(s);
        }
        
        public static int WeekOfMonth(this DateTime dt)
        {
            DateTime first = new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind);
            return dt.WeekOfYear() - first.WeekOfYear() + 1;
        }

        public static int WeekOfYear(this DateTime dt)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        //https://stackoverflow.com/questions/7029353/how-can-i-round-up-the-time-to-the-nearest-x-minutes
        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }

        //https://stackoverflow.com/questions/2499479/how-to-round-off-hours-based-on-minuteshours0-if-min30-hours1-otherwise
        public static DateTime RoundToHour(this DateTime dt)
        {
            long ticks = dt.Ticks + 18000000000;
            return new DateTime(ticks - ticks % 36000000000, dt.Kind);
        }

        public static double Within(this double d, double min, double max) { return Math.Max(min, Math.Min(d, max)); }
        public static DateTime WeekStart(this DateTime dt) { return dt.Date.AddDays(-(int)dt.DayOfWeek).Date; }
        public static DateTime MonthStart(this DateTime dt) { return new DateTime(dt.Year, dt.Month, 1, 0, 0, 0, dt.Kind); }
        public static DateTime YearStart(this DateTime dt) { return new DateTime(dt.Year, 1, 1, 0, 0, 0, dt.Kind); }
        public static DateTime HourStart(this DateTime dt) { return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind); }
        public static int DaySeconds(this DateTime dt)
        {
            DateTime date = dt.Date;
            return (int)(date.AddDays(1) - date).TotalSeconds;
        }
        public static int WeekSeconds(this DateTime dt)
        {
            DateTime weekStart = dt.WeekStart();
            return (int)(weekStart.AddDays(7) - weekStart).TotalSeconds;
        }
        public static int MonthDays(this DateTime dt)
        {
            DateTime monthStart = dt.MonthStart();
            return (int)(monthStart.AddMonths(1) - monthStart).TotalDays;
        }
        public static int MonthWeeks(this DateTime dt)
        {
            return (int)Math.Ceiling((dt.MonthDays() + (int)dt.MonthStart().DayOfWeek) / 7d);
        }
    }
}