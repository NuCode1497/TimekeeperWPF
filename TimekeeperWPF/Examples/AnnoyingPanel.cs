//https://blogs.msdn.microsoft.com/bencon/2006/12/09/iscrollinfo-tutorial-part-iv/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
namespace TimekeeperWPF.Examples
{
    class AnnoyingPanel : Panel,IScrollInfo
    {
        private ScrollViewer _owner;
        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }
        private bool _canHScroll = false;
        private bool _canVScroll = false;
        public bool CanHorizontallyScroll
        {
            get { return _canHScroll; }
            set { _canHScroll=value;}
        }
        public bool CanVerticallyScroll
        {
            get { return _canVScroll; }
            set { _canVScroll=value;}
        }
        private Point _offset;
        public double HorizontalOffset
        {
            get { return _offset.X; }
        }
        public double VerticalOffset
        {
            get { return _offset.Y; }
        }
        public double ExtentHeight
        {
            get { return _extent.Height; }
        }
        public double ExtentWidth
        {
            get { return _extent.Width; }
        }
        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }
        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }
        private TranslateTransform _trans = new TranslateTransform();
        public AnnoyingPanel()
        {
            this.RenderTransform = _trans;
        }
        public void LineUp() { SetVerticalOffset(this.VerticalOffset - 1); }
        public void LineDown() { SetVerticalOffset(this.VerticalOffset + 1); }
        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }
            _offset.Y = offset;
            if (_owner != null)
                _owner.InvalidateScrollInfo();
            _trans.Y = -offset;
        }
        public void LineLeft()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void LineRight()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            for (int i = 0; i < this.InternalChildren.Count; i++)
            {
                if ((Visual)this.InternalChildren[i] == visual)
                {
                    // we found the visual! Let's scroll it into view. First we need to know how big
                    // each child is.
                    Size finalSize = this.RenderSize;
                    Size childSize = new Size(
                        finalSize.Width,
                        (finalSize.Height * 2) / this.InternalChildren.Count);
                    // now we can calculate the vertical offset that we need and set it
                    SetVerticalOffset(childSize.Height * i);
                    // child size is always smaller than viewport, because that is what makes the Panel
                    // an AnnoyingPanel.
                    return rectangle;
                }
            }
            throw new ArgumentException("Given visual is not in this Panel");
        }
        public void MouseWheelUp() { SetVerticalOffset(this.VerticalOffset - 10); }
        public void MouseWheelDown() { SetVerticalOffset(this.VerticalOffset + 10); }
        public void MouseWheelLeft()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void MouseWheelRight()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void PageUp()
        {
            double childHeight = (_viewport.Height * 2) / this.InternalChildren.Count;
            SetVerticalOffset(this.VerticalOffset - childHeight);
        }
        public void PageDown()
        {
            double childHeight = (_viewport.Height * 2) / this.InternalChildren.Count;
            SetVerticalOffset(this.VerticalOffset + childHeight);
        }
        public void PageLeft()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void PageRight()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void SetHorizontalOffset(double offset)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        protected override Size MeasureOverride(Size availableSize)
        {
            Size childSize = new Size(
                availableSize.Width,
                (availableSize.Height * 2) / this.InternalChildren.Count);
            Size extent = new Size(
                availableSize.Width,
                childSize.Height * this.InternalChildren.Count);
            if (extent != _extent)
            {
                _extent = extent;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
            if (availableSize != _viewport)
            {
                _viewport = availableSize;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
            foreach (UIElement child in this.InternalChildren)
            {
                child.Measure(childSize);
            }
            return availableSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Size childSize = new Size(
                finalSize.Width,
                (finalSize.Height * 2) / this.InternalChildren.Count);
            Size extent = new Size(
                finalSize.Width,
                childSize.Height * this.InternalChildren.Count);
            if (extent != _extent)
            {
                _extent = extent;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
            if (finalSize != _viewport)
            {
                _viewport = finalSize;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
            for (int i = 0; i < this.InternalChildren.Count; i++)
            {
                this.InternalChildren[i].Arrange(new Rect(0, childSize.Height * i, childSize.Width, childSize.Height));
            }
            return finalSize;
        }
    }
}
