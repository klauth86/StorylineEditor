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

using StorylineEditor.Model.Behaviors;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Behaviors
{
    public static class BehaviorHelper
    {
        public static IBehavior CreateBehaviorByType(Type type, object node)
        {
            if (type == typeof(B_OptionalM)) return new B_OptionalVM(new B_OptionalM(0), node);

            throw new ArgumentOutOfRangeException(nameof(type));
        }

        public static IBehavior CreateBehaviorByModel(B_BaseM model, object node)
        {
            if (model.GetType() == typeof(B_OptionalM)) return new B_OptionalVM((B_OptionalM)model, node);

            throw new ArgumentOutOfRangeException(nameof(model));
        }

        public static bool IsTrue(B_BaseM model)
        {
            if (model is B_OptionalM optionalBehaviorModel)
            {
                return RandomHelper.NextDouble() * 100 < optionalBehaviorModel.skipChance;
            }

            return false;
        }
    }
}