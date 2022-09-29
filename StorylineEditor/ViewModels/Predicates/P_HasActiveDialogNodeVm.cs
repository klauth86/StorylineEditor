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
    [Description("Имеет вершину в текущем диалоге|реплике")]
    [XmlRoot]
    public class P_HasActiveDialogNodeVm : P_BaseVm
    {
        public P_HasActiveDialogNodeVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            DialogNodeId = null;
        }

        public P_HasActiveDialogNodeVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && DialogNode != null;

        public override bool IsOk => !IsValid ||
            !isInversed && Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.Any((treePath) => treePath?.Tree?.Id == Parent.Parent.Id && treePath.PassedNodes.Contains(DialogNode) && treePath.IsActive) ||
            isInversed && !Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.Any((treePath) => treePath?.Tree?.Id == Parent.Parent.Id && treePath.PassedNodes.Contains(DialogNode) && treePath.IsActive);

        public string DialogNodeId { get; set; }
        
        [XmlIgnore]
        public Node_BaseVm DialogNode
        {
            get => Parent.Parent.Nodes.FirstOrDefault(item => item?.Id == DialogNodeId);
            set
            {
                if (DialogNodeId != value?.Id)
                {
                    DialogNodeId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            DialogNode = null;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasActiveDialogNodeVm casted)
            {
                casted.DialogNodeId = DialogNodeId;
            }
        }
    }
}