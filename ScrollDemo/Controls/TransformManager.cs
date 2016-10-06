using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UappUI.AppCode.Touch
{
    public static class TransformManager
    {
        public static void ScaleElemntsInContainer(ScrollViewer listView)
        {
            if (!(listView.Content is StackPanel))
                throw new NotImplementedException("Method does not support yet elements different of StackPanel inside ScrollViewer");
            StackPanel container = (StackPanel)listView.Content;

            int[] visibleItems = GetVisibleItemsFlag(listView);

            double indexOfMiddleElement = visibleItems.Length / 2;
            int midIndex;
            if (IsInteger(indexOfMiddleElement))
                midIndex = (int)indexOfMiddleElement - 1;
            else
                midIndex = (int)indexOfMiddleElement;

            if (midIndex == 0)
                throw new ArgumentException("Please add more items in array");
            double h = 0.5 / midIndex;
            double scaleCoef = 0.5;
            int counter = 0;

            foreach (int i in visibleItems)
            {
                ScaleElement(container.Children[i], scaleCoef, h);
                if (counter < midIndex)
                    scaleCoef += h;
                else
                    scaleCoef -= h;
                counter++;
            }
        }

        private static bool IsInteger(double number)
        {
            return (number % 1) == 0;
        }

        public static int[] GetVisibleItemsFlag(ScrollViewer itemContainer)
        {
            StackPanel container = (StackPanel)itemContainer.Content;
            List<int> visibleItemIndex = new List<int>();

            for (int i = 0; i < container.Children.Count; i++)
                if (IsVisible(container.Children[i], itemContainer))
                    visibleItemIndex.Add(i);

            return visibleItemIndex.ToArray();
        }

        private static bool IsVisible(object child, FrameworkElement scrollViewer)
        {
            if (!(child is FrameworkElement))
                throw new NotImplementedException("It's too hard :)");
            FrameworkElement fchild = (FrameworkElement)child;

            var childTransform = fchild.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), fchild.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }

        private static void ScaleElement(UIElement element, double scaleCoef, double h)
        {
            Storyboard storyboard = new Storyboard();

            ScaleTransform scale = new ScaleTransform(scaleCoef, scaleCoef);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = scale;

            DoubleAnimation growAnimation = new DoubleAnimation();
            growAnimation.Duration = TimeSpan.FromMilliseconds(150);
            //growAnimation.From = scaleCoef + h;
            //growAnimation.To = scaleCoef;

            DoubleAnimation growAnimation2 = new DoubleAnimation();
            growAnimation2.Duration = TimeSpan.FromMilliseconds(150);
            //growAnimation2.From = scaleCoef + h;
            //growAnimation2.To = scaleCoef;

            storyboard.Children.Add(growAnimation);

            Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTargetProperty(growAnimation2, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(growAnimation, element);
            Storyboard.SetTarget(growAnimation2, element);

            storyboard.Begin();
        }

    }
}
