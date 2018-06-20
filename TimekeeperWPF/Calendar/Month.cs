// Copyright 2017 (C) Cody Neuburger  All rights reserved.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimekeeperWPF.Tools;
using TimekeeperWPF;
using System.Reflection;
using System.Windows.Threading;
using System.Collections;
using TimekeeperDAL.Tools;

namespace TimekeeperWPF.Calendar
{
    public class Month : Week
    {
        #region Constructor
        static Month()
        {
            DateProperty.OverrideMetadata(typeof(Month),
                new FrameworkPropertyMetadata(DateTime.Now.MonthStart(),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    new PropertyChangedCallback(OnDateChanged),
                    new CoerceValueCallback(OnCoerceDate)));
            //Force ForceMaxScale permanently on
            ForceMaxScaleProperty.OverrideMetadata(typeof(Month),
                new FrameworkPropertyMetadata(true,
                    null,
                    new CoerceValueCallback((d, v) => true)));
            ShowMonthBoundsHighlightProperty.OverrideMetadata(typeof(Month),
                new FrameworkPropertyMetadata(true,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        }
        public Month() : base()
        {
            _VisibleRows = DateTime.Now.MonthWeeks();
        }
        #endregion
        #region Events
        protected override void DeterminePosition(MouseEventArgs e)
        {
            if (Orientation == Orientation.Vertical)
            {
                var pos = e.MouseDevice.GetPosition(this);
                var weekDay = (int)((pos.X - TextMargin) / ((RenderSize.Width - TextMargin) / 7d)).Within(0, 6);
                var monthWeek = (int)(pos.Y / (RenderSize.Height / _VisibleRows)).Within(0, _VisibleRows - 1);
                var date = Date.WeekStart().AddDays(weekDay + monthWeek * 7);
                var seconds = (int)(pos.Y * Scale - monthWeek * _DAY_SECONDS).Within(0,_DAY_SECONDS);
                var time = new TimeSpan(0, 0, seconds);
                MousePosition = date + time;
            }
            else
            {
            }
        }
        #endregion Events
        #region Date
        private DateTime MonthWeekStart;
        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Month month = d as Month;
            month._VisibleRows = month.Date.MonthWeeks();
            month.MonthWeekStart = month.Date.WeekStart();
        }
        private static object OnCoerceDate(DependencyObject d, object value)
        {
            DateTime newValue = (DateTime)value;
            newValue = newValue.MonthStart();
            return newValue;
        }
        protected override bool IsDateTimeRelevant(DateTime d) { return d.MonthStart() == Date; }
        #endregion
        #region Layout
        protected override int Days => Date.MonthDays();
        protected override DateTime _FirstVisibleDay => Date.WeekStart();
        protected override int GetRow(DateTime date)
        {
            var r =  (int)((date - _FirstVisibleDay).TotalDays.Within(0, Days + (int)Date.DayOfWeek) / _VisibleColumns);
            return r;
        }
        #endregion
    }
}
