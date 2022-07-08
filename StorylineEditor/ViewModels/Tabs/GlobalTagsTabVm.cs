/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels.Nodes;
using System.Collections.Generic;
using System.Windows.Data;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    public class GlobalTagsTabVm : BaseTabVm<JournalTagVm, FullContextVm> ////// TODO CHECK CLASS NAMINGS FOR ITEM AND TAB
    {
        public GlobalTagsTabVm(FullContextVm Parent) : base(Parent) { }

        public GlobalTagsTabVm() : this(null) { CollectionViewSource.GetDefaultView(Items).Filter = OnFilter; }

        public override JournalTagVm CreateItem(object parameter) => new JournalTagVm(this);

        public override bool RemoveImpl(JournalTagVm tagToRemove)
        {
            CleanCollection(Parent.JournalRecordsTab.Items, tagToRemove);

            CleanCollection(Parent.PlayerDialogsTab.Items, tagToRemove);

            CleanCollection(Parent.ReplicasTab.Items, tagToRemove);

            return Items.Remove(tagToRemove);
        }

        protected string nameFilter;
        [XmlIgnore]
        public string NameFilter
        {
            get => nameFilter;
            set {
                if (nameFilter != value)
                {
                    nameFilter = value;
                    NotifyWithCallerPropName();

                    CollectionViewSource.GetDefaultView(Items).Refresh();
                }
            }
        }

        public IEnumerable<string> TagCategories
        {
            get
            {
                yield return "NONE";
                yield return "CHAPTER";
                yield return "LOCATION";
                yield return "QUEST";
                yield return "GAME";
            }
        }

        protected string categoryFilter;
        [XmlIgnore]
        public string CategoryFilter
        {
            get => categoryFilter;
            set
            {
                if (categoryFilter != value)
                {
                    categoryFilter = value;
                    NotifyWithCallerPropName();

                    CollectionViewSource.GetDefaultView(Items).Refresh();
                }
            }
        }

        protected bool OnFilter(object obj)
        {
            if (obj is JournalTagVm tag)
            {
                return (string.IsNullOrEmpty(nameFilter) || tag.Name.Contains(nameFilter)) &&
                    (string.IsNullOrEmpty(categoryFilter) || tag.TagCategory == categoryFilter);
            }

            return false;
        }

        protected void CleanCollection(ICollection<FolderedVm> collection, JournalTagVm tagToRemove)
        {
            foreach (var item in collection)
            {
                if (item is ITagged tagged) tagged.RemoveTag(tagToRemove.Id);

                if (item is TreeVm tree)
                {
                    foreach (var node in tree.Nodes)
                    {
                        if (node is Node_InteractiveVm iNode)
                        {
                            foreach (var predicate in iNode.Predicates)
                            {
                                if (predicate is ITagged taggedPredicate) taggedPredicate.RemoveTag(tagToRemove.Id);
                            }
                        }
                    }
                }
            }
        }
    }
}