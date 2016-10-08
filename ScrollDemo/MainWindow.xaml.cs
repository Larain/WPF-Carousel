using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UappUI.AppCode.Touch;
using static System.Math;

namespace ScrollDemo
{
    public class BaseClass
    {
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double MIN_SCALE = 0.0;
            const double MAX_SCALE = PI;
            var viewHeight = scrollViewer.ActualHeight;
            var itemMaxHeight = 100;

            List<FrameworkElement> visibleItemes = GetVisibleItems();

            foreach (FrameworkElement image in visibleItemes)
            {
                double currentPosition = GetElementPosition(image, this).Y + image.ActualHeight / 2;
                double mappedHeightValue = currentPosition.Map(
                    itemMaxHeight * (-1), viewHeight + itemMaxHeight, MIN_SCALE, MAX_SCALE);

                var scale = Sin(mappedHeightValue);
                
                DoubleAnimation heightAnimation = new DoubleAnimation
                    (image.ActualHeight, itemMaxHeight * scale, 
                    TimeSpan.FromMilliseconds(10), FillBehavior.HoldEnd);
                image.BeginAnimation(HeightProperty, heightAnimation);
            }
        }

        protected Point GetElementPosition(FrameworkElement childElement, FrameworkElement absoluteElement)
        {
            return childElement.TransformToAncestor(absoluteElement).Transform(new Point(0, 0));
        }
        protected List<FrameworkElement> GetVisibleItems()
        {
            StackPanel container = (StackPanel)scrollViewer.Content;

            List<FrameworkElement> visibleItems = new List<FrameworkElement>();
            foreach (FrameworkElement item in container.Children)
                if (IsItemVisible(item, scrollViewer))
                    visibleItems.Add(item);

            return visibleItems;
        }
        private bool IsItemVisible(FrameworkElement child, FrameworkElement parent)
        {
            var childTransform = child.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }
    }
}
