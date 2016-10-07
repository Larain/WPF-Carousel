using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace UappUI.AppCode.Touch
{
    public class InertiaManager
    {
        public delegate void InertiaEventHandler(Vector absoluteMove, Vector relativeMove);

        private const double SLIDING = 0.92; // 0.97;
        private const double MIN_SPEED = 0.5;

        private Timer _timer;

        public event InertiaEventHandler Inertia;
        public event Func<double?> GetSliding;
        public event Func<double?> GetMinSpeed;

        public bool IsRun { get { return _timer != null; } }

        public void StartScrollInertia(Vector absoluteMove, Vector relativeMove)
        {
            var sliding = RaiseGetSliding() ?? SLIDING;
            var minSpeed = RaiseGetMinSpeed() ?? MIN_SPEED;

            var timer = new Timer(5);
            timer.Elapsed += (s, e) =>
            {
                relativeMove *= sliding; // TotalMove = relativeMove / (1 - sliding)

                var isApproach = Math.Abs(relativeMove.X) < minSpeed && Math.Abs(relativeMove.Y) < minSpeed;
                if (isApproach || Application.Current == null)
                {
                    StopInertia();
                    return;
                }

                absoluteMove += relativeMove;

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (timer.Enabled)
                        RaiseInertia(absoluteMove, relativeMove);
                }), null);

            };
            lock (this)
            {
                if (_timer != null)
                    _timer.Stop(); //?
                _timer = timer;
            }
            timer.Start();
        }

        public void StartSwipeInertia(Vector absoluteMove, Vector relativeMove, bool isMove, Vector inertiaTo)
        {
            StartSwipeInertia(absoluteMove, inertiaTo);
        }

        public void StartSwipeInertia(double currentOffset, double newOffset)
        {
            StartSwipeInertia(new Vector(currentOffset, 0), new Vector(newOffset, 0));
        }

        public void StartSwipeInertia(Vector absoluteMove, Vector inertiaTo)
        {
            var sliding = RaiseGetSliding() ?? SLIDING;
            var minSpeed = RaiseGetMinSpeed() ?? MIN_SPEED;

            var v = (inertiaTo - absoluteMove) * (1 - sliding) / sliding;

            var timer = new Timer(5);
            timer.Elapsed += (s, e) =>
            {
                if (Application.Current == null)
                {
                    StopInertia();
                    return;
                }

                v *= sliding; // TotalMove = v / (1 - sliding)

                var absoluteMove0 = absoluteMove;
                var isApproach = Math.Abs(v.X) < minSpeed && Math.Abs(v.Y) < minSpeed;

                if (isApproach)
                {
                    //var moveRest = inertiaTo.Value.X - absoluteMove.X;
                    //if (Math.Abs(v.X) < Math.Abs(moveRest))
                    //    absoluteMove.X += v.X;
                    //else
                    absoluteMove.X = inertiaTo.X;

                    //var moveRest = inertiaTo.Value.Y - absoluteMove.Y;
                    //if (Math.Abs(relativeMove.Y) < Math.Abs(moveRest))
                    //    absoluteMove.Y += relativeMove.Y;
                    //else
                    absoluteMove.Y = inertiaTo.Y;
                }
                else
                    absoluteMove += v;

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (timer.Enabled)
                        RaiseInertia(absoluteMove, absoluteMove - absoluteMove0);
                }), null);

                if (isApproach && absoluteMove == inertiaTo)
                    StopInertia();
            };
            lock (this)
            {
                if (_timer != null)
                    _timer.Stop(); //?
                _timer = timer;
            }
            timer.Start();
        }

        public void StopInertia()
        {
            lock (this)
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
        }

        #region Raise methods

        private double? RaiseGetSliding()
        {
            var handler = GetSliding;
            if (handler != null)
                return handler();
            return null;
        }

        private double? RaiseGetMinSpeed()
        {
            var handler = GetMinSpeed;
            if (handler != null)
                return handler();
            return null;
        }

        internal void RaiseInertia(Vector absoluteMove, Vector relativeMove)
        {
            var handler = Inertia;
            if (handler != null)
                handler(absoluteMove, relativeMove);
        }

        //internal void RaiseInertiaStopped(Vector absoluteMove, Vector relativeMove, bool isMove)
        //{
        //    var handler = InertiaStopped;
        //    if (handler != null)
        //        handler(absoluteMove, relativeMove, isMove);
        //}

        #endregion Raise methods
    }
}
