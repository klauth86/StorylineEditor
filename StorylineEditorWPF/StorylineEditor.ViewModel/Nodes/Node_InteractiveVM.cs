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
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.GameEvents;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Predicates;
using System;
using System.Collections.Generic;
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
            _predicates = new ObservableCollection<IPredicate>();
            foreach (var predicateModel in Model.predicates) _predicates.Add(PredicatesHelper.CreatePredicateByModel(predicateModel, this));

            _gameEvents = new ObservableCollection<IGameEvent>();
            foreach (var gameEventModel in Model.gameEvents) _gameEvents.Add(GameEventsHelper.CreateGameEventByModel(gameEventModel, this));
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
                    _predicates.Add(predicateViewModel);

                    OnModelChanged(Model, nameof(HasPredicates));
                }

                Notify(nameof(SelectedPredicateType));
            }
        }
        protected ObservableCollection<IPredicate> _predicates;
        public override IEnumerable<IPredicate> Predicates { get => _predicates; }
        public bool HasPredicates => Model.predicates.Count > 0;

        public Type SelectedGameEventType
        {
            set
            {
                if (value != null)
                {
                    IGameEvent gameEventViewModel = GameEventsHelper.CreateGameEventByType(value, this);
                    Model.gameEvents.Add(gameEventViewModel.GetModel<GE_BaseM>());
                    _gameEvents.Add(gameEventViewModel);

                    OnModelChanged(Model, nameof(HasGameEvents));
                }

                Notify(nameof(SelectedGameEventType));
            }
        }
        protected ObservableCollection<IGameEvent> _gameEvents;
        public override IEnumerable<IGameEvent> GameEvents { get => _gameEvents; }
        public bool HasGameEvents => Model.gameEvents.Count > 0;

        protected override void RemoveElementInternal(IWithModel viewModel)
        {
            base.RemoveElementInternal(viewModel);

            if (viewModel is IPredicate predicateViewModel && _predicates.Remove(predicateViewModel))
            {
                Model.predicates.Remove(predicateViewModel.GetModel<P_BaseM>());
                OnModelChanged(Model, nameof(HasPredicates));
            }

            if (viewModel is IGameEvent gameEventViewModel && _gameEvents.Remove(gameEventViewModel))
            {
                Model.gameEvents.Remove(gameEventViewModel.GetModel<GE_BaseM>());
                OnModelChanged(Model, nameof(HasGameEvents));
            }
        }
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

        public BaseM TargetDialog => ActiveContext.GetDialog(Model.dialogId);

        public BaseM TargetExitNode => (TargetDialog as GraphM)?.nodes.FirstOrDefault((node) => node.id == Model.exitNodeId);
    }

    public class Node_GateEditorVM : Node_InteractiveVM<Node_GateM, object>
    {
        public CollectionViewSource DialogsCVS { get; }
        public CollectionViewSource NodesCVS { get; }

        public Node_GateEditorVM(Node_GateVM viewModel) : base(viewModel.Model, viewModel.Parent)
        {
            DialogsCVS = new CollectionViewSource() { Source = ActiveContext.Dialogs };

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
            get => ActiveContext.GetDialog(Model.dialogId);
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