/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace StorylineEditor.Views.Adorners
{
    public class NotAvailableAdorner : Adorner
    {
        static Pen renderPen = new Pen(new SolidColorBrush(Colors.Navy), 1.5);

        public NotAvailableAdorner(UIElement adornedElement) : base(adornedElement) { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.RenderSize);

            drawingContext.DrawLine(renderPen, adornedElementRect.TopLeft, adornedElementRect.BottomRight);
            drawingContext.DrawLine(renderPen, adornedElementRect.BottomLeft, adornedElementRect.TopRight);
        }
    }
}