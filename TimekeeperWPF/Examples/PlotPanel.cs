using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimekeeperWPF.Examples
{
    //https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/how-to-create-a-custom-panel-element
    public class PlotPanel : Panel
    {
        // Default public constructor
        public PlotPanel()
            : base()
        {
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
        // Override the OnRender call to add a Background and Border to the OffSetPanel
        protected override void OnRender(DrawingContext dc)
        {
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Colors.LimeGreen;
            Pen myPen = new Pen(Brushes.Blue, 10);
            Rect myRect = new Rect(0, 0, 300, 300);
            dc.DrawRectangle(mySolidColorBrush, myPen, myRect);
        }
    }
}
