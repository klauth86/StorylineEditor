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
                ICommand command = GetMouseRightButtonDownCommand(dp);
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
    }
}