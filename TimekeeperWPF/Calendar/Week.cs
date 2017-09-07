// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TimekeeperWPF.Calendar
{
    public class Week : Day
    {
        static Week()
        {
            DateProperty.OverrideMetadata(typeof(Week),
                new FrameworkPropertyMetadata(DateTime.Now.Date,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    new PropertyChangedCallback(OnDateChanged)));
        }
        public Week() : base()
        {
        }
        #region Date
        private DateTime _Start;
        private DateTime _End;
        public DateTime Start { get { return _End; } }
        public DateTime End { get { return _End; } }
        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Week week = d as Week;
            week._Start = week.Date.Date.AddDays(-(int)week.Date.DayOfWeek);
            week._End = week.Date.Date.AddDays(6 - (int)week.Date.DayOfWeek);
        }
        private static object OnCoerceDate(DependencyObject d, object value)
        {
            Week week = d as Week;
            DateTime newValue = (DateTime)value;
            return newValue;
        }
        #endregion
    }
}
