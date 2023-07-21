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

using System;

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_JournalM : Node_InteractiveM
    {
        public Node_JournalM(long additionalTicks) : base(additionalTicks)
        {
            result = null;
        }

        public Node_JournalM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_JournalM casted)
            {
                casted.result = result;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                ((result?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                base.PassFilter(filter);
        }

        public string result { get; set; }
    }

    public class Node_StepM : Node_JournalM
    {
        public Node_StepM(long additionalTicks) : base(additionalTicks) { }

        public Node_StepM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_StepM clone = new Node_StepM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_AlternativeM : Node_JournalM
    {
        public Node_AlternativeM(long additionalTicks) : base(additionalTicks)
        {
            isHidden = false;
        }

        public Node_AlternativeM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_AlternativeM clone = new Node_AlternativeM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_AlternativeM casted)
            {
                casted.isHidden = isHidden;
            }
        }

        public bool isHidden { get; set; }
    }
}