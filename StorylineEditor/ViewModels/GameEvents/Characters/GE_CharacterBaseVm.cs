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
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [XmlRoot]
    public abstract class GE_CharacterBaseVm : GE_BaseVm
    {
        public GE_CharacterBaseVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks)
        {
            CharacterId = null;
            searchByName = false;
            affectAll = false;
        }

        public GE_CharacterBaseVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && Character != null;

        public string CharacterId { get; set; }
        
        [XmlIgnore]
        public FolderedVm Character
        {
            get
            {
                return Parent.Parent.Parent.Parent.CharactersTab.Items
                  .FirstOrDefault(item => item?.Id == CharacterId);
            }
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

        protected bool searchByName;
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

        protected bool affectAll;
        public bool AffectAll
        {
            get => affectAll;
            set
            {
                if (affectAll != value)
                {
                    affectAll = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            Character = null;
            SearchByName = false;
            AffectAll = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_CharacterBaseVm casted)
            {
                casted.CharacterId = CharacterId;
                casted.searchByName = searchByName;
                casted.affectAll = affectAll;
            }
        }
    }
}