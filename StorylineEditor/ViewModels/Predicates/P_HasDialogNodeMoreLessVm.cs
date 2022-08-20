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
using System.Windows.Data;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Имеет вершину в диалоге|реплике <>")]
    [XmlRoot]
    public class P_HasDialogNodeMoreLessVm : P_BaseVm
    {
        public P_HasDialogNodeMoreLessVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            DialogId = null;
            num = 0;
            isMore = false;
            isMoreOrEqual = false;
            isEqual = false;
            isLessOrEqual = false;
            isLess = false;
        }

        public P_HasDialogNodeMoreLessVm() : this(null, 0) { }

        public override bool IsValid=> base.IsValid && Dialog != null && DialogNode != null && (isMore || isMoreOrEqual || isEqual || isLessOrEqual || isLess);

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
                    
                    Notify(nameof(ActualNodes));
                    DialogNode = null;
                }
            }
        }

        public string DialogNodeId { get; set; }

        [XmlIgnore]
        public Node_BaseVm DialogNode
        {
            get
            {
                return (Dialog as TreeVm)?.Nodes
                  .FirstOrDefault(item => item?.Id == DialogNodeId);
            }
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

        protected CollectionViewSource actualNodesSource;
        [XmlIgnore]
        public ICollectionView ActualNodes
        {
            get
            {
                if (actualNodesSource == null)
                {
                    actualNodesSource = new CollectionViewSource();
                }

                actualNodesSource.Source = (Dialog as TreeVm)?.Nodes;
                if (actualNodesSource.View != null)
                {
                    actualNodesSource.View.Filter = (object obj) => Parent.Parent.NodeFilter(obj) && (string.IsNullOrEmpty(filter) || ((BaseVm)obj).PassFilter(filter));
                    actualNodesSource.View.MoveCurrentTo(null);
                }

                return actualNodesSource.View;
            }
        }

        protected string filter;
        [XmlIgnore]
        public string Filter
        {
            get => filter;
            set
            {
                if (value != filter)
                {
                    filter = value;
                    ActualNodes?.Refresh();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            Dialog = null;
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

            if (destObj is P_HasDialogNodeMoreLessVm casted)
            {
                casted.DialogId = DialogId;
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
