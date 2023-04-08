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

namespace StorylineEditor.Model.Behaviors
{
    public class B_OptionalM : B_BaseM
    {
        public B_OptionalM(long additionalTicks) : base(additionalTicks) { }

        public B_OptionalM() : this(0)
        {
            skipChance = 0;
        }

        public override BaseM Clone(long additionalTicks)
        {
            B_OptionalM clone = new B_OptionalM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is B_OptionalM casted)
            {
                casted.skipChance = skipChance;
            }
        }

        public double skipChance { get; set; }
    }
}