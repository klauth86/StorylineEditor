﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    public class FolderedComparer : IComparer<FolderedVm>
    {
        public static FolderedComparer Instance = new FolderedComparer();

        public int Compare(FolderedVm a, FolderedVm b)
        {
            if (a.IsFolder && !b.IsFolder) return -1;

            if (!a.IsFolder && b.IsFolder) return 1;

            int namesCompare = string.Compare(a.Name, b.Name);

            if (namesCompare != 0) return namesCompare;

            return string.Compare(a.Id, b.Id);
        }
    }

    public abstract class FolderedTabVm : BaseVm<FullContextVm>, IDragOverable
    {
        public FolderedTabVm(FullContextVm Parent, long additionalTicks) : base(Parent, additionalTicks) { items = new ObservableCollection<FolderedVm>(); }

        public FolderedTabVm() : this(null, 0) { }

        [XmlArray]
        protected ObservableCollection<FolderedVm> items;
        public ObservableCollection<FolderedVm> Items => items;

        protected FolderedVm selectedItem;
        
        [XmlIgnore]
        public FolderedVm SelectedItem { 
            get => selectedItem;
            set
            {
                if (value != selectedItem)
                {
                    selectedItem = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public abstract FolderedVm CreateItem(object parameter);

        ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<object>((parameter) => AddImpl(CreateItem(parameter))));
        public virtual void AddImpl(FolderedVm itemToAdd)
        {
            if (itemToAdd != null)
            {
                if (SelectedItem != null && SelectedItem.IsFolder)
                {
                    SelectedItem.AddChild(itemToAdd);
                    SelectedItem.IsExpanded = true;
                }
                else if (SelectedItem != null && SelectedItem.Folder != null)
                {
                    SelectedItem.Folder.AddChild(itemToAdd);
                    SelectedItem.Folder.IsExpanded = true;
                }
                else
                {
                    AddToCollection(Items, itemToAdd);
                }

                itemToAdd.IsSelected = true;
            }
        }

        ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<FolderedVm>((item) => RemoveImpl(item), (item) => item != null));
        public virtual bool RemoveImpl(FolderedVm itemToRemove)
        {
            SelectedItem.IsSelected = false;

            return (itemToRemove.Folder != null ? itemToRemove.Folder.RemoveChild(itemToRemove) : Items.Remove(itemToRemove)) && itemToRemove.OnRemoval();
        }

        public virtual bool EditItemInPlace => false;

        protected bool isDragOver;
        [XmlIgnore]
        public bool IsDragOver
        {
            get => isDragOver;
            set
            {
                if (value != isDragOver)
                {
                    isDragOver = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public static int FindNewIndex(ObservableCollection<FolderedVm> collection, FolderedVm foldered, int left, int right)
        {
            if (FolderedComparer.Instance.Compare(foldered, collection[left]) < 0) return left;

            if (FolderedComparer.Instance.Compare(collection[right], foldered) < 0) return right + 1;

            if (right - left == 1) return right;

            int center = (left + right) / 2;

            return FolderedComparer.Instance.Compare(collection[center], foldered) < 0
                ? FindNewIndex(collection, foldered, center, right)
                : FindNewIndex(collection, foldered, left, center);
        }

        public static void AddToCollection(ObservableCollection<FolderedVm> collection, FolderedVm foldered)
        {
            if (collection.Count == 1)
            {
                if (FolderedComparer.Instance.Compare(foldered, collection[0]) < 0)
                {
                    collection.Insert(0, foldered);
                }
                else
                {
                    collection.Add(foldered);                
                }
            }
            else if (collection.Count > 1)
            {
                int newIndex = FindNewIndex(collection, foldered, 0, collection.Count - 1);
                if (collection.Count > newIndex)
                {
                    collection.Insert(newIndex, foldered);
                }
                else
                {
                    collection.Add(foldered);
                }
            }
            else { collection.Add(foldered); }
        }

        public static void RenameInCollection(FolderedTabVm folderedTab, ObservableCollection<FolderedVm> collection, FolderedVm foldered)
        {
            if (collection.Contains(foldered) && collection.Count > 1)
            {
                int oldIndex = collection.IndexOf(foldered);

                if (collection.Count == 2 &&
                    (
                    FolderedComparer.Instance.Compare(foldered, collection[1 - oldIndex]) < 0 && 1 - oldIndex < oldIndex ||
                    FolderedComparer.Instance.Compare(collection[1 - oldIndex], foldered) < 0 && oldIndex < 1 - oldIndex
                    ))
                {
                    collection.Move(oldIndex, 1 - oldIndex);
                    foldered.IsSelected = true;
                }
                else
                {
                    int newIndex = oldIndex;

                    if (oldIndex - 1 == 0)
                    {
                        if (FolderedComparer.Instance.Compare(foldered, collection[0]) < 0) newIndex = 0;
                    }
                    else if (oldIndex - 1 > 0)
                    {
                        newIndex = FindNewIndex(collection, foldered, 0, oldIndex - 1);
                    }

                    if (oldIndex + 1 == collection.Count - 1)
                    {
                        if (FolderedComparer.Instance.Compare(collection[collection.Count - 1], foldered) < 0) newIndex = collection.Count - 1;
                    }
                    else if (oldIndex + 1 < collection.Count - 1)
                    {
                        newIndex = FindNewIndex(collection, foldered, oldIndex + 1, collection.Count - 1);
                        if (newIndex == oldIndex + 1)
                        {
                            newIndex = oldIndex;
                        }
                        else 
                        {
                            newIndex--;
                        }
                    }

                    if (newIndex != oldIndex)
                    {
                        collection.Move(oldIndex, newIndex);
                        foldered.IsSelected = true;
                    }
                }
            }
        }

        public static void AddToItem(FolderedVm draggedFoldered, FolderedVm dragOveredFoldered)
        {
            if (draggedFoldered.Folder != null)
                draggedFoldered.Folder.RemoveChild(draggedFoldered);
            else
                (draggedFoldered.Parent as FolderedTabVm)?.Items.Remove(draggedFoldered);

            if (dragOveredFoldered != null)
            {
                if (dragOveredFoldered.IsFolder)
                {
                    dragOveredFoldered.AddChild(draggedFoldered);
                }
                else if (dragOveredFoldered.Folder != null)
                {
                    dragOveredFoldered.Folder.AddChild(draggedFoldered);
                }
                else
                {
                    AddToCollection((draggedFoldered.Parent as FolderedTabVm)?.Items, draggedFoldered);
                }
            }
            else
                AddToCollection((draggedFoldered.Parent as FolderedTabVm)?.Items, draggedFoldered);
        }

        public override void NotifyItemNameChanged(BaseVm renamedVm) { if (renamedVm is FolderedVm foldered) RenameInCollection(this, Items, foldered); }

        public override void SetupParenthood()
        {
            foreach (var item in Items)
            {
                item.Parent = this;
                item.SetupParenthood();
            }
        }

        public void SortItems()
        {
            var orderedItems = Items.OrderBy(foldered => foldered, FolderedComparer.Instance).ToList();
            Items.Clear();
            foreach (var item in orderedItems)
            {
                item.SortItems();
                Items.Add(item);
            }
        }
    }
}