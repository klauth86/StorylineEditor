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
    [Description("Имеет диалог|реплику")]
    [XmlRoot]
    public class P_HasDialogVm : P_BaseVm
    {
        public P_HasDialogVm(Node_BaseVm inParent) : base(inParent) {
            DialogId = null;
        }

        public P_HasDialogVm() : this(null) { }

        public override bool IsValid => base.IsValid && Dialog != null;

        public string DialogId { get; set; }

        [XmlIgnore]
        public FolderedVm Dialog
        {
            get
            {
                return Parent.Parent.Parent.Parent.DialogsAndReplicas.FirstOrDefault(item => item?.Id == DialogId);
            }
            set
            {
                if (DialogId != value?.Id)
                {
                    DialogId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            Dialog = null;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is P_HasDialogVm casted)
            {
                casted.DialogId = DialogId;
            }
        }

        public override string GenerateCode(string outerName)
        {
            return string.Format("({1}dialogHistory.Dialogs.Contains(\"{0}\"))", DialogId, IsInversed ? "!" : "");
        }
    }
}
