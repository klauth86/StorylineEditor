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

namespace StorylineEditor.Model.GameEvents
{
    public abstract class GE_BaseM : BaseM
    {
        public GE_BaseM(long additionalTicks) : base(additionalTicks)
        {
            executionMode = EXECUTION_MODE.ON_ENTER;
        }

        public GE_BaseM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is GE_BaseM casted)
            {
                casted.executionMode = executionMode;
            }
        }

        public byte executionMode { get; set; }
    }

    public class GE_CustomM : GE_BaseM
    {
        public GE_CustomM(long additionalTicks) : base(additionalTicks)
        {
            customStringParam = null;
        }

        public GE_CustomM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            GE_CustomM clone = new GE_CustomM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is GE_CustomM casted)
            {
                casted.customStringParam = customStringParam;
            }
        }

        public string customStringParam { get; set; }
    }
}