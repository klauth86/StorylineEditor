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

namespace StorylineEditor.Model.GameEvents
{
    public class GE_Item_DropM : GE_Item_PickUpM
    {
        public GE_Item_DropM(long additionalTicks) : base(additionalTicks) { }

        public GE_Item_DropM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            GE_Item_PickUpM clone = new GE_Item_PickUpM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}