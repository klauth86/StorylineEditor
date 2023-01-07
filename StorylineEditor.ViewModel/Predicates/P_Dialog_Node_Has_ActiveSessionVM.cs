/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Predicates;
using System.Linq;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Dialog_Node_Has_ActiveSessionVM : P_BaseVM<P_Dialog_Node_Has_ActiveSessionM, object>
    {
        public CollectionViewSource NodesCVS { get; }

        public P_Dialog_Node_Has_ActiveSessionVM(P_Dialog_Node_Has_ActiveSessionM model, object parent) : base(model, parent)
        {
            NodesCVS = new CollectionViewSource();

            ////// TODO
            //GraphM graph = (CallbackContext as IWithModel)?.GetModel<GraphM>();
            
            //NodesCVS.Source = graph?.nodes;
            //if (NodesCVS.View != null) NodesCVS.View.Filter = OnNodesFilter;
            //NodesCVS.View?.MoveCurrentTo(Node != null && graph != null && graph.nodes.Contains(Node) ? Node : null);
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
            get => null;
                ////// TODO (CallbackContext as IWithModel)?.GetModel<GraphM>()?.nodes.FirstOrDefault((node) => node.id == Model.nodeId);
            set
            {
                if (value?.id != Model.nodeId)
                {
                    Model.nodeId = value?.id;
                    Notify(nameof(Node));
                }
            }
        }

        public override bool IsTrue()
        {
            if (Node != null)
            {
                DialogEntryVM dialogEntryVm = ActiveContext.History.DialogEntries.FirstOrDefault((deVm) => deVm.Id == ActiveContext.History.ActiveDialogEntryId);

                if (dialogEntryVm != null)
                {
                    int count = dialogEntryVm.Nodes.Count((node) => node.id == Node.id);

                    bool result = count > 0;

                    if (IsInversed) result = !result;
                    return result;
                }
            }

            return true;
        }
    }
}