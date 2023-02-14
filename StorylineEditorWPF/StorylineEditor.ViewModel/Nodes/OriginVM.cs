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
    public class OriginVM : IPositioned
    {
        public static OriginVM GetOrigin() { return _instance ?? (_instance = new OriginVM()); }
        public double PositionX { get => 0; set => throw new NotImplementedException(); }
        public double PositionY { get => 0; set => throw new NotImplementedException(); }
        public string Id => null;
        public string CharacterId => null;
        private OriginVM() { }

        private static OriginVM _instance;
    }
}