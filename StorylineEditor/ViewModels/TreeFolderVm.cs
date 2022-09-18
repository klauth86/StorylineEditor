/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Tabs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class TreeFolderVm : FolderedVm
    {
        public TreeFolderVm(BaseVm<FullContextVm> Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            Name = "Новая папка";

            Items = new ObservableCollection<FolderedVm>();

            FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged;
        }

        public TreeFolderVm() : this(null, 0) { }

        public ObservableCollection<FolderedVm> Items { get; }

        public override bool IsFolder => true;

        public override void AddChild(FolderedVm foldered)
        {
            FolderedTabVm.AddToCollection(Items, foldered);

            foldered.Folder = this;
        }

        public override bool RemoveChild(FolderedVm foldered) { foldered.Folder = null; return Items.Remove(foldered); }

        public override bool IsContaining(FolderedVm foldered, bool checkSubs) => Items.Contains(foldered) || checkSubs && Items.Any(item => item.IsContaining(foldered, true));

        public override IEnumerable<FolderedVm> FoldersTraversal()
        {            
            foreach (var foldered in Items)
            {
                foreach (var subFoldered in foldered.FoldersTraversal()) yield return subFoldered;
            }
        }

        public override void SortItems()
        {
            var orderedItems = Items.OrderBy(foldered => foldered, FolderedComparer.Instance).ToList();
            Items.Clear();
            foreach (var item in orderedItems)
            {
                item.SortItems();
                Items.Add(item);
            }
        }

        public override void NotifyItemNameChanged(BaseVm renamedVm) { if (Parent is FolderedTabVm folderedTab && renamedVm is FolderedVm foldered) FolderedTabVm.RenameInCollection(folderedTab, Items, foldered); }

        public override void SetupParenthood()
        {
            foreach (var item in Items)
            {
                item.Parent = Parent;
                item.Folder = this;
                item.SetupParenthood();
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) || 
            Items.Any(item => item.PassFilter(filter));

        public override bool OnRemoval() { FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged; return base.OnRemoval(); }
    }
}