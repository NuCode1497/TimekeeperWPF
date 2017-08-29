﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TimekeeperWPF.Views.Calendar.Day
{
    public class Day : Panel
    {
        VisualCollection theVisuals;

        public Day() : base()
        {
        }

        protected override int VisualChildrenCount
        {
            get { return theVisuals.Count; }
        }
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= theVisuals.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return theVisuals[index];
        }
        // Override the default Measure method of Panel
        protected override Size MeasureOverride(Size availableSize)
        {
            Size panelDesiredSize = new Size();

            // In our example, we just have one child. 
            // Report that our panel requires just the size of its only child.
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                panelDesiredSize = child.DesiredSize;
            }

            return panelDesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                double x = 50;
                double y = 50;

                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }
            return finalSize; // Returns the final Arranged size
        }
    }
}
