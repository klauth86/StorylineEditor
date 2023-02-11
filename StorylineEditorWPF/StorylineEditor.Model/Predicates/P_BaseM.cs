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
    public abstract class P_BaseM : BaseM
    {
        public P_BaseM(long additionalTicks) : base(additionalTicks)
        {
            isInversed = false;
        }

        public P_BaseM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_BaseM casted)
            {
                casted.isInversed = isInversed;
            }
        }

        public bool isInversed { get; set; }
    }
}