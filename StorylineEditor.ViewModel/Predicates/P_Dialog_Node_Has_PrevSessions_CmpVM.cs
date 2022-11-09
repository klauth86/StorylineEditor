/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Dialog_Node_Has_PrevSessions_CmpVM : P_BaseVM<P_Dialog_Node_Has_PrevSessions_CmpM>
    {
        public CollectionViewSource DialogsAndReplicasCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public P_Dialog_Node_Has_PrevSessions_CmpVM(P_Dialog_Node_Has_PrevSessions_CmpM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            DialogsAndReplicasCVS = new CollectionViewSource() { Source = ActiveContextService.DialogsAndReplicas };

            if (DialogsAndReplicasCVS.View != null)
            {
                DialogsAndReplicasCVS.View.Filter = OnFilter;
                DialogsAndReplicasCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                DialogsAndReplicasCVS.View.MoveCurrentTo(DialogOrReplica);
            }

            NodesCVS = new CollectionViewSource();

            RefreshNodesCVS();
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(dialogsAndReplicasFilter) ||
                    (model.name?.Contains(dialogsAndReplicasFilter) ?? false) ||
                    (model.description?.Contains(dialogsAndReplicasFilter) ?? false);
            }
            return false;
        }

        protected string dialogsAndReplicasFilter;
        public string DialogsAndReplicasFilter
        {
            set
            {
                if (value != dialogsAndReplicasFilter)
                {
                    dialogsAndReplicasFilter = value;
                    DialogsAndReplicasCVS.View?.Refresh();
                }
            }
        }
        public BaseM DialogOrReplica
        {
            get => ActiveContextService.GetDialogOrReplica(Model.id);
            set
            {
                if (value?.id != Model.id)
                {
                    Model.id = value?.id;
                    Notify(nameof(DialogOrReplica));

                    RefreshNodesCVS();
                }
            }
        }

        private bool OnNodesFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(nodesFilter) ||
                    (model.name?.Contains(nodesFilter) ?? false) ||
                    (model.description?.Contains(nodesFilter) ?? false);
            }
            return false;
        }

        protected string nodesFilter;
        public string NodesFilter
        {
            set
            {
                if (value != nodesFilter)
                {
                    nodesFilter = value;
                    DialogsAndReplicasCVS.View?.Refresh();
                }
            }
        }
        public BaseM Node
        {
            get => (DialogOrReplica as GraphM)?.nodes.FirstOrDefault((node) => node.id == Model.nodeId);
            set
            {
                if (value?.id != Model.nodeId)
                {
                    Model.nodeId = value?.id;
                    Notify(nameof(Node));
                }
            }
        }

        private void RefreshNodesCVS()
        {
            if (DialogOrReplica is GraphM graph)
            {
                NodesCVS.Source = graph.nodes;
                if (NodesCVS.View != null) NodesCVS.View.Filter = OnNodesFilter;
                NodesCVS.View?.MoveCurrentTo(Node != null && graph.nodes.Contains(Node) ? Node : null);
            }
        }

        public byte CompareType
        {
            get => Model.compareType;
            set
            {
                if (value != CompareType)
                {
                    Model.compareType = value;
                    Notify(nameof(CompareType));
                }
            }
        }

        public int Value
        {
            get => Model.value;
            set
            {
                if (value != CompareType)
                {
                    Model.value = value;
                    Notify(nameof(Value));
                }
            }
        }
    }
}