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
    public class Week : Day
    {
        #region Constructor
        static Week()
        {
            DateProperty.OverrideMetadata(typeof(Week),
                new FrameworkPropertyMetadata(DateTime.Now.WeekStart(),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault |
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    new PropertyChangedCallback(OnDateChanged),
                    new CoerceValueCallback(OnCoerceDate)));
        }
        public Week() : base()
        {
        }
        #endregion
        #region Events
        protected override void DeterminePosition(MouseEventArgs e)
        {
            if (Orientation == Orientation.Vertical)
            {
                var pos = e.MouseDevice.GetPosition(this);
                var weekDay = (int)((pos.X - TextMargin) / ((RenderSize.Width - TextMargin) / _VisibleColumns)).Within(0, _VisibleColumns - 1);

                var date = Date.AddDays(weekDay);
                var seconds = (int)((pos.Y + Offset.Y) * Scale).Within(0, _Range);
                var time = new TimeSpan(0, 0, seconds);
                MousePosition = date + time;
            }
            else
            {
                var pos = e.MouseDevice.GetPosition(this);
                var weekDay = (int)((pos.Y) / ((RenderSize.Height - TextMargin) / _VisibleColumns)).Within(0, _VisibleColumns - 1);
                var date = Date.AddDays(weekDay);
                var seconds = (int)((pos.X + Offset.X) * Scale).Within(0, _Range);
                var time = new TimeSpan(0, 0, seconds);
                MousePosition = date + time;
            }
        }
        #endregion Events
        #region Date
        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        private static object OnCoerceDate(DependencyObject d, object value)
        {
            DateTime newValue = (DateTime)value;
            newValue = newValue.WeekStart();
            return newValue;
        }
        protected override bool IsDateTimeRelevant(DateTime d) { return d.WeekStart() == Date; }
        #endregion
        #region Layout
        protected override int DefaultVisibleColumns => 7;
        protected override int Days => 7;
        protected override int GetColumn(DateTime date) { return (int)date.DayOfWeek; }
        #endregion
    }
}
