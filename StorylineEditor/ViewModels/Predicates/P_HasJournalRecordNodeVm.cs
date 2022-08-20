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
    [Description("Имеет шаг/альтернативу в квесте")]
    [XmlRoot]
    public class P_HasJournalRecordNodeVm : P_BaseVm
    {
        public P_HasJournalRecordNodeVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            JournalRecordId = null;
            JournalRecordNodeId = null;
        }

        public P_HasJournalRecordNodeVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && JournalRecord != null && JournalRecordNode != null;

        public string JournalRecordId { get; set; }

        [XmlIgnore]
        public FolderedVm JournalRecord
        {
            get => Parent.Parent.Parent.Parent.JournalRecords.FirstOrDefault(item => item?.Id == JournalRecordId);
            set
            {
                if (JournalRecordId != value?.Id)
                {
                    JournalRecordId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();

                    Notify(nameof(ActualNodes));
                    JournalRecordNode = null;
                }
            }
        }

        public string JournalRecordNodeId { get; set; }
        
        [XmlIgnore]
        public Node_BaseVm JournalRecordNode
        {
            get => (JournalRecord as TreeVm)?.Nodes.FirstOrDefault(item => item?.Id == JournalRecordNodeId);
            set
            {
                if (JournalRecordNodeId != value?.Id)
                {
                    JournalRecordNodeId = value?.Id;
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

                actualNodesSource.Source = (JournalRecord as TreeVm)?.Nodes;
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

            JournalRecord = null;
            JournalRecordNode = null;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasJournalRecordNodeVm casted)
            {
                casted.JournalRecordId = JournalRecordId;
                casted.JournalRecordNodeId = JournalRecordNodeId;
            }
        }
    }
}
