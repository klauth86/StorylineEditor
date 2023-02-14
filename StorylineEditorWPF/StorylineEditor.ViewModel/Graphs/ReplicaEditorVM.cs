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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Graphs
{
    public class ReplicaEditorVM
        : GraphEditorVM<ReplicaM, object>
    {
        public CollectionViewSource FilteredReplicaLocationCVS { get; }

        public ReplicaEditorVM(
            ReplicaVM viewModel
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
        {
            FilteredReplicaLocationCVS = new CollectionViewSource() { Source = ActiveContext.Locations };

            if (FilteredReplicaLocationCVS.View != null)
            {
                FilteredReplicaLocationCVS.View.Filter = OnLocationFilter;
                FilteredReplicaLocationCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredReplicaLocationCVS.View.MoveCurrentTo(ReplicaLocation);
            }
        }

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

            return string.Empty;
        }

        private bool OnLocationFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(replicaLocationFilter) || model.PassFilter(replicaLocationFilter);
            }
            return false;
        }

        protected string replicaLocationFilter;
        public string ReplicaLocationFilter
        {
            set
            {
                if (value != replicaLocationFilter)
                {
                    replicaLocationFilter = value;
                    FilteredReplicaLocationCVS.View?.Refresh();
                }
            }
        }

        public BaseM ReplicaLocation
        {
            get => ActiveContext.GetLocation(Model.locationId);
            set
            {
                if (value?.id != Model.locationId)
                {
                    Model.locationId = value?.id;
                    OnModelChanged(Model, nameof(ReplicaVM.ReplicaLocation));
                    Notify(nameof(ReplicaLocation));
                }
            }
        }
    }
}