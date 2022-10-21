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

        public static void SetMouseLeftButtonDownCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseLeftButtonDownCommandProperty, inCommand);
        }
        public static ICommand GetMouseLeftButtonDownCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseLeftButtonDownCommandProperty);
        }

        private static void MouseLeftButtonDownCommandPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.MouseLeftButtonDown += (sender, args) =>
            {
                GetMouseLeftButtonDownCommand(uiElement).Execute(args);
                args.Handled = true;
            };
        }



        private static readonly DependencyProperty MouseLeftButtonUpCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseLeftButtonUpCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseLeftButtonUpCommandPropertyChanged)
            );

        public static void SetMouseLeftButtonUpCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseLeftButtonUpCommandProperty, inCommand);
        }
        public static ICommand GetMouseLeftButtonUpCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseLeftButtonUpCommandProperty);
        }

        private static void MouseLeftButtonUpCommandPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.MouseLeftButtonUp += (sender, args) =>
            {
                GetMouseLeftButtonUpCommand(uiElement).Execute(args);
                args.Handled = true;
            };
        }



        private static readonly DependencyProperty MouseRightButtonDownCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseRightButtonDownCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseRightButtonDownCommandPropertyChanged)
            );

        public static void SetMouseRightButtonDownCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseRightButtonDownCommandProperty, inCommand);
        }
        public static ICommand GetMouseRightButtonDownCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseRightButtonDownCommandProperty);
        }

        private static void MouseRightButtonDownCommandPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.MouseRightButtonDown += (sender, args) =>
            {
                uiElement.CaptureMouse();
                GetMouseRightButtonDownCommand(uiElement).Execute(args);
                args.Handled = true;
            };
        }



        private static readonly DependencyProperty MouseRightButtonUpCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseRightButtonUpCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseRightButtonUpCommandPropertyChanged)
            );

        public static void SetMouseRightButtonUpCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseRightButtonUpCommandProperty, inCommand);
        }
        public static ICommand GetMouseRightButtonUpCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseRightButtonUpCommandProperty);
        }

        private static void MouseRightButtonUpCommandPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.MouseRightButtonUp += (sender, args) =>
            {
                GetMouseRightButtonUpCommand(uiElement).Execute(args);
                args.Handled = true;

                uiElement.ReleaseMouseCapture();
            };
        }



        private static readonly DependencyProperty MouseWheelCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseWheelCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseWheelCommandPropertyChanged)
            );

        public static void SetMouseWheelCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseWheelCommandProperty, inCommand);
        }
        public static ICommand GetMouseWheelCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseWheelCommandProperty);
        }

        private static void MouseWheelCommandPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.MouseWheel += (sender, args) =>
            {
                GetMouseWheelCommand(uiElement).Execute(args);
                args.Handled = true;
            };
        }



        private static readonly DependencyProperty MouseMoveCommandProperty = DependencyProperty.RegisterAttached
            (
            "MouseMoveCommand",
            typeof(ICommand),
            typeof(MouseExtBehavior),
            new PropertyMetadata(MouseMoveCommandPropertyChanged)
            );

        public static void SetMouseMoveCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(MouseMoveCommandProperty, inCommand);
        }
        public static ICommand GetMouseMoveCommand(UIElement inUIElement)
        {
            return (ICommand)inUIElement.GetValue(MouseMoveCommandProperty);
        }

        private static void MouseMoveCommandPropertyChanged(DependencyObject inDependencyObject, DependencyPropertyChangedEventArgs inEventArgs)
        {
            UIElement uiElement = inDependencyObject as UIElement;
            if (null == uiElement) return;

            uiElement.MouseMove += (sender, args) =>
            {
                if (sender == uiElement)
                {
                    GetMouseMoveCommand(uiElement).Execute(args);
                    args.Handled = true;
                }
            };
        }
    }
}