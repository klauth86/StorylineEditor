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
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Dialog_Node_Has_PrevSessionsVM : P_BaseVM<P_Dialog_Node_Has_PrevSessionsM, object>
    {
        public CollectionViewSource DialogsAndReplicasCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public P_Dialog_Node_Has_PrevSessionsVM(P_Dialog_Node_Has_PrevSessionsM model, object parent) : base(model, parent)
        {
            DialogsAndReplicasCVS = new CollectionViewSource() { Source = ActiveContext.DialogsAndReplicas };

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
                return string.IsNullOrEmpty(dialogsAndReplicasFilter) || model.PassFilter(dialogsAndReplicasFilter);
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
            get => ActiveContext.GetDialogOrReplica(Model.dialogId);
            set
            {
                if (value?.id != Model.dialogId)
                {
                    Model.dialogId = value?.id;
                    Notify(nameof(DialogOrReplica));

                    RefreshNodesCVS();
                }
            }
        }

        private bool OnNodesFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(nodesFilter) || model.PassFilter(nodesFilter);
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
                    NodesCVS.View?.Refresh();
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
            else
            {
                NodesCVS.View?.MoveCurrentTo(null);
            }
        }

        public override bool IsTrue()
        {
            if (DialogOrReplica != null && Node != null)
            {
                var dialogEntryVms = ActiveContext.History.DialogEntries.Where((deVm) => deVm.Model.id == DialogOrReplica.id && deVm.Model.id != ActiveContext.History.ActiveDialogEntryId);

                int count = 0;

                foreach (var dialogEntryVm in dialogEntryVms)
                {
                    count += dialogEntryVm.Nodes.Count((node) => node.id == Node.id);
                }

                bool result = count > 0;

                if (IsInversed) result = !result;
                return result;
            }

            return true;
        }
    }
}