/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
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
    public class ReplicaVM
        : GraphVM<ReplicaM>
    {
        public ReplicaVM(
            ReplicaM model
            , object parent
            )
            : base(
                  model
                  , parent
                  ) { }

        public BaseM ReplicaLocation => ActiveContext.GetLocation(Model.locationId);
    }

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