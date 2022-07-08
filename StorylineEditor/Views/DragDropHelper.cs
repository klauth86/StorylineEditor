/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.Views
{
    public static class DragDropHelper
    {
        public static bool GetIsDraggable(DependencyObject obj) => (bool)obj.GetValue(IsDraggableProperty);
        public static void SetIsDraggable(DependencyObject obj, bool value) { obj.SetValue(IsDraggableProperty, value); }
        
        public static readonly DependencyProperty IsDraggableProperty =
            DependencyProperty.RegisterAttached(
                "IsDraggable",
                typeof(bool),
                typeof(DragDropHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsDraggableChanged));

        private static void IsDraggableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (obj is FrameworkElement draggedElement)
                {
                    draggedElement.PreviewMouseMove += PreviewMouseMove;
                    draggedElement.Unloaded += Unloaded;
                }
            }
            else
            {
                if (obj is FrameworkElement draggedElement)
                {
                    draggedElement.PreviewMouseMove -= PreviewMouseMove;
                    draggedElement.Unloaded -= Unloaded;
                }
            }
        }

        private static void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                FrameworkElement draggedElement = (FrameworkElement)sender;
                (App.Current.MainWindow as MainWindow).StartDrag(draggedElement, draggedElement.PointToScreen(e.GetPosition(draggedElement)));
                System.Diagnostics.Trace.WriteLine("StartDrag");
            }
        }

        private static void Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement draggedElement = (FrameworkElement)sender;
            draggedElement.PreviewMouseMove -= PreviewMouseMove;
            draggedElement.Unloaded -= Unloaded;
        }
    }
}
