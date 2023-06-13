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

using System.Collections.Generic;

namespace StorylineEditor.Model
{
    public class FolderM : BaseM
    {
        public FolderM(long additionalTicks) : base(additionalTicks)
        {
            content = new List<BaseM>();
        }

        public FolderM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            FolderM clone = new FolderM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is FolderM casted)
            {
                for (int i = 0; i < content.Count; i++)
                {
                    casted.content.Add(content[i].Clone(i));
                }
            }
        }

        public List<BaseM> content { get; set; }

        public override bool PassFilter(string filter)
        {
            for (int i = 0; i < content.Count; i++)
            {
                if (content[i].PassFilter(filter)) return true;
            }

            return base.PassFilter(filter);
        }
    }
}