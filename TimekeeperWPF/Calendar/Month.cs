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
        }
        #endregion
        #region Events
        protected override void DeterminePosition(MouseEventArgs e)
        {
            if (Orientation == Orientation.Vertical)
            {
                var pos = e.MouseDevice.GetPosition(this);
                var weekDay = (int)((pos.X - TextMargin) / ((RenderSize.Width - TextMargin) / 7d)).Within(0, 6);
                var monthWeek = (int)(pos.Y / (RenderSize.Height / MonthWeeks)).Within(0, MonthWeeks - 1);
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
        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Month month = d as Month;
            month.MonthWeeks = month.Date.MonthWeeks();
            month.MonthWeekStart = month.Date.WeekStart();
        }
        private int MonthWeeks;
        private DateTime MonthWeekStart;
        private static object OnCoerceDate(DependencyObject d, object value)
        {
            DateTime newValue = (DateTime)value;
            newValue = newValue.MonthStart();
            return newValue;
        }
        protected override bool IsDateTimeRelevant(DateTime d) { return d.MonthStart() == Date; }
        #endregion
        #region Layout
        protected override int NowColumn => (int)DateTime.Now.DayOfWeek;
        protected override int NowRow => GetMonthRow(DateTime.Now);
        private int GetMonthRow(DateTime date)
        {
            return ((int)(date.Date - Date).TotalDays.Within(0, Date.MonthDays() - 1) + (int)Date.DayOfWeek) / MonthWeeks;
        }
        protected override Size ArrangeVertically(Size arrangeSize, Size extent)
        {
            Size cellSize = arrangeSize;
            cellSize.Width = (arrangeSize.Width - TextMargin) / _VisibleColumns;
            cellSize.Height = arrangeSize.Height / _VisibleRows;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                double x = 0;
                double y = 0;
                Size childSize = child.DesiredSize;
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarker)
                {
                    if (IsDateTimeRelevant(DateTime.Today))
                    {
                        child.Visibility = Visibility.Visible;
                        childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / _VisibleColumns;
                        x = TextMargin + NowColumn * cellSize.Width;
                        y = (DateTime.Now - DateTime.Now.Date).TotalSeconds / Scale - childSize.Height / 2 + NowRow * cellSize.Height;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarTaskObject)
                {
                    CalendarTaskObject CalObj = actualChild as CalendarTaskObject;
                    if (IsCalObjRelevant(CalObj))
                    {
                        child.Visibility = Visibility.Visible;
                        //Size
                        childSize.Width = cellSize.Width / CalObj.DimensionCount;
                        childSize.Height = Math.Max(0, (CalObj.End - CalObj.Start).TotalSeconds / Scale);
                        //Position
                        var startDayOfMonth = (int)(CalObj.Start.Date - Date).TotalDays.Within(0, Date.MonthDays() - 1);
                        var currentDayOfMonth = startDayOfMonth + CalObj.DayOffset;
                        var currentDate = Date.AddDays(currentDayOfMonth);
                        var column = (int)currentDate.DayOfWeek;
                        var row = GetMonthRow(currentDate);
                        var cellX = column * cellSize.Width;
                        var cellY = row * cellSize.Height;
                        var dimensionOffset = childSize.Width * CalObj.Dimension;
                        x = TextMargin + cellX + dimensionOffset;
                        y = cellY + (CalObj.Start - currentDate).TotalSeconds / Scale;
                        //Cut off excess
                        if (y < cellY)
                        {
                            childSize.Height += y;
                            y = 0;
                        }
                        else if (y + childSize.Height > cellY + cellSize.Height)
                        {
                            childSize.Height = y + childSize.Height - cellY + cellSize.Height;
                        }
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarNoteObject)
                {
                    CalendarNoteObject CalObj = actualChild as CalendarNoteObject;
                    if (IsDateTimeRelevant(CalObj.DateTime))
                    {
                        var sections = 7d * CalObj.DimensionCount;
                        child.Visibility = Visibility.Visible;
                        childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / sections;
                        var dimensionOffset = childSize.Width * CalObj.Dimension;
                        x = TextMargin + ((int)CalObj.DateTime.DayOfWeek * cellSize.Width) + dimensionOffset;
                        y = (CalObj.DateTime.TimeOfDay).TotalSeconds / Scale - childSize.Height / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarCheckInObject)
                {
                    CalendarCheckInObject CalObj = actualChild as CalendarCheckInObject;
                    if (IsDateTimeRelevant(CalObj.DateTime))
                    {
                        var sections = 7d * CalObj.DimensionCount;
                        child.Visibility = Visibility.Visible;
                        childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / sections;
                        var dimensionOffset = childSize.Width * CalObj.Dimension;
                        x = TextMargin + ((int)CalObj.DateTime.DayOfWeek * cellSize.Width) + dimensionOffset;
                        y = (CalObj.DateTime.TimeOfDay).TotalSeconds / Scale - childSize.Height / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else
                {
                }
                child.Arrange(new Rect(new Point(x - Offset.X, y - Offset.Y), childSize));
            }
            extent.Width = arrangeSize.Width;
            extent.Height = _DaySize;
            return extent;
        }
        protected override Size ArrangeHorizontally(Size arrangeSize, Size extent)
        {
            //Width will be unbound. Height will be bound to UI space.
            double biggestChildHeight = TextMargin;
            double dayHeight = (arrangeSize.Height - TextMargin) / 7d;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                double y = 0;
                double x = 0;
                Size childSize = child.DesiredSize;
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarker)
                {
                    if (IsDateTimeRelevant(DateTime.Today))
                    {
                        child.Visibility = Visibility.Visible;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / 7d;
                        y = (int)DateTime.Now.DayOfWeek * dayHeight;
                        x = (DateTime.Now - DateTime.Now.Date).TotalSeconds / Scale - childSize.Width / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarTaskObject)
                {
                    CalendarTaskObject CalObj = actualChild as CalendarTaskObject;
                    if (IsCalObjRelevant(CalObj))
                    {
                        var sections = 7d * CalObj.DimensionCount;
                        child.Visibility = Visibility.Visible;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / sections;
                        childSize.Width = Math.Max(0, (CalObj.End - CalObj.Start).TotalSeconds / Scale);
                        biggestChildHeight = Math.Max(biggestChildHeight, (childSize.Height * sections) + TextMargin);
                        int startDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.Start.Date - Date).TotalDays, 6));
                        int currentDayofWeek = startDayOfWeek + CalObj.DayOffset;
                        DateTime currentDate = Date.AddDays(currentDayofWeek);
                        var dimensionOffset = childSize.Height * CalObj.Dimension;
                        y = currentDayofWeek * dayHeight + dimensionOffset;
                        x = (CalObj.Start - currentDate).TotalSeconds / Scale;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarNoteObject)
                {
                    CalendarNoteObject CalObj = actualChild as CalendarNoteObject;
                    if (IsDateTimeRelevant(CalObj.DateTime))
                    {
                        var sections = 7d * CalObj.DimensionCount;
                        child.Visibility = Visibility.Visible;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / sections;
                        var dimensionOffset = childSize.Width * CalObj.Dimension;
                        y = ((int)CalObj.DateTime.DayOfWeek * dayHeight) + dimensionOffset;
                        x = (CalObj.DateTime.TimeOfDay).TotalSeconds / Scale - childSize.Width / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarCheckInObject)
                {
                    CalendarCheckInObject CalObj = actualChild as CalendarCheckInObject;
                    if (IsDateTimeRelevant(CalObj.DateTime))
                    {
                        var sections = 7d * CalObj.DimensionCount;
                        child.Visibility = Visibility.Visible;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / sections;
                        var dimensionOffset = childSize.Width * CalObj.Dimension;
                        y = ((int)CalObj.DateTime.DayOfWeek * dayHeight) + dimensionOffset;
                        x = (CalObj.DateTime.TimeOfDay).TotalSeconds / Scale - childSize.Width / 2;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else
                {
                    //x = 0
                    y = arrangeSize.Height - childSize.Height;
                    biggestChildHeight = Math.Max(biggestChildHeight, childSize.Height);
                }
                child.Arrange(new Rect(new Point(x - Offset.X, y - Offset.Y), childSize));
            }
            extent.Height = biggestChildHeight;
            extent.Width = _DaySize;
            return extent;
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
