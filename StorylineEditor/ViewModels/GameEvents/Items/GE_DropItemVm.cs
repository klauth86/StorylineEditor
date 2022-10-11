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

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Предмет: потерять")]
    [XmlRoot]
    public class GE_DropItemVm : GE_ItemBaseVm
    {
        public GE_DropItemVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) { }

        public GE_DropItemVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && Character != null;

        public override void Execute()
        {
            if (IsValid)
            {
                if (CharacterId == CharacterVm.PlayerId) Parent.Parent.Parent.Parent.TreePlayerHistory.DropItem(Item);
            }
        }

        public string CharacterId { get; set; }

        [XmlIgnore]
        public FolderedVm Character
        {
            get => Parent?.Parent?.Parent?.Parent?.Characters.FirstOrDefault(item => item?.Id == CharacterId);
            set
            {
                if (CharacterId != value?.Id)
                {
                    CharacterId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected bool searchCharacterByName;
        public bool SearchCharacterByName
        {
            get => searchCharacterByName;
            set
            {
                if (searchCharacterByName != value)
                {
                    searchCharacterByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool destroyAfterDrop;
        public bool DestroyAfterDrop
        {
            get => destroyAfterDrop;
            set
            {
                if (destroyAfterDrop != value)
                {
                    destroyAfterDrop = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            Character = null;
            SearchCharacterByName = false;
            DestroyAfterDrop = false;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is GE_DropItemVm casted)
            {
                casted.CharacterId = CharacterId;
                casted.searchCharacterByName = searchCharacterByName;
                casted.destroyAfterDrop = destroyAfterDrop;
            }
        }
    }
}