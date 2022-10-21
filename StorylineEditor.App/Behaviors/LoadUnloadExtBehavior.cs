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