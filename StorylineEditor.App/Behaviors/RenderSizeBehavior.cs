﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Windows;

namespace StorylineEditor.App.Behaviors
{
    public static class RenderSizeBehavior
    {
        private static readonly DependencyProperty ActualWidthProperty = DependencyProperty.RegisterAttached
            (
            "ActualWidth",
            typeof(double),
            typeof(RenderSizeBehavior)
            );

        public static void SetActualWidth(DependencyObject dp, double value) { dp.SetValue(ActualWidthProperty, value); }

        private static readonly DependencyProperty ActualHeightProperty = DependencyProperty.RegisterAttached
            (
            "ActualHeight",
            typeof(double),
            typeof(RenderSizeBehavior)
            );

        public static void SetActualHeight(DependencyObject dp, double value) { dp.SetValue(ActualHeightProperty, value); }

        private static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached
            (
            "IsEnabled",
            typeof(bool),
            typeof(RenderSizeBehavior),
            new PropertyMetadata(false, IsEnabledPropertyChanged)
            );

        public static void SetIsEnabled(this UIElement inUIElement, bool enable) { inUIElement.SetValue(IsEnabledProperty, enable); }

        private static void IsEnabledPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            if (inDependencyObject is FrameworkElement frameworkElement)
            {
                if (inEventArgs.NewValue == DependencyProperty.UnsetValue || !(bool)inEventArgs.NewValue)
                {
                    frameworkElement.SizeChanged -= OnSizeChanged;
                }
                else
                {
                    SetActualWidth(frameworkElement, frameworkElement.ActualWidth);
                    SetActualHeight(frameworkElement, frameworkElement.ActualHeight);

                    frameworkElement.SizeChanged += OnSizeChanged;
                }
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                if (args.WidthChanged) SetActualWidth(dp, args.NewSize.Width);
                if (args.HeightChanged) SetActualHeight(dp, args.NewSize.Height);
            }
        }
    }
}