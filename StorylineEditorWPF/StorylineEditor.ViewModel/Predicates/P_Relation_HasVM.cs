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
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Relation_HasVM : P_BaseVM<P_Relation_HasM, object>
    {
        public CollectionViewSource CharactersCVS { get; }

        public P_Relation_HasVM(P_Relation_HasM model, object parent) : base(model, parent)
        {
            CharactersCVS = new CollectionViewSource() { Source = ActiveContext.Characters };

            if (CharactersCVS.View != null)
            {
                CharactersCVS.View.Filter = OnFilter;
                CharactersCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                CharactersCVS.View.MoveCurrentTo(Character);
            }
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return model.id != CharacterM.PLAYER_ID && (string.IsNullOrEmpty(charactersFilter) || model.PassFilter(charactersFilter));
            }
            return false;
        }

        protected string charactersFilter;
        public string CharactersFilter
        {
            set
            {
                if (value != charactersFilter)
                {
                    charactersFilter = value;
                    CharactersCVS.View?.Refresh();
                }
            }
        }

        public BaseM Character
        {
            get => ActiveContext.GetCharacter(Model.npcId);
            set
            {
                if (value?.id != Model.npcId)
                {
                    Model.npcId = value?.id;
                    Notify(nameof(Character));
                }
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

        public float Value
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

        public override bool IsTrue()
        {
            if (Character != null)
            {
                CharacterM character = (CharacterM)Character;

                float relation = ActiveContext.History.Gender == GENDER.MALE
                    ? character.initialRelation
                    : character.initialRelationFemale;
                
                CharacterEntryVM characterEntryVm = ActiveContext.History.CharacterEntries.FirstOrDefault((ceVm) => ceVm.Model.id == Character.id);
                if (characterEntryVm != null)
                {
                    relation += characterEntryVm.DeltaRelation;
                }

                bool result = false;

                switch (CompareType)
                {
                    case COMPARE_TYPE.LESS:
                        result = relation < Value;
                        break;
                    case COMPARE_TYPE.LESS_OR_EQUAL:
                        result = relation <= Value;
                        break;
                    case COMPARE_TYPE.EQUAL:
                        result = relation == Value;
                        break;
                    case COMPARE_TYPE.EQUAL_OR_GREATER:
                        result = relation >= Value;
                        break;
                    case COMPARE_TYPE.GREATER:
                        result = relation > Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(CompareType));
                }

                if (IsInversed) result = !result;
                return result;
            }

            return true;
        }
    }
}