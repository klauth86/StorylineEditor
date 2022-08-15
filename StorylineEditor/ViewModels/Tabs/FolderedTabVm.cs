/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    public abstract class FolderedTabVm : BaseTabVm<FolderedVm, FullContextVm>, IDragOverable
    {
        public FolderedTabVm(FullContextVm Parent) : base(Parent) { }

        public FolderedTabVm() : this(null) { }

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

        public override void AddImpl(FolderedVm itemToAdd)
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

        public override bool RemoveImpl(FolderedVm itemToRemove)
        {
            SelectedItem.IsSelected = false;

            return itemToRemove.Folder != null ? itemToRemove.Folder.RemoveChild(itemToRemove) : base.RemoveImpl(itemToRemove);
        }

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

        protected static int Compare(FolderedVm a, FolderedVm b)
        {
            if (a.IsFolder && !b.IsFolder) return -1;
            
            if (!a.IsFolder && b.IsFolder) return 1;
            
            int namesCompare =  string.Compare(a.Name, b.Name);
            
            if (namesCompare != 0) return namesCompare;
            
            return string.Compare(a.Id, b.Id);
        }

        public static void AddToCollection(ObservableCollection<FolderedVm> collection, FolderedVm foldered)
        {
            if (collection.Count == 1)
            {
                if (Compare(foldered, collection[0]) < 0)
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

        public static int FindNewIndex(ObservableCollection<FolderedVm> collection, FolderedVm foldered, int left, int right)
        {
            if (Compare(foldered, collection[left]) < 0) return left;

            if (Compare(collection[right], foldered) < 0) return right + 1;

            if (right - left == 1) return right;

            int center = (left + right) / 2;

            return Compare(collection[center], foldered) < 0
                ? FindNewIndex(collection, foldered, center, right)
                : FindNewIndex(collection, foldered, left, center);
        }

        public static void RenameInCollection(FolderedTabVm folderedTab, ObservableCollection<FolderedVm> collection, FolderedVm foldered)
        {
            if (collection.Contains(foldered) && collection.Count > 1)
            {
                int oldIndex = collection.IndexOf(foldered);

                if (collection.Count == 2 &&
                    (
                    Compare(foldered, collection[1 - oldIndex]) < 0 && 1 - oldIndex < oldIndex ||
                    Compare(collection[1 - oldIndex], foldered) < 0 && oldIndex < 1 - oldIndex
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
                        if (Compare(foldered, collection[0]) < 0) newIndex = 0;
                    }
                    else if (oldIndex - 1 > 0)
                    {
                        newIndex = FindNewIndex(collection, foldered, 0, oldIndex - 1);
                    }

                    if (oldIndex + 1 == collection.Count - 1)
                    {
                        if (Compare(collection[collection.Count - 1], foldered) < 0) newIndex = collection.Count - 1;
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
    }
}