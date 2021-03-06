/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Имеет предмет")]
    public class P_HasItemVm : P_BaseVm
    {
        public P_HasItemVm(Node_BaseVm inParent) : base(inParent) {
            ItemId = null;
            searchByName = false;
        }

        public P_HasItemVm() : this(null) { }

        public override bool IsValid => base.IsValid && Item != null;

        public string ItemId { get; set; }
        
        [XmlIgnore]
        public FolderedVm Item
        {
            get
            {
                return Parent.Parent.Parent.Parent.ItemsTab.Items
                  .FirstOrDefault(item => item?.Id == ItemId);
            }
            set
            {
                if (ItemId != value?.Id)
                {
                    ItemId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool searchByName;
        public bool SearchByName
        {
            get => searchByName;
            set
            {
                if (searchByName != value)
                {
                    searchByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            Item = null;
            SearchByName = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is P_HasItemVm casted)
            {
                casted.ItemId = ItemId;
                casted.searchByName = searchByName;
            }
        }

        public override string GenerateCode(string outerName)
        {
            return SearchByName
                ? string.Format("(inventoryItems.FindByPredicate([](const AActor* item) {{ return item->GetName() == \"{0}\";}}) {1} nullptr)", Item.ActorName, IsInversed ? "==" : "!=")
                : string.Format("(inventoryItems.FindByPredicate([](const AActor* item) {{ return item->GetClass()->GetPathName() == \"{0}\";}}) {1} nullptr)", Item.ClassPathName, IsInversed ? "==" : "!=");
                
        }
    }
}