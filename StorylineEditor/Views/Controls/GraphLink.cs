/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StorylineEditor.Views.Controls
{
    public class GraphLink : BaseLink
    {
        public readonly GraphNode To;

        public GraphLink(GraphNode from, GraphNode to) : base(from)
        {
            To = to;

            LineAndArrow.Stroke = Brushes.Black;
            LineAndArrow.Fill = Brushes.Black;

            LinkContent.Style = App.Current.MainWindow.FindResource("S_ContentControl_GraphLink") as Style;
        }

        public void UpdateLayout()
        {
            UpdateLayout(From.ActualWidth, From.ActualHeight, Canvas.GetLeft(From), Canvas.GetTop(From),
                To.ActualWidth, To.ActualHeight, Canvas.GetLeft(To), Canvas.GetTop(To));
        }

        protected override void PostUpdateLayout(Point fromPoint, Point toPoint)
        {
            var cx = (fromPoint.X + toPoint.X - LinkContent.ActualWidth * GetScaleX()) / 2;
            var cy = (fromPoint.Y + toPoint.Y - LinkContent.ActualHeight * GetScaleY()) / 2;
            Canvas.SetLeft(LinkContent, cx);
            Canvas.SetTop(LinkContent, cy);
        }
    }
}