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
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Quest_AddVM : GE_BaseVM<GE_Quest_AddM, object>
    {
        public CollectionViewSource QuestsCVS { get; }

        public GE_Quest_AddVM(GE_Quest_AddM model, object parent) : base(model, parent)
        {
            QuestsCVS = new CollectionViewSource() { Source = ActiveContext.Quests };

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
    }
}