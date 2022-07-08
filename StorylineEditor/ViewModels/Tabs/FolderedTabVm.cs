/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    public abstract class FolderedTabVm : BaseTabVm<FolderedVm, FullContextVm>, IDragOverable
    {
        public FolderedTabVm(FullContextVm Parent) : base(Parent) { }

        public FolderedTabVm() : this(null) { }

        [XmlIgnore]
        public FolderedVm SelectedItem { get; set; }

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
            ClearSelection();

            return itemToRemove.Folder != null ? itemToRemove.Folder.RemoveChild(itemToRemove) : base.RemoveImpl(itemToRemove);
        }

        public void ClearSelection()
        {
            if (SelectedItem != null)
            {
                SelectedItem.IsSelected = false;
                SelectedItem = null;
            }
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
            return string.Compare(a.Name, b.Name);
        }

        public static void AddToCollection(ObservableCollection<FolderedVm> collection, FolderedVm foldered)
        {
            if (collection.Count > 0 && Compare(foldered, collection.Last()) < 0)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    if (Compare(foldered, collection[i]) <= 0)
                    {
                        collection.Insert(i, foldered);
                        break;
                    }
                }
            }
            else
                collection.Add(foldered);
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
    }
}