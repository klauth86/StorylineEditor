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
using System.Windows.Input;

namespace StorylineEditor.App.Behaviors
{
    public static class MouseExtBehavior
    {
        private static readonly DependencyProperty MouseLeftButtonDownCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseLeftButtonDownCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseLeftButtonDownCommandPropertyChanged)
            );

        public static void SetMouseLeftButtonDownCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseLeftButtonDownCommandProperty, value); }
        public static ICommand GetMouseLeftButtonDownCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseLeftButtonDownCommandProperty); }

        private static void MouseLeftButtonDownCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
                else
                {
                    uIElement.MouseLeftButtonDown += OnMouseLeftButtonDown;
                }
            }
        }
        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (sender is UIElement uIElement)
            {
                GetMouseLeftButtonDownCommand(uIElement)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseLeftButtonUpCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseLeftButtonUpCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseLeftButtonUpCommandPropertyChanged)
            );

        public static void SetMouseLeftButtonUpCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseLeftButtonUpCommandProperty, value); }
        public static ICommand GetMouseLeftButtonUpCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseLeftButtonUpCommandProperty); }

        private static void MouseLeftButtonUpCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                }
                else
                {
                    uIElement.MouseLeftButtonUp += OnMouseLeftButtonUp;
                }
            }
        }
        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (sender is UIElement uIElement)
            {
                GetMouseLeftButtonUpCommand(uIElement)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseRightButtonDownCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseRightButtonDownCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseRightButtonDownCommandPropertyChanged)
            );

        public static void SetMouseRightButtonDownCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseRightButtonDownCommandProperty, value); }
        public static ICommand GetMouseRightButtonDownCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseRightButtonDownCommandProperty); }

        private static void MouseRightButtonDownCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseRightButtonDown -= OnMouseRightButtonDown;
                }
                else
                {
                    uIElement.MouseRightButtonDown += OnMouseRightButtonDown;
                }
            }
        }
        private static void OnMouseRightButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                GetMouseRightButtonDownCommand(dp)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseRightButtonUpCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseRightButtonUpCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseRightButtonUpCommandPropertyChanged)
            );

        public static void SetMouseRightButtonUpCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseRightButtonUpCommandProperty, value); }
        public static ICommand GetMouseRightButtonUpCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseRightButtonUpCommandProperty); }

        private static void MouseRightButtonUpCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseRightButtonUp -= OnMouseRightButtonUp;
                }
                else
                {
                    uIElement.MouseRightButtonUp += OnMouseRightButtonUp;
                }
            }
        }
        private static void OnMouseRightButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                GetMouseRightButtonUpCommand(dp)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseWheelCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseWheelCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseWheelCommandPropertyChanged)
            );

        public static void SetMouseWheelCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseWheelCommandProperty, value); }
        public static ICommand GetMouseWheelCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseWheelCommandProperty); }

        private static void MouseWheelCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseWheel -= OnMouseWheel;
                }
                else
                {
                    uIElement.MouseWheel += OnMouseWheel;
                }
            }
        }
        private static void OnMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                GetMouseWheelCommand(dp)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseMoveCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseMoveCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseMoveCommandPropertyChanged)
            );

        public static void SetMouseMoveCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseMoveCommandProperty, value); }
        public static ICommand GetMouseMoveCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseMoveCommandProperty); }

        private static void MouseMoveCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseMove -= OnMouseMove;
                }
                else
                {
                    uIElement.MouseMove += OnMouseMove;
                }
            }
        }
        private static void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                GetMouseMoveCommand(dp)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseEnterCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseEnterCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseEnterCommandPropertyChanged)
            );

        public static void SetMouseEnterCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseEnterCommandProperty, value); }
        public static ICommand GetMouseEnterCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseEnterCommandProperty); }

        private static void MouseEnterCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseEnter -= OnMouseEnter;
                }
                else
                {
                    uIElement.MouseEnter += OnMouseEnter;
                }
            }
        }
        private static void OnMouseEnter(object sender, MouseEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                GetMouseEnterCommand(dp)?.Execute(args);
                args.Handled = true;
            }
        }



        private static readonly DependencyProperty MouseLeaveCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseLeaveCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseLeaveCommandPropertyChanged)
            );

        public static void SetMouseLeaveCommand(DependencyObject dp, ICommand value) { dp.SetValue(MouseLeaveCommandProperty, value); }
        public static ICommand GetMouseLeaveCommand(DependencyObject dp) { return (ICommand)dp.GetValue(MouseLeaveCommandProperty); }

        private static void MouseLeaveCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.MouseLeave -= OnMouseLeave;
                }
                else
                {
                    uIElement.MouseLeave += OnMouseLeave;
                }
            }
        }
        private static void OnMouseLeave(object sender, MouseEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                GetMouseLeaveCommand(dp)?.Execute(args);
                args.Handled = true;
            }
        }
    }
}