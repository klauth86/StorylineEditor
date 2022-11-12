﻿/*
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
    public class P_Quest_Node_AddedVM : P_BaseVM<P_Quest_Node_AddedM>
    {
        public CollectionViewSource QuestsCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public P_Quest_Node_AddedVM(P_Quest_Node_AddedM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            QuestsCVS = new CollectionViewSource() { Source = ActiveContextService.Quests };

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
            get => ActiveContextService.GetQuest(Model.questId);
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
        }
    }
}