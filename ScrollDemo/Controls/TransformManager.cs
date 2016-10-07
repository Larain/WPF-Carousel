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
    public class TransformManager
    {
        const double MIN_SCALE = 0.2;
        const double MAX_SCALE = Math.PI - 0.2;

        public void ScaleContainerElements(ScrollViewer listView)
        {
            if (!(listView.Content is StackPanel))
                throw new NotImplementedException("Method does not support yet elements different of StackPanel inside ScrollViewer");

            StackPanel container = (StackPanel)listView.Content;

            List<FrameworkElement> containerItems = new List<FrameworkElement>();
            foreach (UIElement item in container.Children)
                containerItems.Add((FrameworkElement)item);

            double currentMaxElementPosition = listView.ActualHeight;
            // To be sure we get the element with max height;
            double currentMinElementPosition = containerItems.OrderByDescending(i => i.ActualHeight).
                FirstOrDefault().ActualHeight * (-1);

            // Getting visible items;
            List<FrameworkElement> visibleItems = new List<FrameworkElement>();
            for (int i = 0; i < container.Children.Count; i++)
                if (IsVisible(container.Children[i], listView))
                    visibleItems.Add((FrameworkElement)container.Children[i]);

            // Scaling elements;
            foreach (FrameworkElement item in visibleItems)
            {
                double mappedHeightValue = GetElementPosition(item, listView).Y.
                    Map(currentMinElementPosition, currentMaxElementPosition, 
                    MIN_SCALE, MAX_SCALE);

                ScaleElement(item, Math.Sin(mappedHeightValue));
            }
        }

        #region protected methods
        protected Point GetElementPosition(FrameworkElement childElement, FrameworkElement absoluteElement)
        {
            return childElement.TransformToAncestor(absoluteElement).Transform(new Point(0, 0));
        }

        protected void ScaleElement(UIElement element, double scaleCoef)
        {
            ScaleTransform scale = new ScaleTransform(scaleCoef, scaleCoef);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = scale;
        }

        protected List<FrameworkElement> GetVisibleItems(ScrollViewer itemContainer)
        {
            // Refactor;
            if (!(itemContainer.Content is StackPanel))
                throw new NotImplementedException("The method need improvments.");

            StackPanel container = (StackPanel)itemContainer.Content;
            List<FrameworkElement> visibleItemList = new List<FrameworkElement>();

            return visibleItemList;
        }
        #endregion

        #region private methods
        private bool IsVisible(object child, FrameworkElement scrollViewer)
        {
            // Refactor;
            if (!(child is FrameworkElement))
                throw new NotImplementedException("The method need improvments.");

            FrameworkElement fchild = (FrameworkElement)child;
            var childTransform = fchild.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), fchild.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }
        #endregion
    }
}
