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
    public class P_CompositeM : P_BaseM
    {
        public P_CompositeM(long additionalTicks) : base(additionalTicks)
        {
            compositionType = COMPOSITION_TYPE.AND;
            predicateA = null;
            predicateB = null;
        }

        public P_CompositeM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            P_CompositeM clone = new P_CompositeM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_CompositeM casted)
            {
                casted.compositionType = compositionType;
                casted.predicateA = predicateA.CloneAs<P_BaseM>(0);
                casted.predicateB = predicateB.CloneAs<P_BaseM>(1);
            }
        }

        public byte compositionType { get; set; }
        public P_BaseM predicateA { get; set; }
        public P_BaseM predicateB { get; set; }
    }
}