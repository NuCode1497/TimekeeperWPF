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
        #region Events
        #endregion
        #region Features
        #region Date
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
        protected bool IsWeekCurrent => Date.WeekStart() == DateTime.Now.WeekStart();
        #endregion
        #region Scale
        public override double GetMaxScale()
        {
            if (Orientation == Orientation.Vertical)
                return Date.WeekSeconds() / RenderSize.Height;
            else
                return Date.WeekSeconds() / RenderSize.Width;
        }
        #endregion
        #endregion
        #region Layout
        protected override Size MeasureVertically(Size availableSize, Size extent)
        {
            //Height will be unbound. Width will be bound to UI space.
            double biggestWidth = TextMargin; //1D
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                Size childSize = new Size(availableSize.Width, double.PositiveInfinity); //1D
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
                    childSize.Width = Math.Max(0, availableSize.Width - TextMargin) / 7d; //1D
                    biggestWidth = Math.Max(biggestWidth, (actualChild.DesiredSize.Width * 7d) + TextMargin); //1D
                }
                else
                {
                    biggestWidth = Math.Max(biggestWidth, actualChild.DesiredSize.Width); //1D
                }
                child.Measure(childSize);
            }
            extent.Width = biggestWidth; //1D
            extent.Height = DaySize(Date); //1D
            return extent;
        }
        protected override Size MeasureHorizontally(Size availableSize, Size extent)
        {   //Width will be unbound. Height will be bound to UI space.
            double biggestHeight = TextMargin; //1D
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                Size childSize = new Size(double.PositiveInfinity, availableSize.Height); //1D
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
                    childSize.Height = Math.Max(0, availableSize.Height - TextMargin) / 7d; //1D
                    biggestHeight = Math.Max(biggestHeight, (actualChild.DesiredSize.Height * 7d) + TextMargin); //1D
                }
                else
                {
                    biggestHeight = Math.Max(biggestHeight, actualChild.DesiredSize.Height); //1D
                }
                child.Measure(childSize);
            }
            extent.Height = biggestHeight; //1D
            extent.Width = DaySize(Date); //1D
            return extent;
        }
        protected override Size ArrangeVertically(Size arrangeSize, Size extent)
        {
            //Height will be unbound. Width will be bound to UI space.
            double biggestChildWidth = TextMargin; //1D
            double dayWidth = (arrangeSize.Width - TextMargin) / 7d; //1D
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                double x = 0; //1D
                double y = 0; //12:00:00 AM //1D
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
                    if (IsWeekCurrent)
                    {
                        //get day of week that is today
                        child.Visibility = Visibility.Visible;
                        childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / 7d;
                        x = TextMargin + ((int)DateTime.Now.DayOfWeek * dayWidth);
                        y = (DateTime.Now - Date.Date).TotalSeconds / Scale;
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
                    CalObj.Scale = Scale;
                    childSize.Width = Math.Max(0, arrangeSize.Width - TextMargin) / 7d; //1D
                    childSize.Height = Math.Max(0, (CalObj.End - CalObj.Start).TotalSeconds / Scale); //1D
                    //Figure out which day CalObj goes in.
                    //Week days are sequential left to right.
                    //This is configured such that CalendarObjects that normally span across more than 
                    //one day shall have additional copies for each day it occupies in this week, up to 7.
                    //A property DayOffset indicates the copy number and offsets by that number of days.
                    int startDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.Start.Date - Date).TotalDays, 6));
                    x = TextMargin + ((startDayOfWeek + CalObj.DayOffset) * dayWidth); //1D
                    //set y relative to object start //1D
                    //y = 0 is Date = 12:00:00 AM //1D
                    y = (CalObj.Start - Date.Date).TotalSeconds / Scale; //1D
                    biggestChildWidth = Math.Max(biggestChildWidth, (childSize.Width * 7d) + TextMargin); //1D
                }
                else
                {
                    //y = 0
                    //x = 0
                    biggestChildWidth = Math.Max(biggestChildWidth, childSize.Width); //1D
                }
                child.Arrange(new Rect(new Point(x - Offset.X, y - Offset.Y), childSize));
            }
            extent.Width = biggestChildWidth; //1D
            extent.Height = DaySize(Date); //1D
            return extent;
        }
        protected override Size ArrangeHorizontally(Size arrangeSize, Size extent)
        {   //Width will be unbound. Height will be bound to UI space.
            double biggestChildHeight = TextMargin; //1D
            double dayHeight = (arrangeSize.Height - TextMargin) / 7d; //1D
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                UIElement actualChild = child;
                double y = 0; //1D
                double x = 0; //12:00:00 AM //1D
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
                    if (IsWeekCurrent)
                    {
                        //get day of week that is today
                        child.Visibility = Visibility.Visible;
                        childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / 7d;
                        y = (int)DateTime.Now.DayOfWeek * dayHeight;
                        x = (DateTime.Now - Date.Date).TotalSeconds / Scale;
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
                    CalObj.Scale = Scale;
                    childSize.Height = Math.Max(0, arrangeSize.Height - TextMargin) / 7d; //1D
                    childSize.Width = Math.Max(0, (CalObj.End - CalObj.Start).TotalSeconds / Scale); //1D
                    //Figure out which day CalObj goes in.
                    //Week days are sequential top down.
                    //This is configured such that CalendarObjects that normally span across more than 
                    //one day shall have additional copies for each day it occupies in this week, up to 7.
                    //A property DayOffset indicates the copy number and offsets by that number of days.
                    int startDayOfWeek = Math.Max(0, Math.Min((int)(CalObj.Start.Date - Date).TotalDays, 6));
                    y = (startDayOfWeek + CalObj.DayOffset) * dayHeight; //1D
                    //set x relative to object start //1D
                    //x = 0 is Date = 12:00:00 AM //1D
                    x = (CalObj.Start - Date.Date).TotalSeconds / Scale; //1D
                    biggestChildHeight = Math.Max(biggestChildHeight, (childSize.Height * 7d) + TextMargin); //1D
                }
                else
                {
                    //x = 0
                    y = arrangeSize.Height - childSize.Height;
                    biggestChildHeight = Math.Max(biggestChildHeight, childSize.Height); //1D
                }
                child.Arrange(new Rect(new Point(x - Offset.X, y - Offset.Y), childSize));
            }
            extent.Height = biggestChildHeight; //1D
            extent.Width = DaySize(Date); //1D
            return extent;
        }
        protected override void DrawHighlight(DrawingContext dc)
        {
            if (IsWeekCurrent)
            {
                if (Orientation == Orientation.Vertical)
                {
                    Size size = new Size((RenderSize.Width - TextMargin) / 7d, RenderSize.Height);
                    double dayWidth = (RenderSize.Width - TextMargin) / 7d;
                    double x = TextMargin + ((int)DateTime.Now.DayOfWeek * dayWidth);
                    Point point = new Point(x, 0);
                    Rect rect = new Rect(point, size);
                    dc.DrawRectangle(Highlight, null, rect);
                }
                else
                {
                    Size size = new Size(RenderSize.Width, (RenderSize.Height - TextMargin) / 7d);
                    double dayHeight = (RenderSize.Height - TextMargin) / 7d;
                    double y = (int)DateTime.Now.DayOfWeek * dayHeight;
                    Point point = new Point(0, y);
                    Rect rect = new Rect(point, size);
                    dc.DrawRectangle(Highlight, null, rect);
                }
            }
        }
        protected override void DrawWatermark(DrawingContext dc)
        {
            if (Orientation == Orientation.Vertical)
            {
                Size daySize = new Size((RenderSize.Width - TextMargin) / 7d, RenderSize.Height);
                double textSize = Math.Max(12d, Math.Min(daySize.Width / 4d, daySize.Height / 4d));
                for (int i = 0; i < 7; i++)
                {
                    string text = Date.AddDays(i).ToString(WatermarkFormat);
                    FormattedText lineText = new FormattedText(text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(WatermarkFontFamily, FontStyles.Normal,
                        FontWeights.Bold, FontStretches.Normal),
                        textSize, WatermarkBrush, null,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                    lineText.TextAlignment = TextAlignment.Center;
                    double x = TextMargin + i * daySize.Width + daySize.Width / 2d;
                    double y = daySize.Height / 2d - lineText.Height / 2d;
                    dc.DrawText(lineText, new Point(x, y));
                }
            }
            else
            {
                Size daySize = new Size(RenderSize.Width, (RenderSize.Height - TextMargin) / 7d);
                double textSize = Math.Max(12d, Math.Min(daySize.Width / 4d, daySize.Height / 4d));
                for (int i = 0; i < 7; i++)
                {
                    string text = Date.AddDays(i).ToString(WatermarkFormat);
                    FormattedText lineText = new FormattedText(text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(WatermarkFontFamily, FontStyles.Normal,
                        FontWeights.Bold, FontStretches.Normal),
                        textSize, WatermarkBrush, null,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);
                    lineText.TextAlignment = TextAlignment.Center;
                    double x = daySize.Width / 2d;
                    double y = i * daySize.Height + daySize.Height / 2d - lineText.Height / 2d;
                    dc.DrawText(lineText, new Point(x, y));
                }
            }
        }
        protected override void DrawGridVertically(DrawingContext dc, Rect area)
        {
            base.DrawGridVertically(dc, area);
        }
        protected override void DrawGridHorizontally(DrawingContext dc, Rect area)
        {
            base.DrawGridHorizontally(dc, area);
        }
        #endregion
    }
}
