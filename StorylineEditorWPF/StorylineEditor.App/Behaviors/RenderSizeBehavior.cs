/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
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

                if (args.WidthChanged || args.HeightChanged) SetSizeChangedFlag(dp, !GetSizeChangedFlag(dp));
            }
        }

        private static readonly DependencyProperty SizeChangedFlagProperty = DependencyProperty.RegisterAttached
            (
            "SizeChangedFlag",
            typeof(bool),
            typeof(RenderSizeBehavior)
            );

        public static void SetSizeChangedFlag(DependencyObject dp, bool value) { dp.SetValue(SizeChangedFlagProperty, value); }
        public static bool GetSizeChangedFlag(DependencyObject dp) { return (bool)dp.GetValue(SizeChangedFlagProperty); }
    }
}