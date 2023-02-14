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
    public class ActorM : BaseM
    {
        public ActorM(long additionalTicks) : base(additionalTicks)
        {
            rtDescriptionVersion = 0;
            rtDescription = new TextRangeM(0);
            hasDescriptionFemale = false;
            descriptionFemale = null;
            rtDescriptionFemaleVersion = 0;
            rtDescriptionFemale = new TextRangeM(0);
        }

        public ActorM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        { 
            ActorM clone = new ActorM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is ActorM casted)
            {
                casted.rtDescription = rtDescription;
                casted.hasDescriptionFemale = hasDescriptionFemale;
                casted.descriptionFemale = descriptionFemale;
                casted.rtDescriptionFemale = rtDescriptionFemale;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                rtDescription.PassFilter(filter) ||
                hasDescriptionFemale && rtDescriptionFemale.PassFilter(filter) ||
                ((descriptionFemale?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                base.PassFilter(filter);
        }

        public int rtDescriptionVersion { get; set; }
        public TextRangeM rtDescription { get; set; }
        public bool hasDescriptionFemale { get; set; }
        public string descriptionFemale { get; set; }
        public int rtDescriptionFemaleVersion { get; set; }
        public TextRangeM rtDescriptionFemale { get; set; }
    }
}