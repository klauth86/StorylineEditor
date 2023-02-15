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

using System.IO;

namespace StorylineEditor.Service
{
    public interface ISerializationService
    {
        // Universal

        void Serialize<T>(Stream stream, T obj);

        T Deserialize<T>(Stream stream);



        // Clipboard

        string Serialize<T>(T obj);

        T Deserialize<T>(string str);
    }
}