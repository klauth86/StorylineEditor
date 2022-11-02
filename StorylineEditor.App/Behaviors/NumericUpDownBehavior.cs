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

namespace StorylineEditor.App.Behaviors
{
    public static class NumericUpDownBehavior
    {
        private static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached
            (
            "IsEnabled",
            typeof(bool),
            typeof(NumericUpDownBehavior),
            new PropertyMetadata(false, IsEnabledPropertyChanged)
            );

        public static void SetIsEnabled(this UIElement inUIElement, bool value) { inUIElement.SetValue(IsEnabledProperty, value); }

        private static void IsEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is FrameworkElement frameworkElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null || !(bool)args.NewValue)
                {
                    frameworkElement.SizeChanged -= OnSizeChanged;
                }
                else
                {
                    frameworkElement.SizeChanged += OnSizeChanged;
                }
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (sender is DependencyObject dp)
            {

            }
        }

        private static readonly DependencyProperty NumericModifierProperty = DependencyProperty.RegisterAttached
            (
            "NumericModifier",
            typeof(float),
            typeof(NumericUpDownBehavior),
            new PropertyMetadata(false, NumericModifierPropertyChanged)
            );

        public static void SetNumericModifier(this UIElement inUIElement, float value) { inUIElement.SetValue(NumericModifierProperty, value); }

        private static void NumericModifierPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is FrameworkElement frameworkElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null || (float)args.NewValue == 0)
                {
                    frameworkElement.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
                else
                {
                    frameworkElement.MouseLeftButtonDown += OnMouseLeftButtonDown;
                }
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}