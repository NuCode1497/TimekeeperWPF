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
        #region Date
        [Bindable(true)]
        public int SelectedMonthOverride
        {
            get { return (int)GetValue(SelectedMonthOverrideProperty); }
            set { SetValue(SelectedMonthOverrideProperty, value); }
        }
        public static readonly DependencyProperty SelectedMonthOverrideProperty =
            DependencyProperty.Register(
                nameof(SelectedMonthOverride), typeof(int), typeof(Week),
                new FrameworkPropertyMetadata(DateTime.Now.Month,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        private static void OnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
        private static object OnCoerceDate(DependencyObject d, object value)
        {
            Day day = d as Day;
            DateTime newValue = (DateTime)value;
            newValue = newValue.WeekStart();
            return newValue;
        }
        protected bool IsWeekCurrent(DateTime d) { return d.WeekStart() == DateTime.Now.WeekStart(); }
        protected override bool IsDateRelevant(DateTime d) { return d.WeekStart() == Date; }
        #endregion
        #region Highlight
        [Bindable(true), Category("Appearance")]
        public bool ShowMonthBoundsHighlight
        {
            get { return (bool)GetValue(ShowMonthBoundsHighlightProperty); }
            set { SetValue(ShowMonthBoundsHighlightProperty, value); }
        }
        public static readonly DependencyProperty ShowMonthBoundsHighlightProperty =
            DependencyProperty.Register(
                nameof(ShowMonthBoundsHighlight), typeof(bool), typeof(Week),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public Brush MonthBoundsHighlight
        {
            get { return (Brush)GetValue(MonthBoundsHighlightProperty); }
            set { SetValue(MonthBoundsHighlightProperty, value); }
        }
        public static readonly DependencyProperty MonthBoundsHighlightProperty =
            DependencyProperty.Register(
                nameof(MonthBoundsHighlight), typeof(Brush), typeof(Week),
                new FrameworkPropertyMetadata(Brushes.LightGray,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        [Bindable(true), Category("Appearance")]
        public Brush MonthBoundsWatermarkBrush
        {
            get { return (Brush)GetValue(MonthBoundsWatermarkBrushProperty); }
            set { SetValue(MonthBoundsWatermarkBrushProperty, value); }
        }
        public static readonly DependencyProperty MonthBoundsWatermarkBrushProperty =
            DependencyProperty.Register(
                nameof(MonthBoundsWatermarkBrush), typeof(Brush), typeof(Week),
                new FrameworkPropertyMetadata(Brushes.MintCream,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion
        #region Layout
        protected override Size MeasureVertically(Size availableSize, Size extent)
        {
            //Height will be unbound. Width will be bound to UI space.
            double biggestWidth = TextMargin;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                Size childSize = new Size(availableSize.Width, double.PositiveInfinity);
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarkerHorizontal) continue;
                else if (actualChild is NowMarkerVertical)
                {
                    childSize.Width = Math.Max(0, availableSize.Width - TextMargin) / 7d;
                }
                else if (actualChild is CalendarObject)
                {
                    childSize.Width = Math.Max(0, availableSize.Width - TextMargin) / 7d;
                    biggestWidth = Math.Max(biggestWidth, (actualChild.DesiredSize.Width * 7d) + TextMargin);
                }
                else
                {
                    biggestWidth = Math.Max(biggestWidth, actualChild.DesiredSize.Width);
                }
                child.Measure(childSize);
            }
            extent.Width = biggestWidth;
            extent.Height = DaySize(Date);
            return extent;
        }
        protected override Size MeasureHorizontally(Size availableSize, Size extent)
        {   //Width will be unbound. Height will be bound to UI space.
            double biggestHeight = TextMargin;
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                Size childSize = new Size(double.PositiveInfinity, availableSize.Height);
                //unbox the child element
                if ((child as ContentControl)?.Content is UIElement)
                    actualChild = (UIElement)((ContentControl)child).Content;
                if (actualChild is NowMarkerVertical) continue;
                else if (actualChild is NowMarkerHorizontal)
                {
                    childSize.Height = Math.Max(0, availableSize.Height - TextMargin) / 7d;
                }
                else if (actualChild is CalendarObject)
                {
                    childSize.Height = Math.Max(0, availableSize.Height - TextMargin) / 7d;
                    biggestHeight = Math.Max(biggestHeight, (actualChild.DesiredSize.Height * 7d) + TextMargin);
                }
                else
                {
                    biggestHeight = Math.Max(biggestHeight, actualChild.DesiredSize.Height);
                }
                child.Measure(childSize);
            }
            extent.Height = biggestHeight;
            extent.Width = DaySize(Date);
            return extent;
        }
        protected override Size ArrangeVertically(Size arrangeSize, Size extent)
        {
            //Height will be unbound. Width will be bound to UI space.
            double biggestChildWidth = TextMargin;
            double dayWidth = (arrangeSize.Width - TextMargin) / 7d;
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
                if (actualChild is NowMarkerHorizontal)
                {
                    child.Visibility = Visibility.Collapsed;
                    continue;
                }
                else if (actualChild is NowMarkerVertical)
                {
                    if (IsWeekCurrent(Date))
                    {
                        child.Visibility = Visibility.Visible;
                        childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / 7d;
                        x = TextMargin + ((int)DateTime.Now.DayOfWeek * dayWidth);
                        y = (DateTime.Now - DateTime.Now.Date).TotalSeconds / Scale;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarObject)
                {
                    CalendarObject CalObj = actualChild as CalendarObject;
                    if (IsCalObjRelevant(CalObj))
                    {
                        child.Visibility = Visibility.Visible;
                        CalObj.Scale = Scale;
                        childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / 7d;
                        childSize.Height = Math.Max(0, (CalObj.End - CalObj.Start).TotalSeconds / Scale);
                        biggestChildWidth = Math.Max(biggestChildWidth, (childSize.Width * 7d) + TextMargin);
                        //Figure out which day CalObj goes in.
                        //Week days are sequential left to right.
                        //This is configured such that CalendarObjects that normally span across more than 
                        //one day shall have additional copies for each day it occupies in this week, up to 7.
                        //A property 'DayOffset' indicates the copy number and offsets by that number of days.
                        int startDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.Start.Date - Date).TotalDays, 6));
                        int currentDayofWeek = startDayOfWeek + CalObj.DayOffset;
                        DateTime currentDate = Date.AddDays(currentDayofWeek);
                        x = TextMargin + (currentDayofWeek * dayWidth);
                        y = (CalObj.Start - currentDate).TotalSeconds / Scale;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else
                {
                    //y = 0
                    //x = 0
                    biggestChildWidth = Math.Max(biggestChildWidth, childSize.Width);
                }
                child.Arrange(new Rect(new Point(x - Offset.X, y - Offset.Y), childSize));
            }
            extent.Width = biggestChildWidth;
            extent.Height = DaySize(Date);
            return extent;
        }
        protected override Size ArrangeHorizontally(Size arrangeSize, Size extent)
        {   //Width will be unbound. Height will be bound to UI space.
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
                if (actualChild is NowMarkerVertical)
                {
                    child.Visibility = Visibility.Collapsed;
                    continue;
                }
                else if (actualChild is NowMarkerHorizontal)
                {
                    if (IsWeekCurrent(Date))
                    {
                        child.Visibility = Visibility.Visible;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / 7d;
                        y = (int)DateTime.Now.DayOfWeek * dayHeight;
                        x = (DateTime.Now - DateTime.Now.Date).TotalSeconds / Scale;
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                else if (actualChild is CalendarObject)
                {
                    CalendarObject CalObj = actualChild as CalendarObject;
                    if (IsCalObjRelevant(CalObj))
                    {
                        child.Visibility = Visibility.Visible;
                        CalObj.Scale = Scale;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / 7d;
                        childSize.Width = Math.Max(0, (CalObj.End - CalObj.Start).TotalSeconds / Scale);
                        biggestChildHeight = Math.Max(biggestChildHeight, (childSize.Height * 7d) + TextMargin);
                        //Figure out which day CalObj goes in.
                        //Week days are sequential top down.
                        //This is configured such that CalendarObjects that normally span across more than 
                        //one day shall have additional copies for each day it occupies in this week, up to 7.
                        //A property 'DayOffset' indicates the copy number and offsets by that number of days.
                        int startDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.Start.Date - Date).TotalDays, 6));
                        int currentDayofWeek = startDayOfWeek + CalObj.DayOffset;
                        DateTime currentDate = Date.AddDays(currentDayofWeek);
                        y = currentDayofWeek * dayHeight;
                        x = (CalObj.Start - currentDate).TotalSeconds / Scale;
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
            extent.Width = DaySize(Date);
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
            if (IsWeekCurrent(Date))
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
            if (IsWeekCurrent(Date))
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
        protected virtual void DrawMonthBoundsWatermarkText(DrawingContext dc, double textSize, double x, double y, string text)
        {
            FormattedText lineText = new FormattedText(text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(WatermarkFontFamily, FontStyles.Normal,
                FontWeights.Bold, FontStretches.Normal),
                textSize, MonthBoundsWatermarkBrush, null,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            lineText.TextAlignment = TextAlignment.Center;
            dc.DrawText(lineText, new Point(x, y - lineText.Height / 2d));
        }
        protected virtual void DrawSeparators(DrawingContext dc)
        {
            if (Orientation == Orientation.Vertical)
            {
                for (int i = 0; i < 7; i++)
                {
                    double dayWidth = (RenderSize.Width - TextMargin) / 7d;
                    double x = TextMargin + (i * dayWidth);
                    dc.DrawLine(GridRegularPen, new Point(x, 0), new Point(x, RenderSize.Height));
                }
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    double dayHeight = (RenderSize.Height - TextMargin) / 7d;
                    double y = i * dayHeight;
                    dc.DrawLine(GridRegularPen, new Point(0, y), new Point(RenderSize.Width, y));
                }
            }
        }
        #endregion
    }
}
