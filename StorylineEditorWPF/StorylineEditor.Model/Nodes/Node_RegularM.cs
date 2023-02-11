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

using System;

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_RegularM : Node_InteractiveM
    {
        public Node_RegularM(long additionalTicks) : base(additionalTicks)
        {
            characterId = null;
            overrideName = null;
            fileStorageType = STORAGE_TYPE.GOOGLE_DRIVE;
            fileHttpRef = null;
            shortDescription = null;
        }

        public Node_RegularM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_RegularM casted)
            {
                casted.characterId = characterId;
                casted.overrideName = overrideName;
                casted.fileStorageType = casted.fileStorageType;
                casted.fileHttpRef = fileHttpRef;
                casted.shortDescription = shortDescription;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                ((overrideName?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                ((shortDescription?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                base.PassFilter(filter);
        }

        public string characterId { get; set; }
        public string overrideName { get; set; }
        public byte fileStorageType { get; set; }
        public string fileHttpRef { get; set; }
        public string shortDescription { get; set; }
    }

    public class Node_ReplicaM : Node_RegularM
    {
        public Node_ReplicaM(long additionalTicks) : base(additionalTicks) { }

        public Node_ReplicaM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_ReplicaM clone = new Node_ReplicaM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_DialogM : Node_RegularM
    {
        public Node_DialogM(long additionalTicks) : base(additionalTicks) { }

        public Node_DialogM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_DialogM clone = new Node_DialogM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}