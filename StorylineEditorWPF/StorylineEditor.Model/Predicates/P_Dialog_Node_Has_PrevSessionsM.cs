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
    public class P_Dialog_Node_Has_PrevSessionsM : P_Dialog_HasM
    {
        public P_Dialog_Node_Has_PrevSessionsM(long additionalTicks) : base(additionalTicks)
        {
            nodeId = null;
        }

        public P_Dialog_Node_Has_PrevSessionsM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            P_Dialog_Node_Has_PrevSessionsM clone = new P_Dialog_Node_Has_PrevSessionsM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_Dialog_Node_Has_PrevSessionsM casted)
            {
                casted.nodeId = nodeId;
            }
        }

        public string nodeId { get; set; }
    }
}