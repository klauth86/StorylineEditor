/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.GameEvents;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Predicates;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_InteractiveVM<T, U>
        : Node_BaseVM<T, U>
        where T : Node_InteractiveM
        where U : class
    {
        public Node_InteractiveVM(T model, U parent) : base(model, parent)
        {
            Predicates = new ObservableCollection<IPredicate>();
            foreach (var predicateModel in Model.predicates) Predicates.Add(PredicatesHelper.CreatePredicateByModel(predicateModel, this));

            GameEvents = new ObservableCollection<IGameEvent>();
            foreach (var gameEventModel in Model.gameEvents) GameEvents.Add(GameEventsHelper.CreateGameEventByModel(gameEventModel, this));
        }

        public Type SelectedPredicateType
        {
            get => null;
            set
            {
                if (value != null)
                {
                    IPredicate predicateViewModel = PredicatesHelper.CreatePredicateByType(value, this);
                    Model.predicates.Add(predicateViewModel.GetModel<P_BaseM>());
                    Predicates.Add(predicateViewModel);

                    OnModelChanged(Model, nameof(HasPredicates));
                }

                Notify(nameof(SelectedPredicateType));
            }
        }
        public ObservableCollection<IPredicate> Predicates { get; }
        public bool HasPredicates => Model.predicates.Count > 0;

        public Type SelectedGameEventType
        {
            set
            {
                if (value != null)
                {
                    IGameEvent gameEventViewModel = GameEventsHelper.CreateGameEventByType(value, this);
                    Model.gameEvents.Add(gameEventViewModel.GetModel<GE_BaseM>());
                    GameEvents.Add(gameEventViewModel);

                    OnModelChanged(Model, nameof(HasGameEvents));
                }

                Notify(nameof(SelectedGameEventType));
            }
        }
        public ObservableCollection<IGameEvent> GameEvents { get; }
        public bool HasGameEvents => Model.gameEvents.Count > 0;


        protected ICommand removeElementCommand;
        public ICommand RemoveElementCommand => removeElementCommand ?? (removeElementCommand = new RelayCommand<IWithModel>((viewModel) =>
        {
            if (viewModel is IPredicate predicateViewModel && Predicates.Remove(predicateViewModel))
            {
                Model.predicates.Remove(predicateViewModel.GetModel<P_BaseM>());
                OnModelChanged(Model, nameof(HasPredicates));
            }
            
            if (viewModel is IGameEvent gameEventViewModel && GameEvents.Remove(gameEventViewModel))
            {
                Model.gameEvents.Remove(gameEventViewModel.GetModel<GE_BaseM>());
                OnModelChanged(Model, nameof(HasGameEvents));
            }
        }));
    }

    public class Node_RandomVM : Node_InteractiveVM<Node_RandomM, object>
    {
        public Node_RandomVM(Node_RandomM model, object parent) : base(model, parent) { }
    }

    public class Node_RandomEditorVM : Node_RandomVM
    {
        public Node_RandomEditorVM(Node_RandomVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }

    public class Node_TransitVM : Node_InteractiveVM<Node_TransitM, object>
    {
        public Node_TransitVM(Node_TransitM model, object parent) : base(model, parent) { }
    }

    public class Node_TransitEditorVM : Node_TransitVM
    {
        public Node_TransitEditorVM(Node_TransitVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }

    public class Node_GateVM : Node_InteractiveVM<Node_GateM, object>
    {
        public Node_GateVM(Node_GateM model, object parent) : base(model, parent) { }

        public BaseM TargetDialog => ActiveContextService.GetDialog(Model.dialogId);

        public BaseM TargetExitNode => (TargetDialog as GraphM)?.nodes.FirstOrDefault((node) => node.id == Model.exitNodeId);
    }

    public class Node_GateEditorVM : Node_InteractiveVM<Node_GateM, object>
    {
        public CollectionViewSource DialogsCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public Node_GateEditorVM(Node_GateVM viewModel) : base(viewModel.Model, viewModel.Parent)
        {
            DialogsCVS = new CollectionViewSource() { Source = ActiveContextService.Dialogs };

            if (DialogsCVS.View != null)
            {
                DialogsCVS.View.Filter = OnDialogFilter;
                DialogsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                DialogsCVS.View.MoveCurrentTo(TargetDialog);
            }

            NodesCVS = new CollectionViewSource();

            RefreshNodesCVS();
        }

        private bool OnDialogFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(targetDialogFilter) || model.PassFilter(targetDialogFilter);
            }
            return false;
        }

        protected string targetDialogFilter;
        public string TargetDialogFilter
        {
            set
            {
                if (value != targetDialogFilter)
                {
                    targetDialogFilter = value;
                    DialogsCVS.View?.Refresh();
                }
            }
        }

        public BaseM TargetDialog
        {
            get => ActiveContextService.GetDialog(Model.dialogId);
            set
            {
                if (value?.id != Model.dialogId)
                {
                    Model.dialogId = value?.id;
                    OnModelChanged(Model, nameof(TargetDialog));
                    Notify(nameof(TargetDialog));

                    RefreshNodesCVS();

                    Name = TargetDialog?.name;
                }
            }
        }

        private bool OnNodesFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return (string.IsNullOrEmpty(nodesFilter) || model.PassFilter(nodesFilter)) && model is Node_ExitM;
            }
            return false;
        }

        protected string nodesFilter;
        public string NodesFilter
        {
            set
            {
                if (value != nodesFilter)
                {
                    nodesFilter = value;
                    NodesCVS.View?.Refresh();
                }
            }
        }

        public BaseM TargetExitNode
        {
            get => (TargetDialog as GraphM)?.nodes.FirstOrDefault((node) => node.id == Model.exitNodeId);
            set
            {
                if (value?.id != Model.exitNodeId)
                {
                    Model.exitNodeId = value?.id;
                    OnModelChanged(Model, nameof(TargetExitNode));
                    Notify(nameof(TargetExitNode));

                    Description = TargetExitNode?.name;
                }
            }
        }

        private void RefreshNodesCVS()
        {
            if (TargetDialog is GraphM graph)
            {
                NodesCVS.Source = graph.nodes;
                if (NodesCVS.View != null) NodesCVS.View.Filter = OnNodesFilter;
                NodesCVS.View?.MoveCurrentTo(TargetExitNode != null && graph.nodes.Contains(TargetExitNode) ? TargetExitNode : null);
            }
            else
            {
                NodesCVS.View?.MoveCurrentTo(null);
            }
        }
    }

    public class Node_ExitVM : Node_BaseVM<Node_ExitM, object>
    {
        public Node_ExitVM(Node_ExitM model, object parent) : base(model, parent) { }
    }

    public class Node_ExitEditorVM : Node_ExitVM
    {
        public Node_ExitEditorVM(Node_ExitVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }
}