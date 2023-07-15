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

using System;
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.App.Behaviors
{
    public static class KeyboardExtBehavior
    {
        private static readonly DependencyProperty KeyboardButtonDownCommandProperty = DependencyProperty.RegisterAttached
            (
            "KeyboardButtonDownCommand",
            typeof(ICommand),
            typeof(KeyboardExtBehavior),
            new PropertyMetadata(KeyboardButtonDownCommandPropertyChanged)
            );

        public static void SetKeyboardButtonDownCommand(DependencyObject dp, ICommand value) { dp.SetValue(KeyboardButtonDownCommandProperty, value); }
        public static ICommand GetKeyboardButtonDownCommand(DependencyObject dp) { return (ICommand)dp.GetValue(KeyboardButtonDownCommandProperty); }

        private static void KeyboardButtonDownCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.KeyDown -= OnKeyDown;
                }
                else
                {
                    uIElement.KeyDown += OnKeyDown;
                }
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs args)
        {
            if (sender is UIElement uIElement)
            {
                GetKeyboardButtonDownCommand(uIElement)?.Execute(args);
                args.Handled = true;
            }
        }
    }
}