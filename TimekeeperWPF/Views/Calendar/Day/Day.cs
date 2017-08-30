using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TimekeeperWPF
{
    public class Day : Panel
    {
        public Day() : base()
        {
        }

        // Override the default Measure method of Panel
        protected override Size MeasureOverride(Size availableSize)
        {
            //Height will be unbound. Width will be bound to UI space.
            availableSize.Height = double.PositiveInfinity;
            Size panelDesiredSize = new Size();
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableSize);
                panelDesiredSize.Width = child.DesiredSize.Width;
            }

            return panelDesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) { continue; }
                double x = 50;
                double y = 0;
                Size size = child.DesiredSize;
                size.Width = finalSize.Width - x;

                if(child is ICalendarObject)
                {
                    //position CalendarObject on the Calendar according to properties

                }

                child.Arrange(new Rect(new Point(x, y), size));
            }
            return finalSize; // Returns the final Arranged size
        }
    }
}
