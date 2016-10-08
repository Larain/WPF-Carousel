using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScrollDemo
{
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
            var height = scrollViewer.ActualHeight;
            var center = height / 2;
            var cosK = Math.PI * 0.8 / height;

            List<FrameworkElement> visibleItemes = GetVisibleItems();

            var index = 0;
            foreach (FrameworkElement image in visibleItemes)
            {
                var elementPosition = index * 100 + 50 - center - e.VerticalOffset;

                var scale = Math.Cos(elementPosition * cosK);

                DoubleAnimation heightAnimation = new DoubleAnimation();
                heightAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.05));
                heightAnimation.From = image.ActualHeight;
                heightAnimation.To = itemHeight * scale;
                heightAnimation.FillBehavior = FillBehavior.HoldEnd;
                image.BeginAnimation(Window.HeightProperty, heightAnimation);

                index++;
            }
        }

        protected List<FrameworkElement> GetVisibleItems()
        {
            StackPanel container = (StackPanel)scrollViewer.Content;

            List<FrameworkElement> visibleItems = new List<FrameworkElement>();
            foreach (FrameworkElement item in container.Children)
                if (IsVisible(item, scrollViewer))
                    visibleItems.Add(item);

            return visibleItems;
        }

        private bool IsVisible(FrameworkElement child, FrameworkElement parent)
        {
            FrameworkElement fchild = (FrameworkElement)child;
            var childTransform = fchild.TransformToAncestor(scrollViewer);
            var childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), fchild.RenderSize));
            var ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);
            return ownerRectangle.IntersectsWith(childRectangle);
        }
    }
}
