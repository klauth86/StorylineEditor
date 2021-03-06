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
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Потерять предмет")]
    [XmlRoot]
    public class GE_DropItemVm : GE_ItemBaseVm
    {
        public GE_DropItemVm(Node_BaseVm inParent) : base(inParent) { }

        public GE_DropItemVm() : this(null) { }

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

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_DropItemVm casted)
            {
                casted.CharacterId = CharacterId;
                casted.searchCharacterByName = searchCharacterByName;
                casted.destroyAfterDrop = destroyAfterDrop;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_DropItem>({0});", outerName, eventName) + Environment.NewLine;
            if (searchByName) resultCode += string.Format("{0}->ItemName = \"{1}\";", eventName, Item?.ActorName ?? "") + Environment.NewLine;
            if (!searchByName) resultCode += string.Format("{0}->ItemClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Item?.ClassPathName ?? null) + Environment.NewLine;
            if (searchByName) resultCode += string.Format("{0}->SearchByName = true;", eventName) + Environment.NewLine;
            if (affectAll) resultCode += string.Format("{0}->AffectAll = true;", eventName) + Environment.NewLine;

            if (searchCharacterByName) resultCode += string.Format("{0}->CharacterName = \"{1}\";", eventName, Character?.ActorName ?? "") + Environment.NewLine;
            if (!searchCharacterByName) resultCode += string.Format("{0}->CharacterClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Character?.ClassPathName ?? null) + Environment.NewLine;
            if (searchCharacterByName) resultCode += string.Format("{0}->SearchByName = true;", eventName) + Environment.NewLine;
            
            if (destroyAfterDrop) resultCode += string.Format("{0}->DestroyAfterDrop = true;", eventName) + Environment.NewLine;

            return resultCode;
        }
    }
}