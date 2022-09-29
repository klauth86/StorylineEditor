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
    [Description("Имеет вершину в текущем диалоге|реплике <>")]
    [XmlRoot]
    public class P_HasActiveDialogNodeMoreLessVm : P_BaseVm
    {
        public P_HasActiveDialogNodeMoreLessVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            num = 0;
            isMore = false;
            isMoreOrEqual = false;
            isEqual = false;
            isLessOrEqual = false;
            isLess = false;
        }

        public P_HasActiveDialogNodeMoreLessVm() : this(null, 0) { }

        public override bool IsValid=> base.IsValid && DialogNode != null && (isMore || isMoreOrEqual || isEqual || isLessOrEqual || isLess);

        public override bool IsOk => !IsValid ||
            !isInversed && NumericCondition(Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.FirstOrDefault((treePath) => treePath?.Tree?.Id == Parent.Parent.Id && treePath.IsActive)?.PassedNodes.Count((node) => node == DialogNode) ?? 0) ||
            isInversed && !NumericCondition(Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.FirstOrDefault((treePath) => treePath?.Tree?.Id == Parent.Parent.Id && treePath.IsActive)?.PassedNodes.Count((node) => node == DialogNode) ?? 0);

        private bool NumericCondition(int count)
        {
            if (IsMore) return count > Num;
            if (IsMoreOrEqual) return count >= Num;
            if (IsEqual) return count == Num;
            if (IsLessOrEqual) return count <= Num;
            if (IsLess) return count < Num;

            return false;
        }

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

        public int num;
        public int Num
        {
            get => num;
            set
            {
                if (num != value)
                {
                    num = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isMore;
        public bool IsMore
        {
            get => isMore;
            set
            {
                if (isMore != value)
                {
                    isMore = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isMoreOrEqual;
        public bool IsMoreOrEqual
        {
            get => isMoreOrEqual;
            set
            {
                if (isMoreOrEqual != value)
                {
                    isMoreOrEqual = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isEqual;
        public bool IsEqual
        {
            get => isEqual;
            set
            {
                if (isEqual != value)
                {
                    isEqual = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isLessOrEqual;
        public bool IsLessOrEqual
        {
            get => isLessOrEqual;
            set
            {
                if (isLessOrEqual != value)
                {
                    isLessOrEqual = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isLess;
        public bool IsLess
        {
            get => isLess;
            set
            {
                if (isLess != value)
                {
                    isLess = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            DialogNode = null;
            Num = 0;
            IsMore = false;
            IsMoreOrEqual = false;
            IsEqual = false;
            IsLessOrEqual = false;
            IsLess = false;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasActiveDialogNodeMoreLessVm casted)
            {
                casted.DialogNodeId = DialogNodeId;
                casted.num = num;
                casted.isMore = isMore;
                casted.isMoreOrEqual = isMoreOrEqual;
                casted.isEqual = isEqual;
                casted.isLessOrEqual = isLessOrEqual;
                casted.isLess = isLess;
            }
        }
    }
}