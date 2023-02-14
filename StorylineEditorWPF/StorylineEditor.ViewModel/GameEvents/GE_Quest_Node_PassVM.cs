/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Graphs;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Quest_Node_PassVM : GE_BaseVM<GE_Quest_Node_PassM, object>
    {
        public CollectionViewSource QuestsCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public GE_Quest_Node_PassVM(GE_Quest_Node_PassM model, object parent) : base(model, parent)
        {
            QuestsCVS = new CollectionViewSource() { Source = ActiveContext.Quests };

            if (QuestsCVS.View != null)
            {
                QuestsCVS.View.Filter = OnFilter;
                QuestsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                QuestsCVS.View.MoveCurrentTo(Quest);
            }

            NodesCVS = new CollectionViewSource();

            RefreshNodesCVS();
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(questsFilter) || model.PassFilter(questsFilter);
            }
            return false;
        }

        protected string questsFilter;
        public string QuestsFilter
        {
            set
            {
                if (value != questsFilter)
                {
                    questsFilter = value;
                    QuestsCVS.View?.Refresh();
                }
            }
        }
        public BaseM Quest
        {
            get => ActiveContext.GetQuest(Model.questId);
            set
            {
                if (value?.id != Model.questId)
                {
                    Model.questId = value?.id;
                    Notify(nameof(Quest));

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
            get => (Quest as GraphM)?.nodes.FirstOrDefault((node) => node.id == Model.nodeId);
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
            if (Quest is GraphM graph)
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

        public override void Execute()
        {
            if (Quest != null && Node != null)
            {
                QuestEntryVM questEntryVm = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == Quest.id);

                if (questEntryVm == null)
                {
                    ActiveContext.History.AddQuest(Quest);
                }

                questEntryVm = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == Quest.id);

                if (!questEntryVm.HasKnownNode(Node))
                {
                    questEntryVm.AddKnownNode(Node);
                }

                questEntryVm.SetKnownNodeIsPassed(Node, true);
            }
        }
    }
}