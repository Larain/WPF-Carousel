using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace UappUI.AppCode.Touch
{
    #region Addition classes

    public class ScrollDirections
    {
        public bool IsHorizontal { get; set; }
        public bool IsVertical { get; set; }
        public bool IsHandleAll { get; set; }
        public bool HandledEventsToo { get; set; }
    }

    #endregion Addition classes

    public class ScrollTouchManager : TouchManager
    {
        private const int MOUSE_MOVE_MAX_PERIOD = 100;

        public delegate void ScrollEventHandler(Vector absoluteMove, Vector relativeMove, bool isFirst);
        public delegate void ScrollStoppedEventHandler(Vector absoluteMove, Vector relativeMove, bool isMove);

        private bool _isContentCaptured;
        private int _currentIndex;
        private bool _isMouseCaptured; // element.IsMouseCaptured
        private bool _isFirst;
        private Point _startMousePosition;
        private Point _lastMousePosition;
        private bool _isDominanceHorizontal;
        private DateTime _lastMoveTime = DateTime.Now;
        private Vector _relativeMove;
        private Vector _absoluteMove;

        public event Func<bool> GetScrollByContent;
        public event Func<ScrollDirections> GetScrollDirections;
        public event ScrollEventHandler Scroll;
        public event ScrollStoppedEventHandler ScrollStopped;

        internal ScrollDirections ScrollDirections { get; private set; }

        // Constructor
        public ScrollTouchManager(FrameworkElement element)
            : base(element)
        {
            Element.AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(MouseDown), true);
            Element.AddHandler(FrameworkElement.MouseMoveEvent, new MouseEventHandler(MouseMove), true);
            Element.AddHandler(FrameworkElement.MouseUpEvent, new MouseButtonEventHandler(MouseUp), true);
        }

        #region Events

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            var scrollByContent = RaiseGetScrollByContent();
            if (!scrollByContent)
                return;

            if (e.Source != e.OriginalSource && // e.Source is System.Windows.Controls.ScrollViewer &&
                    (e.OriginalSource is System.Windows.Shapes.Rectangle || e.OriginalSource is System.Windows.Controls.Border)) //?
                return;

            ScrollDirections = RaiseGetScrollDirections();
            if (ScrollDirections.IsHorizontal || ScrollDirections.IsVertical)
            {
                SaveAsActive(e);
                _currentIndex = ActiveTouchManagers.Count - 1;

                _isMouseCaptured = false;
                _isContentCaptured = false;
                _isFirst = true;
                AllScrollDirections = null;
                InitPositionKoef();
                MouseDownPosition = GetMousePosition(e);

                // CaptureMouse if I first
                if (_currentIndex == 0)
                    CaptureMouse();
            }
        }

        #region Capture methods

        public void CaptureMouse()
        {
            if (!_isMouseCaptured) // Redundant
            {
                Element.CaptureMouse();  // Auto call MouseMove
                _isMouseCaptured = true; // !!! Set after calling Element.CaptureMouse()
            }
        }

        public void ForceCaptureThisContent(MouseEventArgs e, bool isDominanceHorizontal)
        {
            _isDominanceHorizontal = isDominanceHorizontal;
            var mousePosition = GetMousePosition(e);
            CaptureAnyContent(mousePosition); // This content
            if (AllScrollDirections == null)
                AllScrollDirections = GetTotalScrollDirections(indexFrom: _currentIndex);
        }

        private void CaptureAnyContent(Point mousePosition)
        {
            _isContentCaptured = true;
            _startMousePosition = mousePosition;
            _lastMousePosition = _startMousePosition;
        }

        #endregion Capture methods

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseCaptured)
            {
                var mousePosition = GetMousePosition(e);

                if (!_isContentCaptured)
                {
                    // Init AllScrollDirections
                    if (AllScrollDirections == null)
                    {
                        AllScrollDirections = GetTotalScrollDirections(indexFrom: _currentIndex); 
                    
                        var nextIndex = ActiveTouchManagers.IndexOf(this) + 1;

                        // No more controls or single direction or I use both directions
                        if (nextIndex >= ActiveTouchManagers.Count ||
                            !AllScrollDirections.IsHorizontal ||
                            !AllScrollDirections.IsVertical ||
                            (ScrollDirections.IsHorizontal && ScrollDirections.IsVertical))
                        {
                            CaptureAnyContent(mousePosition); // This content
                            return;
                        }
                    }

                    var absoluteMove = mousePosition - MouseDownPosition;
                    if (IsExceededRange(AllScrollDirections, absoluteMove)) // IsExceededRange needs to AllScrollDirections
                    {
                        CaptureAnyContent(mousePosition); // Any content
                        _isDominanceHorizontal = Math.Abs(absoluteMove.X) >= Math.Abs(absoluteMove.Y);
                        return;
                    }

                    return;
                }

                _relativeMove = mousePosition - _lastMousePosition;
                if (_relativeMove.X == 0 && _relativeMove.Y == 0)
                    return; // Fix bug
                _absoluteMove = mousePosition - _startMousePosition;
                _lastMousePosition = mousePosition;
                _lastMoveTime = DateTime.Now;
                ScrollAll(ActiveTouchManagers.Skip(_currentIndex), _absoluteMove, _relativeMove, _isFirst);
                _isFirst = false;
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseCaptured)
            {
                _isMouseCaptured = false;
                Element.ReleaseMouseCapture();

                var isMove = (DateTime.Now - _lastMoveTime).TotalMilliseconds < MOUSE_MOVE_MAX_PERIOD;
                ScrollStoppedAll(ActiveTouchManagers.Skip(_currentIndex), _absoluteMove, _relativeMove, isMove);
            }
        }

        #endregion Events

        private void ScrollAll(IEnumerable<TouchManager> touchManagers, Vector absoluteMove, Vector relativeMove, bool isFirst)
        {
            EventAll(touchManagers, absoluteMove, relativeMove, isFirst, false);
        }

        private void ScrollStoppedAll(IEnumerable<TouchManager> touchManagers, Vector absoluteMove, Vector relativeMove, bool isMove)
        {
            EventAll(touchManagers, absoluteMove, relativeMove, isMove, true);
        }

        private void EventAll(IEnumerable<TouchManager> touchManagers, Vector absoluteMove, Vector relativeMove, bool flag, bool isStop)
        {
            var horizontalHandled = false;
            var verticalHandled = false;
            foreach (ScrollTouchManager scrollTouchManager in touchManagers.Where(tm => tm is ScrollTouchManager))
            {
                var scrollDirections = scrollTouchManager.ScrollDirections;

                if (_isDominanceHorizontal && AllScrollDirections != null && AllScrollDirections.IsHorizontal && !scrollDirections.IsHorizontal)
                    continue;
                if (!_isDominanceHorizontal && AllScrollDirections != null && AllScrollDirections.IsVertical && !scrollDirections.IsVertical)
                    continue;

                var h = scrollDirections.IsHorizontal && (!horizontalHandled || scrollDirections.HandledEventsToo);
                var v = scrollDirections.IsVertical && (!verticalHandled || scrollDirections.HandledEventsToo);
                if (h || v)
                {
                    var relativeMove1 = relativeMove;
                    var absoluteMove1 = absoluteMove;
                    if (!h)
                    {
                        relativeMove1.X = 0;
                        absoluteMove1.X = 0;
                    }
                    if (!v)
                    {
                        relativeMove1.Y = 0;
                        absoluteMove1.Y = 0;
                    }
                    if (!isStop)
                        scrollTouchManager.RaiseScroll(absoluteMove1, relativeMove1, flag);
                    else
                        scrollTouchManager.RaiseScrollStopped(absoluteMove1, relativeMove1, flag);

                    if (scrollDirections.IsHorizontal || scrollDirections.IsHandleAll)
                        horizontalHandled = true;
                    if (scrollDirections.IsVertical || scrollDirections.IsHandleAll)
                        verticalHandled = true;
                }
            }
        }

        #region Raise methods

        private bool RaiseGetScrollByContent()
        {
            var handler = GetScrollByContent;
            if (handler != null)
                return handler();
            return false;
        }

        private ScrollDirections RaiseGetScrollDirections()
        {
            var handler = GetScrollDirections;
            if (handler != null)
                return handler();
            return new ScrollDirections();
        }

        internal void RaiseScroll(Vector absoluteMove, Vector relativeMove, bool isFirst)
        {
            var handler = Scroll;
            if (handler != null)
                handler(absoluteMove, relativeMove, isFirst);
        }

        internal void RaiseScrollStopped(Vector absoluteMove, Vector relativeMove, bool isMove)
        {
            var handler = ScrollStopped;
            if (handler != null)
                handler(absoluteMove, relativeMove, isMove);
        }

        #endregion Raise methods
    }
}
