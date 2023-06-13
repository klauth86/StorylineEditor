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

namespace StorylineEditor.Model.Predicates
{
    public class P_Relation_HasM : P_BaseM
    {
        public P_Relation_HasM(long additionalTicks) : base(additionalTicks)
        {
            npcId = null;
            compareType = COMPARE_TYPE.LESS;
            value = 0;
        }

        public P_Relation_HasM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            P_Relation_HasM clone = new P_Relation_HasM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_Relation_HasM casted)
            {
                casted.npcId = npcId;
                casted.compareType = compareType;
                casted.value = value;
            }
        }

        public string npcId { get; set; }
        public byte compareType { get; set; }
        public float value { get; set; }
    }
}