﻿// Copyright 2017 (C) Cody Neuburger  All rights reserved.
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
            //Disable TextMargin permanently
            ShowTextMarginProperty.OverrideMetadata(typeof(Month),
                new FrameworkPropertyMetadata(false,
                    null,
                    new CoerceValueCallback((d, v) => false)));
            TextMarginProperty.OverrideMetadata(typeof(Month),
                new FrameworkPropertyMetadata(0,
                    null,
                    new CoerceValueCallback((d, v) => 0)));
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
        protected override int GetRow(DateTime date)
        {
            return ((int)(date.Date - Date).TotalDays.Within(0, Days - 1) + (int)Date.DayOfWeek) / (int)_VisibleRows;
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            DrawSeparators(dc);
        }
        protected override void DrawHighlightVertically(DrawingContext dc)
        {
            double dayWidth = (RenderSize.Width - TextMargin) / 7d;
            Size size = new Size(dayWidth, RenderSize.Height);

            //out of month bounds highlight
            if (ShowMonthBoundsHighlight)
            {
                for (int i = 0; i < 7; i++)
                {
                    DateTime day = Date.AddDays(i);
                    if (day.Month != SelectedMonthOverride)
                    {
                        double x = TextMargin + (i * dayWidth);
                        Point point = new Point(x, 0);
                        Rect rect = new Rect(point, size);
                        dc.DrawRectangle(MonthBoundsHighlight, null, rect);
                    }
                }
            }

            //current day highlight
            if (IsDateTimeRelevant(DateTime.Today))
            {
                double x = TextMargin + ((int)DateTime.Now.DayOfWeek * dayWidth);
                Point point = new Point(x, 0);
                Rect rect = new Rect(point, size);
                dc.DrawRectangle(Highlight, null, rect);
            }
        }
        protected override void DrawHighlightHorizontally(DrawingContext dc)
        {
            double dayHeight = (RenderSize.Height - TextMargin) / 7d;
            Size size = new Size(RenderSize.Width, dayHeight);

            //out of month bounds highlight
            if (ShowMonthBoundsHighlight)
            {
                for (int i = 0; i < 7; i++)
                {
                    DateTime day = Date.AddDays(i);
                    if (day.Month != SelectedMonthOverride)
                    {
                        double y = i * dayHeight;
                        Point point = new Point(0, y);
                        Rect rect = new Rect(point, size);
                        dc.DrawRectangle(MonthBoundsHighlight, null, rect);
                    }
                }
            }

            //current day highlight
            if (IsDateTimeRelevant(DateTime.Today))
            {
                double y = (int)DateTime.Now.DayOfWeek * dayHeight;
                Point point = new Point(0, y);
                Rect rect = new Rect(point, size);
                dc.DrawRectangle(Highlight, null, rect);
            }
        }
        protected override void DrawWatermarkVertically(DrawingContext dc)
        {
            Size daySize = new Size((RenderSize.Width - TextMargin) / 7d, RenderSize.Height);
            double textSize = Math.Max(12d, Math.Min(daySize.Width / 4d, daySize.Height / 4d));
            for (int i = 0; i < 7; i++)
            {
                DateTime day = Date.AddDays(i);
                double x = TextMargin + i * daySize.Width + daySize.Width / 2d;
                double y = daySize.Height / 2d;
                string text = day.ToString(WatermarkFormat);
                if (ShowMonthBoundsHighlight && day.Month != SelectedMonthOverride)
                {
                    DrawMonthBoundsWatermarkText(dc, textSize, x, y, text);
                }
                else
                {
                    DrawWatermarkText(dc, textSize, x, y, text);
                }
            }
        }
        protected override void DrawWatermarkHorizontally(DrawingContext dc)
        {
            Size daySize = new Size(RenderSize.Width, (RenderSize.Height - TextMargin) / 7d);
            double textSize = Math.Max(12d, Math.Min(daySize.Width / 4d, daySize.Height / 4d));
            for (int i = 0; i < 7; i++)
            {
                DateTime day = Date.AddDays(i);
                double x = daySize.Width / 2d;
                double y = i * daySize.Height + daySize.Height / 2d;
                string text = day.ToString(WatermarkFormat);
                if (ShowMonthBoundsHighlight && day.Month != SelectedMonthOverride)
                {
                    Brush temp = WatermarkBrush;
                    WatermarkBrush = MonthBoundsWatermarkBrush;
                    DrawWatermarkText(dc, textSize, x, y, text);
                    WatermarkBrush = temp;
                }
                else
                {
                    DrawWatermarkText(dc, textSize, x, y, text);
                }
            }
        }
        #endregion
    }
}