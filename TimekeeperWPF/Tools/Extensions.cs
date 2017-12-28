// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Windows;
using System.Windows.Media;
using TimekeeperDAL.Tools;
using TimekeeperWPF.Calendar;

namespace TimekeeperWPF.Tools
{
    public static class Extensions
    {
        //Reminder: Exensions are:
        //public static [type] [MethodName](this [type2] [variableName])
        
        public static T FindAncestor<T>(this DependencyObject d)
            where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(d);
            if (parent == null) return null;
            T found = parent as T;
            return found ?? parent.FindAncestor<T>();
        }

        public static bool IsInside(this CalendarCheckInObject CIO, IZone Z) { return CIO.DateTime.IsInside(Z); }
        public static bool IsInside(this CalendarNoteObject NO, IZone Z) { return NO.DateTime.IsInside(Z); }
    }
}
