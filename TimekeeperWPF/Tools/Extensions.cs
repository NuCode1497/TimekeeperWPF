using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TimekeeperWPF.Calendar;

namespace TimekeeperWPF.Tools
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

        //https://stackoverflow.com/questions/2499479/how-to-round-off-hours-based-on-minuteshours0-if-min30-hours1-otherwise
        public static DateTime RoundToHour(this DateTime dt)
        {
            long ticks = dt.Ticks + 18000000000;
            return new DateTime(ticks - ticks % 36000000000, dt.Kind);
        }

        public static T FindAncestor<T>(this DependencyObject d)
            where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(d);
            if (parent == null) return null;
            T found = parent as T;
            return found ?? parent.FindAncestor<T>();
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
    }
}
