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
    public static class LoadUnloadExtBehavior
    {
        private static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.RegisterAttached
            (
            "LoadedCommand",
            typeof(ICommand),
            typeof(LoadUnloadExtBehavior),
            new PropertyMetadata(LoadedCommandPropertyChanged)
            );

        public static void SetLoadedCommand(DependencyObject dp, ICommand value) { dp.SetValue(LoadedCommandProperty, value); }
        public static ICommand GetLoadedCommand(DependencyObject dp) { return (ICommand)dp.GetValue(LoadedCommandProperty); }

        private static void LoadedCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is FrameworkElement frameworkElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    frameworkElement.Loaded -= OnLoaded;
                }
                else
                {
                    frameworkElement.Loaded += OnLoaded;
                }
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                ICommand command = GetLoadedCommand(dp);
                if (command?.CanExecute(args) ?? false) command.Execute(args);
                args.Handled = true;
            }
        }

        private static readonly DependencyProperty UnloadedCommandProperty = DependencyProperty.RegisterAttached
            (
            "UnloadedCommand",
            typeof(ICommand),
            typeof(LoadUnloadExtBehavior),
            new PropertyMetadata(UnloadedCommandPropertyChanged)
            );

        public static void SetUnloadedCommand(DependencyObject dp, ICommand value) { dp.SetValue(UnloadedCommandProperty, value); }
        public static ICommand GetUnloadedCommand(DependencyObject dp) { return (ICommand)dp.GetValue(UnloadedCommandProperty); }

        private static void UnloadedCommandPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is FrameworkElement frameworkElement)
            {
                if (args.NewValue == DependencyProperty.UnsetValue || args.NewValue == null)
                {
                    frameworkElement.Unloaded -= OnUnloaded;
                }
                else
                {
                    frameworkElement.Unloaded += OnUnloaded;
                }
            }
        }

        private static void OnUnloaded(object sender, RoutedEventArgs args)
        {
            if (sender is DependencyObject dp)
            {
                ICommand command = GetUnloadedCommand(dp);
                if (command?.CanExecute(args) ?? false) command.Execute(args);
                args.Handled = true;
            }
        }
    }
}