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
    [Description("Задать цель")]
    [XmlRoot]
    public class GE_SetTargetActorVm : GE_CharacterBaseVm
    {
        public GE_SetTargetActorVm(Node_BaseVm inParent) : base(inParent) {
            TargetId = null;
            searchTargetByName = false;
            interactionType = "EInteractionType::USE";
        }

        public GE_SetTargetActorVm() : this(null) { }

        public override bool IsValid => base.IsValid && Target != null;

        public string TargetId { get; set; }

        [XmlIgnore]
        public BaseVm Target
        {
            get
            {
                return Parent.Parent.Parent.Parent.AllActors
                  .FirstOrDefault(item => item?.Id == TargetId);
            }
            set
            {
                if (TargetId != value?.Id)
                {
                    TargetId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected bool searchTargetByName;
        public bool SearchTargetByName
        {
            get => searchTargetByName;
            set
            {
                if (searchTargetByName != value)
                {
                    searchTargetByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected string interactionType;
        public string InteractionType {
            get => interactionType;
            set {
                if (value != interactionType) {
                    interactionType = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            Target = null;
            SearchTargetByName = false;
            interactionType = "EInteractionType::USE";
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_SetTargetActorVm casted)
            {
                casted.TargetId = TargetId;
                casted.searchTargetByName = searchTargetByName;
                casted.interactionType = interactionType;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_SetTargetActor>({0});", outerName, eventName) + Environment.NewLine;
            if (searchByName) resultCode += string.Format("{0}->CharacterName = \"{1}\";", eventName, Character?.ActorName ?? "") + Environment.NewLine;
            if (!searchByName) resultCode += string.Format("{0}->CharacterClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Character?.ClassPathName ?? null) + Environment.NewLine;
            if (searchByName) resultCode += string.Format("{0}->SearchByName = true;", eventName) + Environment.NewLine;
            if (affectAll) resultCode += string.Format("{0}->AffectAll = true;", eventName) + Environment.NewLine;
            if (searchTargetByName) resultCode += string.Format("{0}->TargetActorName = \"{1}\";", eventName, Target?.ActorName ?? "") + Environment.NewLine;
            if (!searchTargetByName) resultCode += string.Format("{0}->TargetActorClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Target?.ClassPathName ?? null) + Environment.NewLine;
            if (searchTargetByName) resultCode += string.Format("{0}->SearchTargetActorByName = true;", eventName) + Environment.NewLine;
            resultCode += string.Format("{0}->InteractionType = {1};", eventName, interactionType) + Environment.NewLine;
            return resultCode;
        }
    }
}