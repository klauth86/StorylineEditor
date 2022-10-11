/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StorylineEditor.Views.Controls
{
    public class FilteredComboBox : ComboBox
    {
        protected CollectionViewSource filteredCVS = null;

        protected NotifyCollectionChangedEventHandler CollectionChangedHandler = delegate { };

        public FilteredComboBox() { CollectionChangedHandler = OnCollectionChangedHandler; }

        private void OnCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            filteredCVS?.View?.Refresh();
        }

        public IEnumerable FilteredItemsSource
        {
            get => filteredCVS?.View;
            set
            {
                if (value == null)
                {
                    ClearValue(FilteredItemsSourceProperty);
                }
                else
                {
                    SetValue(FilteredItemsSourceProperty, value);
                }
            }
        }

        public static readonly DependencyProperty FilteredItemsSourceProperty = DependencyProperty.Register(
            "FilteredItemsSource", typeof(IEnumerable), typeof(FilteredComboBox), new FrameworkPropertyMetadata(null, OnFilteredItemsSourceChanged));

        private static void OnFilteredItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilteredComboBox comboBox)
            {
                if (comboBox.filteredCVS != null)
                {
                    if (comboBox.filteredCVS.View != null) comboBox.filteredCVS.View.Filter = null;
                    comboBox.filteredCVS = null;
                }

                if (e.NewValue != null)
                {
                    comboBox.filteredCVS = new CollectionViewSource() { Source = e.NewValue };
                    if (comboBox.filteredCVS.View != null)
                    {
                        comboBox.filteredCVS.View.Filter = (o) => comboBox.FilterItem(o);
                        comboBox.filteredCVS.View.MoveCurrentTo(comboBox.SelectedValue);
                    }
                }
                
                d.SetValue(ItemsSourceProperty, comboBox.filteredCVS?.View);
            }
        }

        public IList ExcludeItemsSource
        {
            get => GetValue(ExcludeItemsSourceProperty) as IList;
            set
            {
                if (value == null)
                {
                    ClearValue(ExcludeItemsSourceProperty);
                }
                else
                {
                    SetValue(ExcludeItemsSourceProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ExcludeItemsSourceProperty = DependencyProperty.Register(
            "ExcludeItemsSource", typeof(IList), typeof(FilteredComboBox), new FrameworkPropertyMetadata(null, OnExcludeItemsSourceChanged));

        private static void OnExcludeItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilteredComboBox comboBox)
            {
                if (e.OldValue is INotifyCollectionChanged oldNotifyCollectionChanged)
                {
                    oldNotifyCollectionChanged.CollectionChanged -= comboBox.CollectionChangedHandler;
                }

                if (e.NewValue is INotifyCollectionChanged newNotifyCollectionChanged)
                {
                    newNotifyCollectionChanged.CollectionChanged += comboBox.CollectionChangedHandler;
                }

                comboBox.filteredCVS?.View?.Refresh();
            }
        }

        public string Filter
        {
            get => this.GetValue(FilterProperty)?.ToString();
            set { this.SetValue(FilterProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(FilteredComboBox), new FrameworkPropertyMetadata(null, OnFilterChanged));

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilteredComboBox comboBox)
            {
                comboBox.filteredCVS?.View?.Refresh();
            }
        }

        protected bool FilterItem(object o)
        {
            return
                (string.IsNullOrEmpty(Filter) || o is BaseVm baseVm && baseVm.PassFilter(Filter)) &&
                (ExcludeItemsSource == null || !ExcludeItemsSource.Contains(o));        
        }
    }
}
