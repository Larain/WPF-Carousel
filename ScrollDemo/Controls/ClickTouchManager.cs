using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UappUI.AppCode.Touch
{
    public class ClickTouchManager : TouchManager
    {
        private bool _isClickExpectation;
        private bool _isDirectionsCalc;

        public event Action Click;

        // Constructor
        public ClickTouchManager(FrameworkElement element)
            : base(element)
        {
            Element.AddHandler(FrameworkElement.MouseDownEvent, new MouseButtonEventHandler(MouseDown), false);
            Element.AddHandler(FrameworkElement.MouseMoveEvent, new MouseEventHandler(MouseMove), false);
            Element.AddHandler(FrameworkElement.MouseLeaveEvent, new MouseEventHandler(MouseLeave), true);
            Element.AddHandler(FrameworkElement.MouseUpEvent, new MouseButtonEventHandler(MouseUp), false);
        }

        #region Events

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SaveAsActive(e);

                // Activate
                _isClickExpectation = true;
                _isDirectionsCalc = false;
                InitPositionKoef();
                MouseDownPosition = GetMousePosition(e);
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (_isClickExpectation)
            {
                var isHandle = RaiseGetIsHandle();
                if (!isHandle)
                {
                    var nextIndex = ActiveTouchManagers.IndexOf(this) + 1;
                    if (nextIndex < ActiveTouchManagers.Count)
                    {
                        if (!_isDirectionsCalc)
                        {
                            AllScrollDirections = GetTotalScrollDirections(nextIndex);
                            _isDirectionsCalc = true;
                        }

                        var absoluteMove = GetMousePosition(e) - MouseDownPosition;
                        if (IsExceededRange(AllScrollDirections, absoluteMove))
                        {
                            _isClickExpectation = false;
                            if (PassToCaptureMouseAndContent(e, nextIndex, absoluteMove)) // Call CaptureMouse, then MouseLeave
                                return;
                            _isClickExpectation = true;
                        }
                    }
                }
                e.Handled = true; //?
            }
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isClickExpectation)
            {
                _isClickExpectation = false; //? Temp

                var isHandle = RaiseGetIsHandle();
                if (!isHandle)
                {
                    // Lost click expectation
                    var nextIndex = ActiveTouchManagers.IndexOf(this) + 1;
                    if (nextIndex < ActiveTouchManagers.Count)
                    {
                        var absoluteMove = GetMousePosition(e) - MouseDownPosition;
                        _isClickExpectation = false;
                        if (PassToCaptureMouse(e, nextIndex, absoluteMove))
                            return;
                        _isClickExpectation = true;
                    }
                }
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isClickExpectation)
            {
                _isClickExpectation = false;
                RaiseClick();
                e.Handled = true;
            }
            //? ScrollStoppedAll();
        }

        #endregion Events

        #region Raise methods

        private void RaiseClick()
        {
            var handler = Click;
            if (handler != null)
                handler();
        }

        #endregion Raise methods
    }
}
