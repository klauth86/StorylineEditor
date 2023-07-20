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

using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Predicates;
using System.Collections.Generic;

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_InteractiveM : Node_BaseM
    {
        public Node_InteractiveM(long additionalTicks) : base(additionalTicks)
        {
            predicates = new List<P_BaseM>();
            gameEvents = new List<GE_BaseM>();
        }

        public Node_InteractiveM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_InteractiveM casted)
            {
                for (int i = 0; i < predicates.Count; i++)
                {
                    casted.predicates.Add(predicates[i].CloneAs<P_BaseM>(i + additionalTicks));
                }

                for (int i = 0; i < gameEvents.Count; i++)
                {
                    casted.gameEvents.Add(gameEvents[i].CloneAs<GE_BaseM>(i + additionalTicks));
                }
            }
        }

        public List<P_BaseM> predicates { get; set; }
        public List<GE_BaseM> gameEvents { get; set; }
    }

    public class Node_RandomM : Node_InteractiveM
    {
        public Node_RandomM(long additionalTicks) : base(additionalTicks) { }

        public Node_RandomM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_RandomM clone = new Node_RandomM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_TransitM : Node_InteractiveM
    {
        public Node_TransitM(long additionalTicks) : base(additionalTicks) { }

        public Node_TransitM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_TransitM clone = new Node_TransitM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_GateM : Node_InteractiveM
    {
        public Node_GateM(long additionalTicks) : base(additionalTicks) { }

        public Node_GateM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_GateM clone = new Node_GateM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_GateM casted)
            {
                casted.dialogId = dialogId;
                casted.exitNodeId = exitNodeId;
            }
        }

        public string dialogId { get; set; }
        public string exitNodeId { get; set; }
    }

    public class Node_ExitM : Node_BaseM
    {
        public Node_ExitM(long additionalTicks) : base(additionalTicks) { }

        public Node_ExitM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_ExitM clone = new Node_ExitM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}