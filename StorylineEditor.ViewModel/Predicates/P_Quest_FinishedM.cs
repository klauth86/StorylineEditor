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
    public class P_Quest_FinishedVM : P_BaseVM<P_Quest_FinishedM, object>
    {
        public CollectionViewSource QuestsCVS { get; }

        public P_Quest_FinishedVM(P_Quest_FinishedM model, object parent) : base(model, parent)
        {
            QuestsCVS = new CollectionViewSource() { Source = ActiveContext.Quests };

            if (QuestsCVS.View != null)
            {
                QuestsCVS.View.Filter = OnFilter;
                QuestsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                QuestsCVS.View.MoveCurrentTo(Quest);
            }
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
                }
            }
        }

        public override bool IsTrue()
        {
            if (Quest != null) ////// TODO SIMPLIFY
            {
                GraphM graph = (GraphM)Quest;

                bool result = false;

                QuestEntryVM questEntryVm = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == Quest.id);
                
                if (questEntryVm != null)
                {
                    foreach (var passedNode in questEntryVm.PassedNodes)
                    {
                        bool isLeaf = graph.links.All((linkM) => linkM.fromNodeId != passedNode.id);
                        if (isLeaf)
                        {
                            result = true;
                            break;
                        }
                    }
                }

                if (IsInversed) result = !result;
                return result;
            }

            return true;
        }
    }
}