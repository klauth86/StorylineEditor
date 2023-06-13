/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Dialog_Node_Has_PrevSessions_CmpVM : P_BaseVM<P_Dialog_Node_Has_PrevSessions_CmpM, object>
    {
        public CollectionViewSource DialogsAndReplicasCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public P_Dialog_Node_Has_PrevSessions_CmpVM(P_Dialog_Node_Has_PrevSessions_CmpM model, object parent) : base(model, parent)
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

        protected ICommand compareTypeCommand;
        public ICommand CompareTypeCommand => compareTypeCommand ?? (compareTypeCommand = new RelayCommand<byte>((compareType) => CompareType = compareType));

        public byte CompareType
        {
            get => Model.compareType;
            set
            {
                if (value != Model.compareType)
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
                if (value != Model.value)
                {
                    Model.value = value;
                    Notify(nameof(Value));
                }
            }
        }
    }
}