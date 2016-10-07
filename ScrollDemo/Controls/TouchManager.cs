using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace UappUI.AppCode.Touch
{
    public abstract class TouchManager
    {
        private const int HANDLE_RANGE = 15;

        private static MouseEventArgs _lastMouseEventArgs;
        private static List<TouchManager> _allTouchManagers = new List<TouchManager>();
        protected static List<TouchManager> ActiveTouchManagers = new List<TouchManager>();

        #region Static methods

        public static ClickTouchManager ApplyClickEvents(FrameworkElement element)
        {
            var touchManager = new ClickTouchManager(element);
            _allTouchManagers.Add(touchManager);
            return touchManager;
        }

        public static ScrollTouchManager ApplyScrollEvents(FrameworkElement element)
        {
            var touchManager = new ScrollTouchManager(element);
            _allTouchManagers.Add(touchManager);
            return touchManager;
        }

        #endregion Static methods

        private double _kx;
        private double _ky;

        protected FrameworkElement Element;
        protected Point MouseDownPosition;

        // Only for active element: has this and next ActiveTouchManagers Horizontal or Vertical direction
        protected ScrollDirections AllScrollDirections;

        public event Func<bool> GetIsHandle;
        public event Func<double?> GetHandleRange;

        // Constructor
        public TouchManager(FrameworkElement element)
        {
            Element = element;
        }

        #region Protected methods

        #region GetMousePosition

        protected void InitPositionKoef()
        {
            var elementUnitVector = Element.TranslatePoint(new Point(1, 1), null) - Element.TranslatePoint(new Point(0, 0), null);
            _kx = 1 / elementUnitVector.X;
            _ky = 1 / elementUnitVector.Y;
        }

        protected Point GetMousePosition(MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(null);
            mousePosition.X = mousePosition.X * _kx;
            mousePosition.Y = mousePosition.Y * _ky;
            return mousePosition;
        }

        #endregion GetMousePosition

        protected void SaveAsActive(MouseEventArgs mouseEventArgs)
        {
            if (_lastMouseEventArgs != mouseEventArgs)
            {
                _lastMouseEventArgs = mouseEventArgs;
                ActiveTouchManagers.Clear();
            }
            ActiveTouchManagers.Add(this);
        }

        protected ScrollDirections GetTotalScrollDirections(int indexFrom)
        {
            var hasHorizontal = false;
            var hasVertical = false;
            foreach (ScrollTouchManager scrollTouchManager in ActiveTouchManagers.Skip(indexFrom).Where(tm => tm is ScrollTouchManager))
            {
                if (scrollTouchManager.ScrollDirections.IsHorizontal)
                    hasHorizontal = true;
                if (scrollTouchManager.ScrollDirections.IsVertical)
                    hasVertical = true;
            }
            return new ScrollDirections { IsHorizontal = hasHorizontal, IsVertical = hasVertical };
        }

        protected bool IsExceededRange(ScrollDirections scrollDirections, Vector absoluteMove)
        {
            var handleRange = RaiseGetHandleRange() ?? HANDLE_RANGE;

            return (scrollDirections.IsHorizontal && Math.Abs(absoluteMove.X) > handleRange) ||
                   (scrollDirections.IsVertical && Math.Abs(absoluteMove.Y) > handleRange);
        }

        // from MouseMove
        protected bool PassToCaptureMouseAndContent(MouseEventArgs e, int indexFrom, Vector absoluteMove)
        {
            var isDominanceHorizontal = Math.Abs(absoluteMove.X) >= Math.Abs(absoluteMove.Y);

            foreach (ScrollTouchManager scrollTouchManager in ActiveTouchManagers.Skip(indexFrom).Where(tm => tm is ScrollTouchManager))
                if (isDominanceHorizontal && scrollTouchManager.ScrollDirections.IsHorizontal ||
                    !isDominanceHorizontal && scrollTouchManager.ScrollDirections.IsVertical)
                {
                    scrollTouchManager.CaptureMouse();
                    scrollTouchManager.ForceCaptureThisContent(e, isDominanceHorizontal);
                    return true;
                }
            return false;
        }

        // from MouseLeave
        protected bool PassToCaptureMouse(MouseEventArgs e, int indexFrom, Vector absoluteMove)
        {
            foreach (ScrollTouchManager scrollTouchManager in ActiveTouchManagers.Skip(indexFrom).Where(tm => tm is ScrollTouchManager))
            {
                scrollTouchManager.CaptureMouse();
                scrollTouchManager.MouseMove(scrollTouchManager.Element, e); // Check for capture content
                return true;
            }
            return false;
        }

        #endregion Protected methods

        #region Raise methods

        protected bool RaiseGetIsHandle()
        {
            var handler = GetIsHandle;
            if (handler != null)
                return handler();
            return false;
        }

        protected double? RaiseGetHandleRange()
        {
            var handler = GetHandleRange;
            if (handler != null)
                return handler();
            return null;
        }

        #endregion Raise methods
    }
}
