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

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StorylineEditor.ViewModel.Graphs
{
    public class QuestEditorVM : GraphEditorVM<QuestM, object>
    {
        public QuestEditorVM(
            QuestVM viewModel
            , object parent
            , IEnumerable<Type> nodeTypes
            , Func<Type, Point, BaseM> mCreator
            , Func<BaseM, Notifier> vmCreator
            , Func<Notifier, Notifier> evmCreator
            )
            : base(
                  viewModel.Model
                  , parent
                  , nodeTypes
                  , mCreator
                  , vmCreator
                  , evmCreator
                  )
        { }

        protected override string CanLinkNodes(INode from, INode to)
        {
            if (from == to) return nameof(ArgumentException);

            if (FromNodesLinks.ContainsKey(from.Id) && ToNodesLinks.ContainsKey(to.Id))
            {
                foreach (var linkId in FromNodesLinks[from.Id])
                {
                    if (ToNodesLinks[to.Id].Contains(linkId)) return nameof(ArgumentException);
                }
            }

            if (from is Node_Journal_AlternativeVM && to is Node_Journal_AlternativeVM) return nameof(NotImplementedException);

            foreach (var linkId in FromNodesLinks[from.Id])
            {
                if (LinksVMs.ContainsKey(linkId))
                {
                    if (LinksVMs[linkId].ToNodeId == to.Id) return nameof(NotImplementedException);
                }
            }

            return string.Empty;
        }

        protected override void PreLinkNodes(INode from, INode to)
        {
            if (from is Node_Journal_AlternativeVM) // Remove all if linking Alternative to smth (with respect to Gender)
            {
                foreach (var fromlinkId in FromNodesLinks[from.Id].ToList())
                {
                    if (LinksVMs.ContainsKey(fromlinkId))
                    {
                        if (NodesVMs.ContainsKey(LinksVMs[fromlinkId].ToNodeId))
                        {
                            if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_StepVM existingTo)
                            {
                                if (existingTo.Gender * to.Gender == 0 || existingTo.Gender == to.Gender) RemoveLink(fromlinkId);
                            }
                        }
                    }
                }
            }
            else if (to is Node_Journal_AlternativeVM) // Remove all Steps if linking Step to Alternative
            {
                foreach (var fromlinkId in FromNodesLinks[from.Id].ToList())
                {
                    if (LinksVMs.ContainsKey(fromlinkId))
                    {
                        if (NodesVMs.ContainsKey(LinksVMs[fromlinkId].ToNodeId))
                        {
                            if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_StepVM) RemoveLink(fromlinkId);
                        }
                    }
                }
            }
            else // Remove all if linking Step to Step (with respect to Gender)
            {
                foreach (var fromlinkId in FromNodesLinks[from.Id].ToList())
                {
                    if (LinksVMs.ContainsKey(fromlinkId))
                    {
                        if (NodesVMs.ContainsKey(LinksVMs[fromlinkId].ToNodeId))
                        {
                            if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_StepVM existingTo)
                            {
                                if (existingTo.Gender * to.Gender == 0 || existingTo.Gender == to.Gender) RemoveLink(fromlinkId);
                            }
                            else if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_AlternativeVM existingAlternativeTo)
                            {
                                RemoveLink(fromlinkId);
                            }
                        }
                    }
                }
            }
        }
    }
}