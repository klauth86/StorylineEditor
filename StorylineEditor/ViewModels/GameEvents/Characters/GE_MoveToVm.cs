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
    [Description("Пойти к")]
    [XmlRoot]
    public class GE_MoveToVm : GE_CharacterBaseVm
    {
        public GE_MoveToVm(Node_BaseVm inParent) : base(inParent) {
            TargetId = null;
            searchTargetByName = false;
            acceptanceRadius = 0.01f;
            endInteraction = false;
        }

        public GE_MoveToVm() : this(null) { }

        public override bool IsValid => base.IsValid && Target != null && acceptanceRadius >= 0;

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

        protected float acceptanceRadius;
        public float AcceptanceRadius {
            get => acceptanceRadius;
            set {
                if (value != acceptanceRadius) {
                    acceptanceRadius = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool endInteraction;
        public bool EndInteraction
        {
            get => endInteraction;
            set
            {
                if (endInteraction != value)
                {
                    endInteraction = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            Target = null;
            SearchTargetByName = false;
            AcceptanceRadius = 0;
            EndInteraction = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_MoveToVm casted)
            {
                casted.TargetId = TargetId;
                casted.searchTargetByName = searchTargetByName;
                casted.acceptanceRadius = acceptanceRadius;
                casted.endInteraction = endInteraction;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_MoveTo>({0});", outerName, eventName) + Environment.NewLine;
            if (searchByName) resultCode += string.Format("{0}->CharacterName = \"{1}\";", eventName, Character?.ActorName ?? "") + Environment.NewLine;
            if (!searchByName) resultCode += string.Format("{0}->CharacterClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Character?.ClassPathName ?? null) + Environment.NewLine;
            if (searchByName) resultCode += string.Format("{0}->SearchByName = true;", eventName) + Environment.NewLine;
            if (affectAll) resultCode += string.Format("{0}->AffectAll = true;", eventName) + Environment.NewLine;
            if (searchTargetByName) resultCode += string.Format("{0}->TargetActorName = \"{1}\";", eventName, Target?.ActorName ?? "") + Environment.NewLine;
            if (searchTargetByName) resultCode += string.Format("{0}->SearchTargetActorByName = true;", eventName) + Environment.NewLine;
            if (!searchTargetByName) resultCode += string.Format("{0}->TargetActorClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Target?.ClassPathName ?? null) + Environment.NewLine;
            resultCode += string.Format("{0}->AcceptanceRadius = {1};", eventName, acceptanceRadius);
            if (endInteraction) resultCode += string.Format("{0}->EndInteraction = true;", eventName) + Environment.NewLine;

            return resultCode;
        }
    }
}