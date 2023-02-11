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
    public class GE_Relation_ChangeM : GE_BaseM
    {
        public GE_Relation_ChangeM(long additionalTicks) : base(additionalTicks)
        {
            npcId = null;
            value = 0;
        }

        public GE_Relation_ChangeM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            GE_Relation_ChangeM clone = new GE_Relation_ChangeM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is GE_Relation_ChangeM casted)
            {
                casted.npcId = npcId;
                casted.value = value;
            }
        }

        public string npcId { get; set; }
        public float value { get; set; }
    }
}