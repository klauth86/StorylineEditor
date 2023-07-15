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
using System.Xml.Serialization;

namespace StorylineEditor.Model
{
    public abstract class BaseM
    {
        public BaseM(long additionalTicks)
        {
            createdAt = DateTime.UtcNow;
            id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", GetType().Name, createdAt, createdAt.Ticks, additionalTicks);
            name = null;
            description = null;
        }

        public BaseM() : this(0) { }

        public T CloneAs<T>(long additionalTicks) where T : BaseM { return (T)Clone(additionalTicks); }
        public abstract BaseM Clone(long additionalTicks);
        protected virtual void CloneInternal(BaseM targetObject)
        {
            targetObject.name = name;
            targetObject.description = description;
        }

        public virtual bool PassFilter(string filter)
        {
            return
                ((name?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                ((description?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
        }

        [XmlIgnore] ////// TODO Remove at 31.12.2023
        public DateTime createdAt { get; set; }

        [XmlElement("createdAt")]
        public DateTime createdAtf
        {
            get => createdAt.ToUniversalTime();
            set
            {
                createdAt = value.ToUniversalTime();
            }
        }

        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}