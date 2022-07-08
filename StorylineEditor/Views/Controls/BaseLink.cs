/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StorylineEditor.Views.Controls
{
    public abstract class BaseLink
    {
        protected int BorderThickness;

        public readonly GraphNode From;

        public readonly Polygon LineAndArrow;

        public readonly ContentControl LinkContent;

        public BaseLink(GraphNode from)
        {
            BorderThickness = 10;

            From = from;

            LineAndArrow = new Polygon();
            {
                Binding myBinding = new Binding("DataContext.IsVisible")
                {
                    Converter = App.Current.FindResource("collapseFalseConverter") as IValueConverter,
                    Source = LineAndArrow
                };

                LineAndArrow.SetBinding(UIElement.VisibilityProperty, myBinding);
            }

            LinkContent = new ContentControl();
            {
                Binding myBinding = new Binding("DataContext.IsVisible")
                {
                    Converter = App.Current.FindResource("collapseFalseConverter") as IValueConverter,
                    Source = LinkContent
                };

                LinkContent.SetBinding(UIElement.VisibilityProperty, myBinding);
            }
        }

        protected void UpdateLayout(double fromWidth, double fromHeight, double fromLeft, double fromTop,
            double toWidth, double toHeight, double toLeft, double toTop)
        {
            var touchPointFrom = GetTouchPoint(fromWidth, fromHeight, fromLeft, fromTop, toWidth, toHeight, toLeft, toTop);
            var touchPointTo = GetTouchPoint(toWidth, toHeight, toLeft, toTop, fromWidth, fromHeight, fromLeft, fromTop);

            var direction = (touchPointTo - touchPointFrom);
            direction.Normalize();
            var binormal = new Vector(-direction.Y, direction.X) * BorderThickness;

            var transform = new TranslateTransform
            {
                X = binormal.X * LinkContent.ActualWidth
            };
            LinkContent.LayoutTransform = transform;

            touchPointFrom += binormal;
            touchPointTo += binormal;

            AddArrows(touchPointFrom, touchPointTo, 50);

            PostUpdateLayout(touchPointFrom, touchPointTo);
        }

        protected abstract void PostUpdateLayout(Point fromPoint, Point toPoint);

        protected void AddArrows(Point fromPoint, Point toPoint, int segmentLength)
        {
            var backward = -(toPoint - fromPoint);
            backward.Normalize();
            var normal = new Vector(backward.Y, -backward.X);

            LineAndArrow.Points.Clear();
            LineAndArrow.StrokeThickness = 1;

            int division = 1;
            var length = (fromPoint - toPoint).Length;
            while (length > division * segmentLength) { division *= 2; }

            var from = fromPoint;
            LineAndArrow.Points.Add(from);

            var actualSegment = length / division;

            while (division > 0)
            {
                var to = from - (backward * actualSegment);

                var point1 = to + BorderThickness * backward / 2 + BorderThickness * normal / 4;
                var point2 = to + BorderThickness * backward / 2 - BorderThickness * normal / 4;

                LineAndArrow.Points.Add(to);
                LineAndArrow.Points.Add(point1);
                LineAndArrow.Points.Add(point2);
                LineAndArrow.Points.Add(to);

                from = to;

                division--;
            }
        }

        protected Point GetTouchPoint(double fromWidth, double fromHeight, double fromLeft, double fromTop,
            double toWidth, double toHeight, double toLeft, double toTop)
        {
            var fromC = new Point(fromWidth / 2 + fromLeft, fromHeight / 2 + fromTop);
            var toC = new Point(toWidth / 2 + toLeft, toHeight / 2 + toTop);

            if (fromWidth == 0 && fromHeight == 0) return fromC;

            var dcx = toC.X - fromC.X;
            var dcy = toC.Y - fromC.Y;

            double dx;
            double dy;

            if (Math.Abs(dcx) * fromHeight > Math.Abs(dcy) * fromWidth)
            {
                dx = Math.Sign(toC.X - fromC.X) * (fromWidth / 2 + BorderThickness / 2);
                dy = dcy / dcx * dx;
            }
            else
            {
                dy = Math.Sign(toC.Y - fromC.Y) * (fromHeight / 2 + BorderThickness / 2);
                dx = dcx / dcy * dy;
            }

            var result = fromC;
            result.X += dx;
            result.Y += dy;

            return result;
        }
    }
}