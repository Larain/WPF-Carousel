using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UappUI.AppCode.Touch;

// 1. Поймать событие клика (PrewiewMouseClick в окне)
// 2. Поймать событие перемещения (PrewiewMouseMove в окне)
// 3. Отправить событие Scroll по пути события MouseMove

// How to: Create a Custom Routed Event
// https://msdn.microsoft.com/en-us/library/ms752288%28v=vs.110%29.aspx

// Уровни поддержки многопозиционного ввода
// http://professorweb.ru/my/WPF/base_WPF/level5/5_13.php

// Windows Touch Gestures Overview
// https://msdn.microsoft.com/en-us/library/dd940543%28VS.85%29.aspx

//window.RaiseEvent(e);
//RoutingStrategy.Bubble

namespace Viamo
{
    #region Addition classes

    public enum WheelOrientation
    {
        None = 0,
        Auto = 1,
        Horizontal = 2,
        Vertical = 3,
    }

    #endregion Addition classes

    public class ScrollViewer : System.Windows.Controls.ScrollViewer
    {
        private static MouseWheelEventArgs _lastMouseWheelEventArgs;
        private double _startHorizontalOffset;
        private double _startVerticalOffset;
        private InertiaManager _inertiaManager;

        public WheelOrientation WheelOrientation { get; set; }
        public bool ScrollByContent { get; set; }
        public bool IsInertia { get; set; }
        public double? Sliding { get; set; }
        public double? MinSpeed { get; set; }

        // Constructor
        public ScrollViewer()
            : base()
        {
            WheelOrientation = WheelOrientation.Auto;

            var touchManager = TouchManager.ApplyScrollEvents(this);
            touchManager.GetScrollByContent += () => ScrollByContent;
            //touchManager.GetInertiaTo += (offset, relativeMove, moveTo) => { return moveTo; };
            touchManager.GetScrollDirections += OnGetScrollDirections;
            touchManager.Scroll += OnScroll;
            touchManager.ScrollStopped += OnScrollStopped;

            _inertiaManager = new InertiaManager();
            _inertiaManager.GetSliding += () => Sliding;
            _inertiaManager.GetMinSpeed += () => MinSpeed;
            _inertiaManager.Inertia += OnInertia;

            // Add events to scrollViewer
            AddHandler(MouseDownEvent, new MouseButtonEventHandler((s, e) => _inertiaManager.StopInertia()), true);
            AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnMouseWheelEx), true);
        }

        private ScrollDirections OnGetScrollDirections()
        {
            var isHorizontal = HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled && ScrollableWidth > 0;
            var isVertical = VerticalScrollBarVisibility != ScrollBarVisibility.Disabled && ScrollableHeight > 0;
            return new ScrollDirections { IsHorizontal = isHorizontal, IsVertical = isVertical, IsHandleAll = true, HandledEventsToo = false };
        }

        private void OnScroll(Vector absoluteMove, Vector relativeMove, bool isFirst)
        {
            if (isFirst)
            {
                _startHorizontalOffset = HorizontalOffset;
                _startVerticalOffset = VerticalOffset;
            }

            if (HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                ScrollToHorizontalOffset(_startHorizontalOffset - absoluteMove.X);

            if (VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                ScrollToVerticalOffset(_startVerticalOffset - absoluteMove.Y);

            TransformManager.ScaleElemntsInContainer(this);
        }

        private void OnScrollStopped(Vector absoluteMove, Vector relativeMove, bool isMove)
        {
            if (IsInertia && isMove)
                _inertiaManager.StartScrollInertia(new Vector(HorizontalOffset, VerticalOffset), -relativeMove);
        }

        private void OnInertia(Vector absoluteMove, Vector relativeMove)
        {
            if (HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                ScrollToHorizontalOffset(absoluteMove.X);

            if (VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                ScrollToVerticalOffset(absoluteMove.Y);
        }

        // Lock standard wheel event
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            //base.OnMouseWheel(e);
        }

        private void OnMouseWheelEx(object sender, MouseWheelEventArgs e)
        {
            if (e == _lastMouseWheelEventArgs)
                return;

            if ((WheelOrientation == WheelOrientation.Vertical || WheelOrientation == WheelOrientation.Auto) && VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                if (ScrollableHeight > 0)
                    ScrollToVerticalOffset(VerticalOffset - e.Delta / 4);
                _inertiaManager.StopInertia();
                _lastMouseWheelEventArgs = e;
                e.Handled = true;
            }
            else if ((WheelOrientation == WheelOrientation.Horizontal || WheelOrientation == WheelOrientation.Auto) && HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                if (ScrollableWidth > 0)
                    ScrollToHorizontalOffset(HorizontalOffset - e.Delta / 4);
                _inertiaManager.StopInertia();
                _lastMouseWheelEventArgs = e;
                e.Handled = true;
            }
        }
    }
}
