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
using StorylineEditor.ViewModel.Nodes;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
    public class ReplicaVM : BaseVM<ReplicaM>
    {
        public BaseM ReplicaLocation => ActiveContextService.GetLocation(Model.locationId);

        public ReplicaVM(ReplicaM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        protected ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((viewModel) =>
        {
            CallbackContext?.Callback(this, nameof(ICallbackContext));
        }));

        public override string Stats => Graph_BaseVM<ReplicaM>.GetStats(Model);
    }

    public class ReplicaEditorVM : Graph_BaseVM<ReplicaM>
    {
        public CollectionViewSource FilteredReplicaLocationCVS { get; }

        public ReplicaEditorVM(ReplicaVM viewModel, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
           Func<Notifier, ICallbackContext, Notifier> editorCreator, Type defaultNodeType) : base(viewModel.Model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, defaultNodeType)
        {
            FilteredReplicaLocationCVS = new CollectionViewSource() { Source = ActiveContextService.Locations };

            if (FilteredReplicaLocationCVS.View != null)
            {
                FilteredReplicaLocationCVS.View.Filter = OnLocationFilter;
                FilteredReplicaLocationCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredReplicaLocationCVS.View.MoveCurrentTo(ReplicaLocation);
            }
        }

        protected override string CanLinkNodes(INode from, INode to) { if (from == to) return nameof(ArgumentException); return string.Empty; }

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
            get => ActiveContextService.GetLocation(Model.locationId);
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

        public override string Title => null;
        public override string Stats => null;
    }
}