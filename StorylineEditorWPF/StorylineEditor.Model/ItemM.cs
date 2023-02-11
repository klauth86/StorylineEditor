﻿/*
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

namespace StorylineEditor.Model
{
    public class ItemM : ActorM
    {
        public ItemM(long additionalTicks) : base(additionalTicks)
        {
            hasInternalDescription = false;
            internalDescription = null;
            hasInternalDescriptionFemale = false;
            internalDescriptionFemale = null;
        }

        public ItemM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            ItemM clone = new ItemM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is ItemM casted)
            {
                casted.hasInternalDescription = hasInternalDescription;
                casted.internalDescription = internalDescription;
                casted.hasInternalDescriptionFemale = hasInternalDescriptionFemale;
                casted.internalDescriptionFemale = internalDescriptionFemale;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                ((internalDescription?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                ((internalDescriptionFemale?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                base.PassFilter(filter);
        }

        public bool hasInternalDescription { get; set; }
        public string internalDescription { get; set; }
        public bool hasInternalDescriptionFemale { get; set; }
        public string internalDescriptionFemale { get; set; }
    }
}