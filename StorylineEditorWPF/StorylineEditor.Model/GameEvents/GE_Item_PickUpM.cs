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
    public class GE_Item_PickUpM : GE_BaseM
    {
        public GE_Item_PickUpM(long additionalTicks) : base(additionalTicks)
        {
            itemId = null;
        }

        public GE_Item_PickUpM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            GE_Item_PickUpM clone = new GE_Item_PickUpM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is GE_Item_PickUpM casted)
            {
                casted.itemId = itemId;
            }
        }

        public string itemId { get; set; }
    }
}