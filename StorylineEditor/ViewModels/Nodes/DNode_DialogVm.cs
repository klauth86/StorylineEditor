/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Views.Nodes;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public class DNode_DialogVm : DNode_CharacterVm
    {
        public DNode_DialogVm(TreeVm Parent) : base(Parent)
        {
            isNondialogNode = false;
        }

        public DNode_DialogVm() : this(null) { }

        public override bool IsValid
        {
            get
            {
                return base.IsValid && (!IsNondialogNode ||
                    Parent.NodesTraversal(this, false).ToList().TrueForAll(childNode => !(childNode is IOwnered ownered) || ownered.Owner.Id != CharacterVm.PlayerId));
            }
        }

        protected bool isNondialogNode;
        public bool IsNondialogNode
        {
            get
            {
                return isNondialogNode;
            }
            set
            {
                if (value != isNondialogNode)
                {
                    isNondialogNode = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is DNode_DialogVm casted)
            {
                casted.isNondialogNode = isNondialogNode;
            }
        }
    }
}
