/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
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