/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Windows;

namespace StorylineEditor.Views
{
    public static class GlobalFilterHelper
    {
        public static HashSet<FrameworkElement> Instances = new HashSet<FrameworkElement>();

        public static bool GetIsFiltered(DependencyObject obj) => (bool)obj.GetValue(IsFilteredProperty);
        public static void SetIsFiltered(DependencyObject obj, bool value) { obj.SetValue(IsFilteredProperty, value); }

        public static readonly DependencyProperty IsFilteredProperty =
            DependencyProperty.RegisterAttached(
                "IsFiltered",
                typeof(bool),
                typeof(GlobalFilterHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsFilteredChanged));

        private static void IsFilteredChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (obj is FrameworkElement filteredElement)
                {
                    filteredElement.Loaded += OnLoaded;
                    filteredElement.Unloaded += OnUnloaded;
                }
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement filteredElement)
            {
                if (!Instances.Contains(filteredElement)) Instances.Add(filteredElement);
            }
        }

        private static void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement filteredElement)
            {
                if (Instances.Contains(filteredElement)) Instances.Remove(filteredElement);

                filteredElement.Loaded -= OnLoaded;
                filteredElement.Unloaded -= OnUnloaded;
            }
        }
    }
}
