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

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_BaseM : BaseM
    {
        public Node_BaseM(long additionalTicks) : base(additionalTicks)
        {
            gender = GENDER.UNSET;
            positionX = 0;
            positionY = 0;
            rtDescriptionVersion = 0;
            rtDescription = new TextRangeM(0);
        }

        public Node_BaseM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_BaseM casted)
            {
                casted.gender = gender;
                casted.positionX = positionX;
                casted.positionY = positionY;
                casted.rtDescription = rtDescription;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                rtDescription.PassFilter(filter) ||
                base.PassFilter(filter);
        }

        public byte gender { get; set; }
        public double positionX { get; set; }
        public double positionY { get; set; }
        public int rtDescriptionVersion { get; set; }
        public TextRangeM rtDescription { get; set; }
    }
}