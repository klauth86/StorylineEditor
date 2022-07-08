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
    [Description("Начать диалог")]
    [XmlRoot]
    public class GE_StartDialogVm : GE_BaseVm
    {
        public GE_StartDialogVm(Node_BaseVm inParent) : base(inParent) {
            CharacterAId = null;
            CharacterBId = null;
        }

        public GE_StartDialogVm() : this(null) { }

        public override bool IsValid => base.IsValid && CharacterA != null && CharacterB != null;

        public string CharacterAId { get; set; }

        [XmlIgnore]
        public FolderedVm CharacterA
        {
            get
            {
                return Parent.Parent.Parent.Parent.CharactersTab.Items
                  .FirstOrDefault(item => item?.Id == CharacterAId);
            }
            set
            {
                if (CharacterAId != value?.Id)
                {
                    CharacterAId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public string CharacterBId { get; set; }
        
        [XmlIgnore]
        public FolderedVm CharacterB
        {
            get
            {
                return Parent.Parent.Parent.Parent.CharactersTab.Items
                  .FirstOrDefault(item => item?.Id == CharacterBId);
            }
            set
            {
                if (CharacterBId != value?.Id)
                {
                    CharacterBId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            CharacterA = null;
            CharacterB = null;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_StartDialogVm casted)
            {
                casted.CharacterAId = CharacterAId;
                casted.CharacterBId = CharacterBId;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_StartDialog>({0});", outerName, eventName) + Environment.NewLine;
            resultCode += string.Format("{0}->InterlocutorAClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, CharacterA?.ClassPathName ?? null) + Environment.NewLine;
            resultCode += string.Format("{0}->InterlocutorBClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, CharacterB?.ClassPathName ?? null) + Environment.NewLine;
            return resultCode;
        }
    }
}
