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

using StorylineEditor.Service;
using System;
using System.Collections.Generic;

namespace StorylineEditor.ViewModel.Interface
{
    public enum ENodeUpdateFlags
    {
        X = 1,
        Y = 2,
        XY = 3,
    }

    public interface IGraph
    {
        string Id { get; }
        INode SelectionNode { get; }
        INode FindNode(string nodeId);
        INode GenerateNode(string nodeId);
        void MoveTo(IPositioned positioned, Action<CustomStatus> callbackAction, float playRate);
        void MoveTo(string positionedId, Action<CustomStatus> callbackAction, float playRate);
        List<List<IPositioned>> GetAllPaths(string nodeId);
        void SetPlayerContext(object oldPlayerContext, object newPlayerContext);
        void TickPlayer(double deltaTime);
        void OnNodeGenderChanged(INode node);
        void OnNodePositionChanged(INode node, ENodeUpdateFlags updateFlags);
        void OnNodeSizeChanged(INode node, ENodeUpdateFlags updateFlags);
    }
}