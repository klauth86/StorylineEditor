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

using StorylineEditor.Model.RichText;
using System;

namespace StorylineEditor.Model
{
    public class ItemM : ActorM
    {
        public ItemM(long additionalTicks) : base(additionalTicks)
        {
            hasInternalDescription = false;
            internalDescription = null;
            rtInternalDescriptionVersion = 0;
            rtInternalDescription = new TextRangeM(0);
            hasInternalDescriptionFemale = false;
            internalDescriptionFemale = null;
            rtInternalDescriptionFemaleVersion = 0;
            rtInternalDescriptionFemale = new TextRangeM(0);
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
                casted.rtInternalDescription = rtInternalDescription;
                casted.hasInternalDescriptionFemale = hasInternalDescriptionFemale;
                casted.internalDescriptionFemale = internalDescriptionFemale;
                casted.rtInternalDescriptionFemale = rtInternalDescriptionFemale;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                hasInternalDescription && rtInternalDescription.PassFilter(filter) ||
                hasInternalDescription && hasInternalDescriptionFemale && rtInternalDescriptionFemale.PassFilter(filter) ||
                ((internalDescription?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                ((internalDescriptionFemale?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                base.PassFilter(filter);
        }

        public bool hasInternalDescription { get; set; }
        public string internalDescription { get; set; }
        public int rtInternalDescriptionVersion { get; set; }
        public TextRangeM rtInternalDescription { get; set; }
        public bool hasInternalDescriptionFemale { get; set; }
        public string internalDescriptionFemale { get; set; }
        public int rtInternalDescriptionFemaleVersion { get; set; }
        public TextRangeM rtInternalDescriptionFemale { get; set; }
    }
}