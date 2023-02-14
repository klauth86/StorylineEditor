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

using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Nodes
{
    public class PositionVM : IPositioned
    {
        public PositionVM(double inPositionX, double inPositionY, string inId, string inCharacterId)
        {
            positionX = inPositionX;
            positionY = inPositionY;
            id = inId;
            characterId = inCharacterId;
        }

        protected readonly double positionX;
        public double PositionX { get => positionX; set => throw new NotImplementedException(); }

        protected readonly double positionY;
        public double PositionY { get => positionY; set => throw new NotImplementedException(); }

        protected readonly string id;
        public string Id { get => id; }

        protected readonly string characterId;
        public string CharacterId { get => characterId; }
    }
}