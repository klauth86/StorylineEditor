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
    public class GE_Quest_Node_PassM : GE_Quest_Node_AddM
    {
        public GE_Quest_Node_PassM(long additionalTicks) : base(additionalTicks) { }

        public GE_Quest_Node_PassM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            GE_Quest_Node_PassM clone = new GE_Quest_Node_PassM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}