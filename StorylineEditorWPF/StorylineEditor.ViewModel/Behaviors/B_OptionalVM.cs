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

using StorylineEditor.Model.Behaviors;

namespace StorylineEditor.ViewModel.Behaviors
{
    public class B_OptionalVM : B_BaseVM<B_OptionalM, object>
    {
        public B_OptionalVM(B_OptionalM model, object parent) : base(model, parent) { }

        public int SkipChance
        {
            get => Model.skipChance;
            set
            {
                if (Model.skipChance != value)
                {
                    Model.skipChance = value;
                    Notify(nameof(SkipChance));
                }
            }
        }
    }
}