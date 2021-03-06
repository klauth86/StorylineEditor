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
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Сюжетная цепочка")]
    [XmlRoot]
    public class GE_Storyline : GE_BaseVm
    {
        public GE_Storyline(Node_BaseVm inParent) : base(inParent) { }
        
        public GE_Storyline() : this(null) { }

        public override bool IsValid => base.IsValid && !string.IsNullOrEmpty(storyline);

        protected string storyline;
        public string Storyline
        {
            get => storyline;
            set
            {
                if (storyline != value)
                {
                    storyline = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            Storyline = null;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_Storyline casted)
            {
                casted.storyline = storyline;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_Storyline>({0});", outerName, eventName) + Environment.NewLine;
            resultCode += string.Format("{0}->Storyline = \"{1}\";", eventName, storyline) + Environment.NewLine;
            return resultCode;
        }
    }
}
