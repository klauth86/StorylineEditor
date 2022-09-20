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
    [Description("Имеет вершину в диалоге|реплике")]
    [XmlRoot]
    public class P_HasDialogNodeVm : P_BaseVm
    {
        public P_HasDialogNodeVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            DialogId = null;
            DialogNodeId = null;
        }

        public P_HasDialogNodeVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && Dialog != null && DialogNode != null;

        public string DialogId { get; set; }

        [XmlIgnore]
        public FolderedVm Dialog
        {
            get => Parent.Parent.Parent.Parent.DialogsAndReplicas.FirstOrDefault(item => item?.Id == DialogId);
            set
            {
                if (DialogId != value?.Id)
                {
                    DialogId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();

                    actualNodesSource = null;
                    Notify(nameof(ActualNodes));
                    DialogNode = null;
                }
            }
        }

        protected CollectionViewSource actualTreesSource;
        [XmlIgnore]
        public ICollectionView ActualTrees
        {
            get
            {
                if (actualTreesSource == null)
                {
                    actualTreesSource = new CollectionViewSource() { Source = Parent.Parent.Parent.Parent.DialogsAndReplicas };

                    if (actualTreesSource.View != null)
                    {
                        actualTreesSource.View.MoveCurrentTo(null);
                    }
                }

                if (actualTreesSource.View != null)
                {
                    actualTreesSource.View.Filter = (object obj) => string.IsNullOrEmpty(treeFilter) || obj != null && ((BaseVm)obj).PassFilter(treeFilter);
                }

                return actualTreesSource.View;
            }
        }

        protected string treeFilter;
        [XmlIgnore]
        public string TreeFilter
        {
            get => treeFilter;
            set
            {
                if (value != treeFilter)
                {
                    treeFilter = value;
                    ActualTrees?.Refresh();
                }
            }
        }

        public string DialogNodeId { get; set; }
        
        [XmlIgnore]
        public Node_BaseVm DialogNode
        {
            get => (Dialog as TreeVm)?.Nodes.FirstOrDefault(item => item?.Id == DialogNodeId);
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

        protected CollectionViewSource actualNodesSource;
        [XmlIgnore]
        public ICollectionView ActualNodes
        {
            get
            {
                if (actualNodesSource == null)
                {
                    actualNodesSource = new CollectionViewSource() { Source = (Dialog as TreeVm)?.Nodes };
                    
                    if (actualNodesSource.View != null)
                    {
                        actualNodesSource.View.MoveCurrentTo(null);
                    }
                }
                
                if (actualNodesSource.View != null)
                {
                    actualNodesSource.View.Filter = (object obj) => Parent.Parent.NodeFilter(obj) && (string.IsNullOrEmpty(filter) || ((BaseVm)obj).PassFilter(filter));
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
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            var casted = destObj as P_HasDialogNodeVm;
            if (casted != null)
            {
                casted.DialogId = DialogId;
                casted.DialogNodeId = DialogNodeId;
            }
        }
    }
}
