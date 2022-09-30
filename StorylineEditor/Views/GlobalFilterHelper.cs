/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.Views.Adorners;

namespace StorylineEditor.Views
{
    public static class GlobalFilterHelper
    {
        public static HashSet<FrameworkElement> Instances = new HashSet<FrameworkElement>();
        

        private static string filter;
        public static string Filter
        {
            get => filter;
            set
            {
                if (value != filter)
                {
                    filter = value;

                    if (string.IsNullOrEmpty(filter))
                    {
                        foreach (var instance in Instances)
                        {
                            instance.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        foreach (var instance in Instances)
                        {
                            if (instance.DataContext is BaseVm dataContext)
                            {
                                instance.Visibility = dataContext.PassFilter(filter) ? Visibility.Visible : Visibility.Collapsed;
                            }
                            else
                            {
                                instance.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
            }
        }


        public static bool GetIsFiltered(DependencyObject obj) => (bool)obj.GetValue(IsFilteredProperty);
        public static void SetIsFiltered(DependencyObject obj, bool value) { obj.SetValue(IsFilteredProperty, value); }

        public static readonly DependencyProperty IsFilteredProperty =
            DependencyProperty.RegisterAttached(
                "IsFiltered",
                typeof(bool),
                typeof(GlobalFilterHelper),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsFilteredChanged));

        private static void IsFilteredChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (obj is FrameworkElement filteredElement)
                {
                    filteredElement.Loaded += OnLoaded;
                    filteredElement.Unloaded += OnUnloaded;
                }
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement instance)
            {
                if (!Instances.Contains(instance))
                {
                    Instances.Add(instance);

                    if (string.IsNullOrEmpty(filter))
                    {
                        instance.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (instance.DataContext is BaseVm dataContext)
                        {
                            instance.Visibility = dataContext.PassFilter(filter) ? Visibility.Visible : Visibility.Collapsed;
                        }
                        else
                        {
                            instance.Visibility = Visibility.Visible;
                        }
                    }

                    if (availabilityAdorners)
                    {
                        if (instance.DataContext is Node_InteractiveVm interactiveNode)
                        {
                            if (interactiveNode.IsAvailable)
                            {
                                RemoveAdorner(instance);
                            }
                            else
                            {
                                AddAdorner(instance);
                            }
                        }
                    }
                }
            }
        }

        private static void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement instance)
            {
                RemoveAdorner(instance);

                if (Instances.Contains(instance)) Instances.Remove(instance);

                instance.Loaded -= OnLoaded;
                instance.Unloaded -= OnUnloaded;
            }
        }


        private static bool availabilityAdorners;
        public static bool AvailabilityAdorners
        {
            get => availabilityAdorners;
            set
            {
                if (value != availabilityAdorners)
                {
                    availabilityAdorners = value;

                    if (availabilityAdorners)
                    {
                        foreach (var instance in Instances)
                        {
                            if (instance.DataContext is Node_InteractiveVm interactiveNode)
                            {
                                if (interactiveNode.IsAvailable)
                                {
                                    RemoveAdorner(instance);
                                }
                                else
                                {
                                    AddAdorner(instance);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var instance in Instances)
                        {
                            if (instance.DataContext is Node_InteractiveVm interactiveNode)
                            {
                                RemoveAdorner(instance);
                            }
                        }
                    }
                }
            }
        }

        private static void AddAdorner(FrameworkElement instance)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(instance);

            adornerLayer.Add(new NotAvailableAdorner(instance));
        }

        private static void RemoveAdorner(FrameworkElement instance)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(instance);

            foreach (var adorner in adornerLayer.GetAdorners(instance))
            {
                adornerLayer.Remove(adorner);
            }
        }
    }
}