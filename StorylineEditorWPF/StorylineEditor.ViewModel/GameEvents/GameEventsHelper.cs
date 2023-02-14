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

using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.GameEvents
{
    public static class GameEventsHelper
    {
        public static IGameEvent CreateGameEventByType(Type type, INode node)
        {
            if (type == typeof(GE_Item_DropM)) return new GE_Item_DropVM(new GE_Item_DropM(0), node);
            if (type == typeof(GE_Item_PickUpM)) return new GE_Item_PickUpVM(new GE_Item_PickUpM(0), node);
            if (type == typeof(GE_Quest_AddM)) return new GE_Quest_AddVM(new GE_Quest_AddM(0), node);
            if (type == typeof(GE_Quest_Node_AddM)) return new GE_Quest_Node_AddVM(new GE_Quest_Node_AddM(0), node);
            if (type == typeof(GE_Quest_Node_PassM)) return new GE_Quest_Node_PassVM(new GE_Quest_Node_PassM(0), node);
            if (type == typeof(GE_Relation_ChangeM)) return new GE_Relation_ChangeVM(new GE_Relation_ChangeM(0), node);

            throw new ArgumentOutOfRangeException(nameof(type));
        }

        public static IGameEvent CreateGameEventByModel(GE_BaseM model, INode node)
        {
            if (model.GetType() == typeof(GE_Item_DropM)) return new GE_Item_DropVM((GE_Item_DropM)model, node);
            if (model.GetType() == typeof(GE_Item_PickUpM)) return new GE_Item_PickUpVM((GE_Item_PickUpM)model, node);
            if (model.GetType() == typeof(GE_Quest_AddM)) return new GE_Quest_AddVM((GE_Quest_AddM)model, node);
            if (model.GetType() == typeof(GE_Quest_Node_AddM)) return new GE_Quest_Node_AddVM((GE_Quest_Node_AddM)model, node);
            if (model.GetType() == typeof(GE_Quest_Node_PassM)) return new GE_Quest_Node_PassVM((GE_Quest_Node_PassM)model, node);
            if (model.GetType() == typeof(GE_Relation_ChangeM)) return new GE_Relation_ChangeVM((GE_Relation_ChangeM)model, node);

            throw new ArgumentOutOfRangeException(nameof(model));
        }
    }
}