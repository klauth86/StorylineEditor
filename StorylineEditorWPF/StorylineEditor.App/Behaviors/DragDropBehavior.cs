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
            if (dp is UIElement uIElement)
            {
                if (args.NewValue is bool boolNewValue && boolNewValue)
                {
                    uIElement.MouseMove += OnMouseMove;
                }
                else
                {
                    uIElement.MouseMove -= OnMouseMove;
                }
            }
        }
        private static void OnMouseMove(object sender, MouseEventArgs args)
        {
            if (args.RightButton == MouseButtonState.Released && args.LeftButton == MouseButtonState.Pressed && args.MiddleButton == MouseButtonState.Released)
            {
                if (sender is FrameworkElement frameworkElement)
                {
                    IDataObject dataObject = new DataObject();
                    dataObject.SetData(typeof(object), frameworkElement.DataContext);
                    DragDrop.DoDragDrop(frameworkElement, dataObject, DragDropEffects.All);

                    args.Handled = true;
                }
            }
        }



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
            if (dp is UIElement uIElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    uIElement.Drop -= OnDrop;
                    uIElement.DragOver -= OnDragOver;
                }
                else
                {
                    uIElement.DragOver += OnDragOver;
                    uIElement.Drop += OnDrop;
                }
            }
        }

        private static void OnDragOver(object sender, DragEventArgs args)
        {
            args.Effects = DragDropEffects.None;

            if (sender is FrameworkElement frameworkElement)
            {
                if (args.Data != null && args.Data.GetData(typeof(object)) != null && args.Data.GetData(typeof(object)) != frameworkElement.DataContext)
                {
                    args.Effects = DragDropEffects.Copy;
                }
            }

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