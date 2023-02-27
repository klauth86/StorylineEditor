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

using System;
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.App.Behaviors
{
    public static class DragDropBehavior
    {
        private static readonly DependencyProperty DragEnabledProperty = DependencyProperty.RegisterAttached
            (
            "DragEnabled",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(DragEnabledPropertyChanged)
            );

        public static void SetDragEnabled(DependencyObject dp, bool value) { dp.SetValue(DragEnabledProperty, value); }
        public static bool GetDragEnabled(DependencyObject dp) { return (bool)dp.GetValue(DragEnabledProperty); }

        private static void DragEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uiElement)
            {
                if (args.NewValue is bool boolNewValue && boolNewValue)
                {
                    uiElement.MouseMove += OnMouseMove;
                }
                else
                {
                    uiElement.MouseMove -= OnMouseMove;
                }
            }
        }

        private static bool IsPossibleDrag = false;

        private static Point PossibleDragStart = new Point();

        private static void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsPossibleDrag)
                {
                    IsPossibleDrag = true;
                    PossibleDragStart = args.GetPosition(App.Current.MainWindow);
                }
                else
                {
                    if ((args.GetPosition(App.Current.MainWindow) - PossibleDragStart).LengthSquared > 4)
                    {
                        if (sender is FrameworkElement frameworkElement)
                        {
                            frameworkElement.QueryContinueDrag += OnQueryContinueDrag;

                            IDataObject dataObject = new DataObject();
                            dataObject.SetData(typeof(object), frameworkElement.DataContext);
                            DragDrop.DoDragDrop(frameworkElement, dataObject, DragDropEffects.All);

                            args.Handled = true;
                        }
                    }
                }
            }
            else
            {
                IsPossibleDrag = false;
            }
        }

        private static void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs args)
        {
            if (args.KeyStates != DragDropKeyStates.LeftMouseButton)
            {
                if (sender is FrameworkElement frameworkElement)
                {
                    frameworkElement.QueryContinueDrag -= OnQueryContinueDrag;
                    args.Action = DragAction.Cancel;
                }
            }
        }



        private static readonly DependencyProperty DragEnterCommandProperty = DependencyProperty.RegisterAttached
            (
            "DragEnterCommand",
            typeof(ICommand),
            typeof(DragDropBehavior)
            );

        public static void SetDragEnterCommand(DependencyObject dp, ICommand value) { dp.SetValue(DragEnterCommandProperty, value); }
        public static ICommand GetDragEnterCommand(DependencyObject dp) { return (ICommand)dp.GetValue(DragEnterCommandProperty); }



        private static readonly DependencyProperty DragLeaveCommandProperty = DependencyProperty.RegisterAttached
            (
            "DragLeaveCommand",
            typeof(ICommand),
            typeof(DragDropBehavior)
            );

        public static void SetDragLeaveCommand(DependencyObject dp, ICommand value) { dp.SetValue(DragLeaveCommandProperty, value); }
        public static ICommand GetDragLeaveCommand(DependencyObject dp) { return (ICommand)dp.GetValue(DragLeaveCommandProperty); }



        private static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached
            (
            "DropCommand",
            typeof(ICommand),
            typeof(DragDropBehavior),
            new PropertyMetadata(DropCommandPropertyChanged)
            );

        public static void SetDropCommand(DependencyObject dp, ICommand value) { dp.SetValue(DropCommandProperty, value); }
        public static ICommand GetDropCommand(DependencyObject dp) { return (ICommand)dp.GetValue(DropCommandProperty); }

        private static void DropCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is UIElement uiElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uiElement.Drop -= OnDrop;
                    uiElement.DragLeave -= OnDragLeave;
                    uiElement.DragOver -= OnDragOver;
                    uiElement.DragEnter -= OnDragEnter;
                }
                else
                {
                    uiElement.DragEnter += OnDragEnter;
                    uiElement.DragOver += OnDragOver;
                    uiElement.DragLeave += OnDragLeave;
                    uiElement.Drop += OnDrop;
                }
            }
        }

        private static bool CanDrop(object sender, DragEventArgs args)
        {
            return sender is FrameworkElement frameworkElement
                && args.Data != null
                && args.Data.GetData(typeof(object)) != null
                && args.Data.GetData(typeof(object)) != frameworkElement.DataContext;
        }

        private static void OnDragEnter(object sender, DragEventArgs args)
        {
            bool canDrop = CanDrop(sender, args);
            if (canDrop && sender is FrameworkElement frameworkElement) GetDragEnterCommand(frameworkElement)?.Execute(frameworkElement.DataContext);
            args.Effects = canDrop ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private static void OnDragOver(object sender, DragEventArgs args)
        {
            args.Effects = CanDrop(sender, args) ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private static void OnDragLeave(object sender, DragEventArgs args)
        {
            bool canDrop = CanDrop(sender, args);
            if (canDrop && sender is FrameworkElement frameworkElement) GetDragLeaveCommand(frameworkElement)?.Execute(frameworkElement.DataContext);
            args.Effects = canDrop ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private static void OnDrop(object sender, DragEventArgs args)
        {
            if (sender is FrameworkElement frameworkElement)
            {
                GetDropCommand(frameworkElement)?.Execute(new Tuple<object, object>(frameworkElement.DataContext, args.Data.GetData(typeof(object))));
                args.Handled = true;
            }
        }
    }
}