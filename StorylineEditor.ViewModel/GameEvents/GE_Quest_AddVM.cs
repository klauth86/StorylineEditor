﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Common;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Quest_AddVM : GE_BaseVM<GE_Quest_AddM>
    {
        public CollectionViewSource QuestsCVS { get; }

        public GE_Quest_AddVM(GE_Quest_AddM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            QuestsCVS = new CollectionViewSource() { Source = ActiveContextService.Quests };

            if (QuestsCVS.View != null)
            {
                QuestsCVS.View.Filter = OnFilter;
                QuestsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
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
            get => ActiveContextService.GetQuest(Model.questId);
            set
            {
                if (value?.id != Model.questId)
                {
                    Model.questId = value?.id;
                    Notify(nameof(Quest));
                }
            }
        }
    }
}