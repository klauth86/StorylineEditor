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

namespace StorylineEditor.Model.Predicates
{
    public class P_Dialog_Node_Has_PrevSessions_CmpM : P_Dialog_Node_Has_PrevSessionsM
    {
        public P_Dialog_Node_Has_PrevSessions_CmpM(long additionalTicks) : base(additionalTicks)
        {
            compareType = COMPARE_TYPE.LESS;
            value = 1;
        }

        public P_Dialog_Node_Has_PrevSessions_CmpM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            P_Dialog_Node_Has_PrevSessions_CmpM clone = new P_Dialog_Node_Has_PrevSessions_CmpM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_Dialog_Node_Has_PrevSessions_CmpM casted)
            {
                casted.compareType = compareType;
                casted.value = value;
            }
        }

        public byte compareType { get; set; }
        public int value { get; set; }
    }
}