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

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Service;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
    public enum ELinkVMUpdate : byte
    {
        FromX = 1,
        FromY = 2,
        ToX = 4,
        ToY = 8,
        Scale = 16
    }

    public abstract class GraphEditorVM<T, U>
        : CollectionVM<T, U, Point>
        , ICopyPaste
        , IGraph
        where T : GraphM
        where U : class
    {
        public GraphEditorVM(
            T model
            , U parent
            , IEnumerable<Type> nodeTypes
            , Func<Type, Point, BaseM> mCreator
            , Func<BaseM, Notifier> vmCreator
            , Func<Notifier, Notifier> evmCreator
            )
            : base(
                  model
                  , parent
                  , mCreator
                  , vmCreator
                  , evmCreator
                  )
        {
            _nodeTypes = new HashSet<Type>(nodeTypes);

            offsetY = offsetX = 0;
            scale = 1;
            selectedNodeType = _nodeTypes.FirstOrDefault();

            targetNodeViewModel = null;

            previewLink = new PreviewLinkVM(new LinkM(), this);

            selectionBox = new SelectionBoxVM();

            playerIndicator = new PlayerIndicatorVM(2, 300, 600);

            UserAction = null;

            NodesVMs = new Dictionary<string, Notifier>();
            LinksVMs = new Dictionary<string, LinkVM>();

            FromNodesLinks = new Dictionary<string, HashSet<string>>();
            ToNodesLinks = new Dictionary<string, HashSet<string>>();

            RootNodeIds = new List<string>();

            Dictionary<string, Tuple<double, double>> nodesPositions = new Dictionary<string, Tuple<double, double>>();

            foreach (var nodeModel in Model.nodes)
            {
                if (!FromNodesLinks.ContainsKey(nodeModel.id)) FromNodesLinks.Add(nodeModel.id, new HashSet<string>());
                if (!ToNodesLinks.ContainsKey(nodeModel.id)) ToNodesLinks.Add(nodeModel.id, new HashSet<string>());
                if (!nodesPositions.ContainsKey(nodeModel.id)) nodesPositions.Add(nodeModel.id, new Tuple<double, double>(nodeModel.positionX, nodeModel.positionY));
            }

            foreach (var linkModel in Model.links)
            {
                AddLinkVM(linkModel,
                    linkModel.fromNodeId, nodesPositions[linkModel.fromNodeId].Item1, nodesPositions[linkModel.fromNodeId].Item2,
                    linkModel.toNodeId, nodesPositions[linkModel.toNodeId].Item1, nodesPositions[linkModel.toNodeId].Item2);
            }

            foreach (var nodeModel in Model.nodes)
            {
                if (ToNodesLinks[nodeModel.id].Count == 0)
                {
                    RootNodeIds.Add(nodeModel.id);
                }
            }

            RootNodeIndex = 0;

            selection = new HashSet<string>();
        }

        protected readonly HashSet<Type> _nodeTypes;

        void StartScrollingTask(IPositioned positioned, Action<CustomStatus> callbackAction, float playRate)
        {
            double localX = FromAbsoluteToLocalX(positioned.PositionX);
            double localY = FromAbsoluteToLocalY(positioned.PositionY);

            double distance = Math.Sqrt(
                (localX - ActiveContext.ViewWidth / 2) * (localX - ActiveContext.ViewWidth / 2) +
                (localY - ActiveContext.ViewHeight / 2) * (localY - ActiveContext.ViewHeight / 2));

            if (distance < 16) // Ignore offset of 2x2 pixels
            {
                callbackAction?.Invoke(CustomStatus.RanToCompletion);
            }
            else
            {
                double velocityMsec = playRate * ActiveContext.ViewWidth / 2
                    / 400 // Msec
                    / 100; // playRate is %

                double durationMsec = distance / velocityMsec;

                double startOffsetX = OffsetX;
                double startOffsetY = OffsetY;
                double targetOffsetX = positioned.PositionX - ActiveContext.ViewWidth / 2 / Scale;
                double targetOffsetY = positioned.PositionY - ActiveContext.ViewHeight / 2 / Scale;

                ActiveContext.TaskService.Start(
                    durationMsec,
                    (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec) =>
                    {
                        playerIndicator.Tick(inDeltaTimeMsec);

                        double alpha = (inTimeMsec - inStartTimeMsec) / inDurationMsec;

                        SetView(startOffsetX * (1 - alpha) + alpha * targetOffsetX, startOffsetY * (1 - alpha) + alpha * targetOffsetY);

                        return CustomStatus.Running;
                    },
                    (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec, customStatus) =>
                    {
                        if (customStatus == CustomStatus.RanToCompletion)
                        {
                            playerIndicator.Tick(inDeltaTimeMsec);

                            SetView(targetOffsetX, targetOffsetY);
                        }

                        return customStatus;
                    }
                    , callbackAction);
            }
        }

        void StartScalingTask()
        {
            if (Math.Abs(Scale - 1) > 0.01)
            {
                ActiveContext.TaskService.Start(
                256,
                (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec) =>
                {                    
                    double alpha = (inTimeMsec - inStartTimeMsec) / inDurationMsec;
                    
                    double newScale = Scale * (1 - alpha) + alpha;
                    
                    SetScale(ActiveContext.ViewWidth / 2, ActiveContext.ViewHeight / 2, newScale);
                    
                    return CustomStatus.Running;
                },
                (inStartTimeMsec, inDurationMsec, inTimeMsec, inDeltaTimeMsec, customStatus) =>
                {
                    if (customStatus == CustomStatus.RanToCompletion)
                    {
                        SetScale(ActiveContext.ViewWidth / 2, ActiveContext.ViewHeight / 2, 1);
                    }

                    return customStatus;
                }
                , null);
            }
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ?? (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type));

        protected ICommand prevRootNodeCommand;
        public ICommand PrevRootNodeCommand => prevRootNodeCommand ?? (prevRootNodeCommand = new RelayCommand(() =>
        {
            RootNodeIndex = (RootNodeIndex + 1 + RootNodeIds.Count) % RootNodeIds.Count;

            if (RootNodeIds.Count > 0 && RootNodeIndex >= 0 && RootNodeIndex < RootNodeIds.Count)
            {
                Node_BaseM rootNodeModel = Model.nodes.FirstOrDefault(nodeModel => nodeModel.id == RootNodeIds[RootNodeIndex]);
                if (rootNodeModel != null)
                {
                    string characterId = rootNodeModel is Node_RegularM regularNodeModel ? regularNodeModel.characterId : null;
                    StartScrollingTask(new PositionVM(rootNodeModel.positionX, rootNodeModel.positionY, rootNodeModel.id, characterId), null, 100);
                }
            }
        }, () => RootNodeIds.Count > 0));

        protected ICommand nextRootNodeCommand;
        public ICommand NextRootNodeCommand => nextRootNodeCommand ?? (nextRootNodeCommand = new RelayCommand(() =>
        {
            RootNodeIndex = (RootNodeIndex - 1 + RootNodeIds.Count) % RootNodeIds.Count;

            if (RootNodeIds.Count > 0 && RootNodeIndex >= 0 && RootNodeIndex < RootNodeIds.Count)
            {
                Node_BaseM rootNodeModel = Model.nodes.FirstOrDefault(nodeModel => nodeModel.id == RootNodeIds[RootNodeIndex]);
                if (rootNodeModel != null)
                {
                    string characterId = rootNodeModel is Node_RegularM regularNodeModel ? regularNodeModel.characterId : null;
                    StartScrollingTask(new PositionVM(rootNodeModel.positionX, rootNodeModel.positionY, rootNodeModel.id, characterId), null, 100);
                }
            }
        }, () => RootNodeIds.Count > 0));

        protected ICommand resetScaleCommand;
        public ICommand ResetScaleCommand => resetScaleCommand ?? (resetScaleCommand = new RelayCommand(() => StartScalingTask()));

        protected ICommand goToOriginCommand;
        public ICommand GoToOriginCommand => goToOriginCommand ?? (goToOriginCommand = new RelayCommand(() => StartScrollingTask(OriginVM.GetOrigin(), null, 100)));

        protected ICommand playCommand;
        public ICommand PlayCommand => playCommand ?? (playCommand = new RelayCommand(() => ActiveContext.DialogService?.ShowDialog(ActiveContext.History), () => HasSelection()));

        protected void AddLinkVM(LinkM model, string fromId, double fromX, double fromY, string toId, double toX, double toY)
        {
            LinkVM viewModel = (LinkVM)_vmCreator(model);

            viewModel.FromX = fromX;
            viewModel.FromY = fromY;
            viewModel.ToX = toX;
            viewModel.ToY = toY;

            LinksVMs.Add(model.id, viewModel);
            Add(null, null, viewModel);

            FromNodesLinks[fromId].Add(model.id);
            ToNodesLinks[toId].Add(model.id);

            UpdateLinkLocalPosition(LinksVMs[model.id], ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);
        }

        protected INode targetNodeViewModel;
        protected Point prevPosition;
        protected LinkVM previewLink;
        protected SelectionBoxVM selectionBox;
        protected PlayerIndicatorVM playerIndicator;
        protected UserActionM UserAction;

        protected ICommand moveCommand;
        public ICommand MoveCommand => moveCommand ?? (moveCommand = new RelayCommand<MouseEventArgs>((args) =>
        {
            if (UserAction == null) return;

            if (!HasActiveInput(UserAction))
            {
                FinishUserAction(args);
                return;
            }

            if (UserAction.UserActionType == USER_ACTION_TYPE.LINK)
            {
                if (targetNodeViewModel != null)
                {
                    if ((args.OriginalSource as FrameworkElement)?.DataContext is INode toNodeViewModel)
                    {
                        if (targetNodeViewModel == toNodeViewModel)
                        {
                            Point position = args.GetPosition(args.Source as UIElement);

                            previewLink.ToX = FromLocalToAbsoluteX(position.X);
                            previewLink.ToY = FromLocalToAbsoluteY(position.Y);
                            UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                            previewLink.Description = null;
                        }
                        else
                        {
                            previewLink.ToX = toNodeViewModel.PositionX;
                            previewLink.ToY = toNodeViewModel.PositionY;
                            UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                            previewLink.Description = CanLinkNodes(targetNodeViewModel, toNodeViewModel);
                        }
                    }
                    else
                    {
                        Point position = args.GetPosition(args.Source as UIElement);

                        previewLink.ToX = FromLocalToAbsoluteX(position.X);
                        previewLink.ToY = FromLocalToAbsoluteY(position.Y);
                        UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                        previewLink.Description = null;
                    }
                }
            }

            if (UserAction.UserActionType == USER_ACTION_TYPE.DUPLICATE_NODE)
            {
                Point position = args.GetPosition(null);

                double deltaX = (position.X - prevPosition.X) / Scale;
                double deltaY = (position.Y - prevPosition.Y) / Scale;

                if (targetNodeViewModel != null)
                {
                    if (!targetNodeViewModel.IsSelected)
                    {
                        targetNodeViewModel.PositionX += deltaX;
                        targetNodeViewModel.PositionY += deltaY;
                    }

                    HashSet<string> withoutVms = new HashSet<string>();

                    foreach (string selectedId in selection)
                    {
                        if (NodesVMs.ContainsKey(selectedId))
                        {
                            if (NodesVMs[selectedId] is INode nodeViewModel)
                            {
                                nodeViewModel.PositionX += deltaX;
                                nodeViewModel.PositionY += deltaY;
                            }
                        }
                        else
                        {
                            withoutVms.Add(selectedId);
                        }
                    }

                    foreach (var nodeModel in Model.nodes)
                    {
                        if (withoutVms.Contains(nodeModel.id))
                        {
                            nodeModel.positionX += deltaX;
                            nodeModel.positionY += deltaY;
                        }
                    }
                }

                prevPosition = position;
            }

            if (UserAction.UserActionType == USER_ACTION_TYPE.DRAG_AND_SCROLL)
            {
                Point position = args.GetPosition(null);

                double deltaX = (position.X - prevPosition.X) / Scale;
                double deltaY = (position.Y - prevPosition.Y) / Scale;

                if (targetNodeViewModel != null)
                {
                    if (!targetNodeViewModel.IsSelected)
                    {
                        targetNodeViewModel.PositionX += deltaX;
                        targetNodeViewModel.PositionY += deltaY;
                    }
                    else
                    {
                        HashSet<string> withoutVms = new HashSet<string>();

                        foreach (string selectedId in selection)
                        {
                            if (NodesVMs.ContainsKey(selectedId))
                            {
                                if (NodesVMs[selectedId] is INode nodeViewModel)
                                {
                                    nodeViewModel.PositionX += deltaX;
                                    nodeViewModel.PositionY += deltaY;
                                }
                            }
                            else
                            {
                                withoutVms.Add(selectedId);
                            }
                        }

                        foreach (var nodeModel in Model.nodes)
                        {
                            if (withoutVms.Contains(nodeModel.id))
                            {
                                nodeModel.positionX += deltaX;
                                nodeModel.positionY += deltaY;
                            }
                        }
                    }
                }
                else
                {
                    TranslateView(deltaX, deltaY);
                }

                prevPosition = position;
            }

            if (UserAction.UserActionType == USER_ACTION_TYPE.SELECTION_BOX)
            {
                Point position = args.GetPosition(null);

                double deltaX = (position.X - prevPosition.X) / Scale;
                double deltaY = (position.Y - prevPosition.Y) / Scale;

                selectionBox.ToX += deltaX;
                selectionBox.ToY += deltaY;

                UpdateSelectionBoxLocalPosition(selectionBox, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                prevPosition = position;
            }
        }));

        protected ICommand initCommand;
        public ICommand InitCommand => initCommand ?? (initCommand = new RelayCommand<RoutedEventArgs>((args) =>
        {
            TranslateView(0, 0);

            OnPostFilterChangedHandler(Filter);

            OnSecondFilterChangedPass += OnPostFilterChangedHandler;
        }, (args) => args != null));

        protected ICommand unInitCommand;
        public ICommand UnInitCommand => unInitCommand ?? (unInitCommand = new RelayCommand<RoutedEventArgs>((args) =>
        {
            OnSecondFilterChangedPass -= OnPostFilterChangedHandler;
        }));

        protected ICommand scaleCommand;
        public ICommand ScaleCommand => scaleCommand ?? (scaleCommand = new RelayCommand<MouseWheelEventArgs>((args) =>
        {
            if (UserAction != null) return;

            if (args.Source is UIElement uiElement)
            {
                Point position = args.GetPosition(uiElement);
                SetScale(position.X, position.Y, Math.Max(Math.Min(Scale + args.Delta * 0.0002, 4), 1.0 / 64));
            }
        }));

        protected ICommand facadeMouseCommand;
        public ICommand FacadeMouseCommand => facadeMouseCommand ?? (facadeMouseCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (args.ButtonState == MouseButtonState.Pressed && UserAction == null)
            {
                List<UserActionM> possibleUserActions = ConfigM.Config.UserActions
                .Where((userAction) => IsMatching(args, userAction))
                .Where((userAction) => CanExec(args, userAction.UserActionType)).ToList();

                if (possibleUserActions.Count > 0)
                {
                    if (possibleUserActions.Count > 1)
                    {
                        ////// TODO
                    }
                    else if (possibleUserActions[0] != null)
                    {
                        switch (possibleUserActions[0].UserActionType)
                        {
                            case USER_ACTION_TYPE.CREATE_NODE: CreateNodeImpl(args); break;
                            case USER_ACTION_TYPE.DUPLICATE_NODE: DuplicateNodeImpl(args); break;
                            case USER_ACTION_TYPE.LINK: StartLinkImpl(args); break;
                            case USER_ACTION_TYPE.DRAG_AND_SCROLL: StartDragImpl(args); break;
                            case USER_ACTION_TYPE.SELECTION_SIMPLE: SelectionSimple(args); break;
                            case USER_ACTION_TYPE.SELECTION_ADDITIVE: SelectionAdditive(args); break;
                            case USER_ACTION_TYPE.SELECTION_BOX: StartSelectionBox(args); break;
                        }
                    }
                }
            }
            else if (args.ButtonState == MouseButtonState.Released && UserAction != null)
            {
                if (args.ChangedButton == UserAction.MouseButton) FinishUserAction(args);
            }
        }));

        protected ICommand facadeKeyboardCommand;
        public ICommand FacadeKeyboardCommand => facadeKeyboardCommand ?? (facadeKeyboardCommand = new RelayCommand<KeyEventArgs>((args) =>
        {
            List<UserActionM> possibleUserActions = ConfigM.Config.UserActions
            .Where((userAction) => IsMatching(args, userAction))
            .Where((userAction) => CanExec(args, userAction.UserActionType)).ToList();

            if (possibleUserActions.Count > 0)
            {
                if (possibleUserActions.Count > 1)
                {
                    ////// TODO
                }
                else if (possibleUserActions[0] != null)
                {
                    switch (possibleUserActions[0].UserActionType)
                    {
                        case USER_ACTION_TYPE.ALIGN_HOR: AlignHorImpl(args); break;
                        case USER_ACTION_TYPE.ALIGN_VER: AlignVerImpl(args); break;
                    }
                }
            }
        }));

        private void AlignHorImpl(KeyEventArgs args)
        {
            List<INode> nodesToAlign = new List<INode>();
            double centeredY = 0;

            foreach (string selectedId in selection)
            {
                if (NodesVMs.ContainsKey(selectedId))
                {
                    if (NodesVMs[selectedId] is INode nodeViewModel)
                    {
                        nodesToAlign.Add(nodeViewModel);
                        centeredY += nodeViewModel.PositionY;
                    }
                }
            }

            foreach (var node in nodesToAlign)
            {
                node.PositionY = centeredY / nodesToAlign.Count;
            }
        }

        private void AlignVerImpl(KeyEventArgs args)
        {
            List<INode> nodesToAlign = new List<INode>();
            double centeredX = 0;

            foreach (string selectedId in selection)
            {
                if (NodesVMs.ContainsKey(selectedId))
                {
                    if (NodesVMs[selectedId] is INode nodeViewModel)
                    {
                        nodesToAlign.Add(nodeViewModel);
                        centeredX += nodeViewModel.PositionX;
                    }
                }
            }

            foreach (var node in nodesToAlign)
            {
                node.PositionX = centeredX / nodesToAlign.Count;
            }
        }

        private void CreateNodeImpl(MouseButtonEventArgs args)
        {
            Point position = args.GetPosition((IInputElement)args.Source);

            position.X = FromLocalToAbsoluteX(position.X);
            position.Y = FromLocalToAbsoluteY(position.Y);

            BaseM model = _mCreator(SelectedNodeType, position);

            AddNode(model, true);
        }

        private void DuplicateNodeImpl(MouseButtonEventArgs args)
        {
            IWithModel sourceObject = (IWithModel)(args.OriginalSource as FrameworkElement).DataContext;
            BaseM sourceModel = sourceObject.GetModel<BaseM>();

            BaseM model = sourceModel.Clone(0);

            AddNode(model, true);

            prevPosition = args.GetPosition(null);

            targetNodeViewModel = selectionNode;

            UserAction = ConfigM.Config.UserActions.First((userAction) => userAction.UserActionType == USER_ACTION_TYPE.DUPLICATE_NODE);

            CommandManager.InvalidateRequerySuggested();
        }

        private void StartLinkImpl(MouseButtonEventArgs args)
        {
            targetNodeViewModel = (INode)(args.OriginalSource as FrameworkElement).DataContext;

            ShowPreviewLink(targetNodeViewModel);

            UserAction = ConfigM.Config.UserActions.First((userAction) => userAction.UserActionType == USER_ACTION_TYPE.LINK);
        }

        private void StartDragImpl(MouseButtonEventArgs args)
        {
            prevPosition = args.GetPosition(null);

            targetNodeViewModel = (args.OriginalSource as FrameworkElement)?.DataContext as INode;

            UserAction = ConfigM.Config.UserActions.First((userAction) => userAction.UserActionType == USER_ACTION_TYPE.DRAG_AND_SCROLL);

            CommandManager.InvalidateRequerySuggested();
        }

        private void SelectionSimple(MouseButtonEventArgs args)
        {
            AddToSelection((Notifier)(args.OriginalSource as FrameworkElement).DataContext, true);
        }

        private void SelectionAdditive(MouseButtonEventArgs args)
        {
            AddToSelection((Notifier)(args.OriginalSource as FrameworkElement).DataContext, false);
        }

        private void StartSelectionBox(MouseButtonEventArgs args)
        {
            prevPosition = args.GetPosition(null);

            Point position = args.GetPosition((IInputElement)args.Source);

            double positionX = FromLocalToAbsoluteX(position.X);
            double positionY = FromLocalToAbsoluteY(position.Y);

            ShowSelectionBox(positionX, positionY);

            UserAction = ConfigM.Config.UserActions.First((userAction) => userAction.UserActionType == USER_ACTION_TYPE.SELECTION_BOX);
        }

        private void FinishUserAction(MouseEventArgs args)
        {
            switch (UserAction.UserActionType)
            {
                case USER_ACTION_TYPE.DUPLICATE_NODE:
                    {
                        targetNodeViewModel = null;
                        UserAction = null;
                    }
                    break;
                case USER_ACTION_TYPE.LINK:
                    {
                        if (targetNodeViewModel != null)
                        {
                            if ((args.OriginalSource as FrameworkElement)?.DataContext is INode toNodeViewModel)
                            {
                                string message = CanLinkNodes(targetNodeViewModel, toNodeViewModel);
                                if (message == string.Empty)
                                {
                                    PreLinkNodes(targetNodeViewModel, toNodeViewModel);

                                    LinkM model = (LinkM)_mCreator(typeof(LinkM), new Point());
                                    model.fromNodeId = targetNodeViewModel.Id;
                                    model.toNodeId = toNodeViewModel.Id;

                                    Add(GetContext(model), model, null);

                                    AddLinkVM(model,
                                        targetNodeViewModel.Id, targetNodeViewModel.PositionX, targetNodeViewModel.PositionY,
                                        toNodeViewModel.Id, toNodeViewModel.PositionX, toNodeViewModel.PositionY);

                                    RootNodeIds.Remove(toNodeViewModel.Id);

                                    CommandManager.InvalidateRequerySuggested();
                                }
                            }
                        }

                        HidePreviewLink();
                        targetNodeViewModel = null;
                        UserAction = null;
                    }
                    break;
                case USER_ACTION_TYPE.DRAG_AND_SCROLL:
                    {
                        targetNodeViewModel = null;
                        UserAction = null;
                    }
                    break;
                case USER_ACTION_TYPE.SELECTION_BOX:
                    {
                        ProcessSelectionBox();
                        HideSelectionBox();
                        UserAction = null;
                    }
                    break;
            }
        }

        private bool IsMatching(MouseButtonEventArgs args, UserActionM userAction) { return args.ChangedButton == userAction.MouseButton && Keyboard.Modifiers == userAction.ModifierKeys; }

        private bool IsMatching(KeyEventArgs args, UserActionM userAction) { return args.Key == userAction.KeyboardButton && Keyboard.GetKeyStates(args.Key) == KeyStates.Down; }

        private bool CanExec(MouseEventArgs args, byte userActiontype)
        {
            if (userActiontype == USER_ACTION_TYPE.CREATE_NODE) return !((args.OriginalSource as FrameworkElement)?.DataContext is INode) && args.Source is IInputElement && SelectedNodeType != null;

            if (userActiontype == USER_ACTION_TYPE.DUPLICATE_NODE) return (args.OriginalSource as FrameworkElement)?.DataContext is INode;

            if (userActiontype == USER_ACTION_TYPE.LINK) return (args.OriginalSource as FrameworkElement)?.DataContext is INode;

            if (userActiontype == USER_ACTION_TYPE.DRAG_AND_SCROLL) return true;

            if (userActiontype == USER_ACTION_TYPE.SELECTION_SIMPLE) return (args.OriginalSource as FrameworkElement)?.DataContext is INode;

            if (userActiontype == USER_ACTION_TYPE.SELECTION_ADDITIVE) return (args.OriginalSource as FrameworkElement)?.DataContext is INode;

            if (userActiontype == USER_ACTION_TYPE.SELECTION_BOX) return args.Source is IInputElement;

            return false;
        }

        private bool CanExec(KeyEventArgs args, byte userActiontype) { return selection.Count > 1; }

        private bool HasActiveInput(UserActionM userAction)
        {
            if (userAction.MouseButton == MouseButton.Left) return Mouse.LeftButton == MouseButtonState.Pressed;
            if (userAction.MouseButton == MouseButton.Right) return Mouse.RightButton == MouseButtonState.Pressed;

            return false;
        }

        public SelectionType GetSelection<SelectionType>() where SelectionType : class { return SelectionNode as SelectionType; }

        public string Copy()
        {
            GraphM graphModel = new QuestM();

            // nodes

            foreach (string nodeId in selection)
            {
                if (NodesVMs.ContainsKey(nodeId))
                {
                    Node_BaseM nodeModel = (NodesVMs[nodeId] as IWithModel)?.GetModel<Node_BaseM>();

                    if (nodeModel != null)
                    {
                        graphModel.nodes.Add(nodeModel);
                    }
                }
            }

            // links

            foreach (string nodeId in selection)
            {
                foreach (var linkId in FromNodesLinks[nodeId])
                {
                    if (LinksVMs.ContainsKey(linkId))
                    {
                        if (selection.Contains(LinksVMs[linkId].ToNodeId))
                        {
                            graphModel.links.Add(LinksVMs[linkId].Model);
                        }
                    }
                }
            }

            GraphM graphModelCopy = graphModel.CloneAs<GraphM>(0);

            foreach (var nodeModel in graphModelCopy.nodes)
            {
                nodeModel.positionX -= OffsetX;
                nodeModel.positionY -= OffsetY;
            }

            return ActiveContext.SerializationService.Serialize(graphModelCopy);
        }

        public void Paste(string clipboard)
        {
            GraphM graphModel = ActiveContext.SerializationService.Deserialize<GraphM>(clipboard);

            if (graphModel != null)
            {
                PrePaste(graphModel);

                GraphM graphModelCopy = graphModel.CloneAs<GraphM>(0);

                bool resetSelection = true;

                foreach (var nodeModel in graphModelCopy.nodes)
                {
                    nodeModel.positionX += OffsetX;
                    nodeModel.positionY += OffsetY;
                    AddNode(nodeModel, resetSelection);
                    resetSelection = false;
                }

                foreach (var linkModel in graphModelCopy.links)
                {
                    Add(GetContext(linkModel), linkModel, null);

                    INode fromNodeViewModel = NodesVMs[linkModel.fromNodeId] as INode;
                    INode toNodeViewModel = NodesVMs[linkModel.toNodeId] as INode;

                    AddLinkVM(linkModel,
                        fromNodeViewModel.Id, fromNodeViewModel.PositionX, fromNodeViewModel.PositionY,
                        toNodeViewModel.Id, toNodeViewModel.PositionX, toNodeViewModel.PositionY);

                    RootNodeIds.Remove(toNodeViewModel.Id);
                }
            }
        }

        protected void PrePaste(GraphM graphModel)
        {
            // Clear up nodes

            HashSet<Node_BaseM> nodesToRemove = new HashSet<Node_BaseM>();
            HashSet<string> nodeIdsToRemove = new HashSet<string>();

            foreach (var nodeModel in graphModel.nodes)
            {
                if (!_nodeTypes.Contains(nodeModel.GetType()))
                {
                    nodesToRemove.Add(nodeModel);
                    nodeIdsToRemove.Add(nodeModel.id);
                }
            }

            foreach (var nodeModel in nodesToRemove)
            {
                graphModel.nodes.Remove(nodeModel);
            }

            // Clear up links

            HashSet<LinkM> linksToRemove = new HashSet<LinkM>();

            foreach (var linkModel in graphModel.links)
            {
                if (nodeIdsToRemove.Contains(linkModel.fromNodeId) || nodeIdsToRemove.Contains(linkModel.toNodeId))
                {
                    linksToRemove.Add(linkModel);
                }
            }

            foreach (var linkModel in linksToRemove)
            {
                graphModel.links.Remove(linkModel);
            }
        }

        void AddNode(BaseM model, bool resetSelection)
        {
            Add(GetContext(model), model, null);

            if (model is Node_ExitM) UpdateExitNames(model);

            Notifier viewModel = _vmCreator(model);

            NodesVMs.Add(model.id, viewModel);
            Add(null, null, viewModel);

            FromNodesLinks.Add(model.id, new HashSet<string>());
            ToNodesLinks.Add(model.id, new HashSet<string>());

            RootNodeIds.Add(model.id);

            AddToSelection(viewModel, resetSelection);

            OnModelChanged(Model, nameof(GraphVM<GraphM>.Stats));
        }

        public void Delete()
        {
            List<string> prevSelection = new List<string>();
            GetSelection(prevSelection);

            foreach (string selectedId in prevSelection)
            {
                if (NodesVMs.ContainsKey(selectedId))
                {
                    if (NodesVMs[selectedId] is INode nodeViewModel)
                    {
                        RemoveFromSelection(nodeViewModel);

                        foreach (var fromlinkId in FromNodesLinks[nodeViewModel.Id].ToList()) RemoveLink(fromlinkId);
                        foreach (var tolinkId in ToNodesLinks[nodeViewModel.Id].ToList()) RemoveLink(tolinkId);

                        RemoveNode(nodeViewModel.Id);
                    }
                }
            }
        }

        void SetScale(double transformX, double transformY, double newScale)
        {
            double oldX = transformX / Scale;
            double oldY = transformY / Scale;

            Scale = newScale;

            double newX = transformX / Scale;
            double newY = transformY / Scale;

            TranslateView(newX - oldX, newY - oldY);
        }

        protected ICommand removeElementCommand;
        public ICommand RemoveElementCommand => removeElementCommand ?? (removeElementCommand = new RelayCommand<Notifier>((viewModel) =>
        {
            if (viewModel is INode nodeViewModel)
            {
                RemoveFromSelection(nodeViewModel);

                foreach (var fromlinkId in FromNodesLinks[nodeViewModel.Id].ToList()) RemoveLink(fromlinkId);
                foreach (var tolinkId in ToNodesLinks[nodeViewModel.Id].ToList()) RemoveLink(tolinkId);

                RemoveNode(nodeViewModel.Id);

                CommandManager.InvalidateRequerySuggested();
            }
            else if (viewModel is ILinkVM linkViewModel)
            {
                RemoveLink(linkViewModel.Id);

                CommandManager.InvalidateRequerySuggested();
            }
        }, (viewModel) => viewModel != null));

        protected void RemoveNode(string nodeId)
        {
            if (NodesVMs.ContainsKey(nodeId))
            {
                if (NodesVMs[nodeId] is INode)
                {
                    RootNodeIds.Remove(nodeId);

                    FromNodesLinks.Remove(nodeId);
                    ToNodesLinks.Remove(nodeId);

                    BaseM nodeModel = (NodesVMs[nodeId] as IWithModel)?.GetModel<BaseM>();

                    Remove(NodesVMs[nodeId], null, null);
                    NodesVMs.Remove(nodeId);
                    Remove(null, nodeModel, GetContext(nodeModel));

                    OnModelChanged(Model, nameof(GraphVM<GraphM>.Stats));

                    if (nodeModel is Node_ExitM) UpdateExitNames(null);
                }
            }
        }

        protected void RemoveLink(string linkId)
        {
            if (LinksVMs.ContainsKey(linkId))
            {
                if (LinksVMs[linkId] is ILinkVM linkViewModel)
                {
                    FromNodesLinks[linkViewModel.FromNodeId].Remove(linkViewModel.Id);
                    ToNodesLinks[linkViewModel.ToNodeId].Remove(linkViewModel.Id);

                    BaseM linkModel = (LinksVMs[linkId] as IWithModel)?.GetModel<BaseM>();

                    Remove(LinksVMs[linkId], null, null);
                    LinksVMs.Remove(linkId);
                    Remove(null, linkModel, GetContext(linkModel));

                    if (ToNodesLinks[linkViewModel.ToNodeId].Count == 0)
                    {
                        RootNodeIds.Add(linkViewModel.ToNodeId);
                    }
                }
            }
        }



        protected double offsetX;
        public double OffsetX
        {
            get => offsetX;
            set
            {
                if (offsetX != value)
                {
                    offsetX = value;
                    Notify(nameof(OffsetX));
                }
            }
        }

        protected double offsetY;
        public double OffsetY
        {
            get => offsetY;
            set
            {
                if (offsetY != value)
                {
                    offsetY = value;
                    Notify(nameof(OffsetY));
                }
            }
        }

        protected double scale;
        public double Scale
        {
            get => scale;
            set
            {
                if (scale != value)
                {
                    scale = value;
                    Notify(nameof(Scale));
                }
            }
        }

        protected Type selectedNodeType;
        public Type SelectedNodeType
        {
            get => selectedNodeType;
            set
            {
                if (value != selectedNodeType)
                {
                    selectedNodeType = value;
                    Notify(nameof(SelectedNodeType));

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }



        public override string Id => Model?.id;
        public override IList GetContext(BaseM model) { if (model is LinkM) return Model.links; return Model.nodes; }



        protected readonly Dictionary<string, Notifier> NodesVMs;
        protected readonly Dictionary<string, LinkVM> LinksVMs;
        protected readonly Dictionary<string, HashSet<string>> FromNodesLinks;
        protected readonly Dictionary<string, HashSet<string>> ToNodesLinks;
        private readonly List<string> RootNodeIds;
        private int RootNodeIndex;

        protected double FromLocalToAbsoluteX(double x)
        {
            double result = x / Scale;  // Scale
            result += OffsetX;          // Transaltion
            return result;
        }
        protected double FromLocalToAbsoluteY(double y)
        {
            double result = y / Scale;  // Scale
            result += OffsetY;          // Transaltion
            return result;
        }

        protected double FromAbsoluteToLocalX(double x)
        {
            double result = x - OffsetX;    // Transaltion
            result *= Scale;                // Scale
            return result;
        }
        protected double FromAbsoluteToLocalY(double y)
        {
            double result = y - OffsetY;    // Transaltion
            result *= Scale;                // Scale
            return result;
        }

        private void UpdateLocalPosition(INode nodeViewModel, ENodeUpdateFlags updateFlags)
        {
            if ((updateFlags & ENodeUpdateFlags.X) > 0) nodeViewModel.Left = FromAbsoluteToLocalX(nodeViewModel.PositionX) - nodeViewModel.Width / 2;
            if ((updateFlags & ENodeUpdateFlags.Y) > 0) nodeViewModel.Top = FromAbsoluteToLocalY(nodeViewModel.PositionY) - nodeViewModel.Height / 2;

            if (updateFlags > 0)
            {
                foreach (var linkId in FromNodesLinks[nodeViewModel.Id])
                {
                    LinksVMs[linkId].FromX = nodeViewModel.PositionX;
                    LinksVMs[linkId].FromY = nodeViewModel.PositionY;
                    UpdateLinkLocalPosition(LinksVMs[linkId], ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);
                }

                foreach (var linkId in ToNodesLinks[nodeViewModel.Id])
                {
                    LinksVMs[linkId].ToX = nodeViewModel.PositionX;
                    LinksVMs[linkId].ToY = nodeViewModel.PositionY;
                    UpdateLinkLocalPosition(LinksVMs[linkId], ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);
                }
            }
        }

        private void UpdateLinkLocalPosition(ILinkVM linkViewModel, ELinkVMUpdate updateTarget)
        {
            if ((updateTarget & ELinkVMUpdate.FromX) > 0) linkViewModel.Left = FromAbsoluteToLocalX(linkViewModel.FromX);
            if ((updateTarget & ELinkVMUpdate.FromY) > 0) linkViewModel.Top = FromAbsoluteToLocalY(linkViewModel.FromY);
            if ((updateTarget & ELinkVMUpdate.ToX) > 0) linkViewModel.HandleX = FromAbsoluteToLocalX(linkViewModel.ToX) - linkViewModel.Left;
            if ((updateTarget & ELinkVMUpdate.ToY) > 0) linkViewModel.HandleY = FromAbsoluteToLocalY(linkViewModel.ToY) - linkViewModel.Top;
            if ((updateTarget & ELinkVMUpdate.Scale) > 0) { linkViewModel.Scale = Scale; }

            linkViewModel.RefreshStepPoints();
        }

        private void UpdateSelectionBoxLocalPosition(SelectionBoxVM selectionBoxViewModel, ELinkVMUpdate updateTarget)
        {
            if ((updateTarget & ELinkVMUpdate.FromX) > 0) selectionBoxViewModel.Left = FromAbsoluteToLocalX(selectionBoxViewModel.FromX);
            if ((updateTarget & ELinkVMUpdate.FromY) > 0) selectionBoxViewModel.Top = FromAbsoluteToLocalY(selectionBoxViewModel.FromY);
            if ((updateTarget & ELinkVMUpdate.ToX) > 0) selectionBoxViewModel.HandleX = FromAbsoluteToLocalX(selectionBoxViewModel.ToX) - selectionBoxViewModel.Left;
            if ((updateTarget & ELinkVMUpdate.ToY) > 0) selectionBoxViewModel.HandleY = FromAbsoluteToLocalY(selectionBoxViewModel.ToY) - selectionBoxViewModel.Top;
        }

        private void TranslateView(double absoluteDeltaX, double absoluteDeltaY) { SetView(OffsetX - absoluteDeltaX, OffsetY - absoluteDeltaY); }

        private void SetView(double offsetX, double offsetY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;

            double maxWidth = (double)Application.Current.FindResource("Double_Node_MaxWidth");

            double viewportLeft = OffsetX;
            double viewportRight = viewportLeft + ActiveContext.ViewWidth / Scale;
            double viewportBottom = OffsetY;
            double viewportTop = viewportBottom + ActiveContext.ViewHeight / Scale;

            HashSet<BaseM> keepMs = new HashSet<BaseM>();
            HashSet<BaseM> addMs = new HashSet<BaseM>();
            HashSet<BaseM> removeMs = new HashSet<BaseM>();

            double multiplier = 0.625;// 0.5 * 1.25;

            foreach (var nodeEntry in NodesVMs)
            {
                if (nodeEntry.Value is INode nodeViewModel)
                {
                    if (nodeViewModel.PositionX - nodeViewModel.Width * multiplier <= viewportRight &&
                       nodeViewModel.PositionX + nodeViewModel.Width * multiplier >= viewportLeft &&
                       nodeViewModel.PositionY - nodeViewModel.Height * multiplier <= viewportTop &&
                       nodeViewModel.PositionY + nodeViewModel.Height * multiplier >= viewportBottom)
                    {
                        keepMs.Add((nodeViewModel as IWithModel)?.GetModel<BaseM>());
                    }
                    else
                    {
                        removeMs.Add((nodeViewModel as IWithModel)?.GetModel<BaseM>());
                    }
                }
            }

            foreach (var nodeModel in Model.nodes)
            {
                if (keepMs.Contains(nodeModel)) continue;

                if (removeMs.Contains(nodeModel)) continue;

                if (nodeModel.positionX - maxWidth * multiplier <= viewportRight &&
                    nodeModel.positionX + maxWidth * multiplier >= viewportLeft &&
                    nodeModel.positionY - 2 * maxWidth * multiplier <= viewportTop &&
                    nodeModel.positionY + 2 * maxWidth * multiplier * multiplier >= viewportBottom)
                {
                    addMs.Add(nodeModel);
                }
            }

            foreach (var model in removeMs)
            {
                if (NodesVMs.ContainsKey(model.id))
                {
                    Remove(NodesVMs[model.id], null, null);
                    NodesVMs.Remove(model.id);
                }
            }

            foreach (var model in addMs)
            {
                if (!NodesVMs.ContainsKey(model.id))
                {
                    Notifier viewModel = _vmCreator(model);
                    viewModel.IsSelected = selection.Contains(model.id);

                    NodesVMs.Add(model.id, viewModel);
                    Add(null, null, viewModel);
                }
            }

            foreach (var nodeEntry in NodesVMs) UpdateLocalPosition((INode)nodeEntry.Value, ENodeUpdateFlags.XY);
            foreach (var linkEntry in LinksVMs) UpdateLinkLocalPosition(linkEntry.Value, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);
        }

        public bool SizeChangedFlag { set => TranslateView(0, 0); }



        protected readonly HashSet<string> selection;
        public override void AddToSelection(Notifier vmToSelect, bool resetSelection)
        {
            bool hasChanges = false;

            if (resetSelection)
            {
                hasChanges = selection.Count > 0;

                foreach (var selectedId in selection)
                {
                    if (NodesVMs.ContainsKey(selectedId)) NodesVMs[selectedId].IsSelected = false;
                }

                selection.Clear();
            }

            if (vmToSelect != null && !selection.Contains(vmToSelect.Id))
            {
                if (selection.Add(vmToSelect.Id))
                {
                    vmToSelect.IsSelected = true;

                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                SelectionEditor = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? _evmCreator(NodesVMs[selection.First()]) : null;

                SelectionNode = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? (INode)NodesVMs[selection.First()] : null;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        void RemoveFromSelection(INode nodeViewModel)
        {
            if (nodeViewModel != null)
            {
                if (selection.Remove(nodeViewModel.Id))
                {
                    SelectionEditor = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? _evmCreator(NodesVMs[selection.First()]) : null;

                    SelectionNode = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? (INode)NodesVMs[selection.First()] : null;

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public override void GetSelection(IList outSelection)
        {
            foreach (var selectedViewModel in selection) outSelection.Add(selectedViewModel);
        }
        public override bool HasSelection() => selection.Count > 0;
        public override bool SelectionCanBeDeleted() => true;


        protected INode selectionNode;
        public INode SelectionNode
        {
            get => selectionNode;
            set
            {
                if (value != selectionNode)
                {
                    selectionNode = value;
                    Notify(nameof(SelectionNode));
                }
            }
        }

        public INode FindNode(string nodeId)
        {
            return NodesVMs.ContainsKey(nodeId) ? (NodesVMs[nodeId] as INode) : null;
        }

        public INode GenerateNode(string nodeId)
        {
            Node_BaseM targetNodeModel = Model.nodes.FirstOrDefault((nodeModel) => nodeModel.id == nodeId);
            if (targetNodeModel != null)
            {
                return _vmCreator(targetNodeModel) as INode;
            }

            return null;
        }

        public void MoveTo(IPositioned positioned, Action<CustomStatus> callbackAction, float playRate)
        {
            if (positioned != null)
            {
                StartScrollingTask(positioned, callbackAction, playRate);
            }
            else
            {
                callbackAction(CustomStatus.WaitingToRun);
            }
        }

        public void MoveTo(string targetId, Action<CustomStatus> callbackAction, float playRate)
        {
            Node_BaseM targetNodeModel = Model.nodes.FirstOrDefault(node => node.id == targetId);
            if (targetNodeModel != null)
            {
                string characterId = targetNodeModel is Node_RegularM regularNodeModel ? regularNodeModel.characterId : null;
                MoveTo(new PositionVM(targetNodeModel.positionX, targetNodeModel.positionY, targetId, characterId), callbackAction, playRate);
            }
            else
            {
                callbackAction(CustomStatus.WaitingToRun);
            }
        }

        public List<List<IPositioned>> GetAllPaths(string nodeId)
        {
            List<List<IPositioned>> paths = new List<List<IPositioned>>();

            List<string> nodeIds = Model.links.Where(link => link.fromNodeId == nodeId).Select(link => link.toNodeId).ToList();

            List<Node_BaseM> ntNodeModels = Model.nodes.Where((otherNode) => nodeIds.Contains(otherNode.id) && !(otherNode is Node_TransitM)).ToList();

            foreach (var ntNodeModel in ntNodeModels)
            {
                string characterId = ntNodeModel is Node_RegularM regularNodeModel ? regularNodeModel.characterId : null;
                paths.Add(new List<IPositioned>() { new PositionVM(ntNodeModel.positionX, ntNodeModel.positionY, ntNodeModel.id, characterId) });
            }

            foreach (var tNodeModel in Model.nodes.Where((otherNode) => nodeIds.Contains(otherNode.id) && !ntNodeModels.Contains(otherNode)))
            {
                List<List<IPositioned>> childPaths = GetAllPaths(tNodeModel.id);

                foreach (var path in childPaths)
                {
                    path.Insert(0, new PositionVM(tNodeModel.positionX, tNodeModel.positionY, tNodeModel.id, null));
                    paths.Add(path);
                }
            }

            return paths;
        }

        public void SetPlayerContext(object oldPlayerContext, object newPlayerContext)
        {
            INode playerContextNode = playerIndicator.PlayerContext as INode;

            if (playerContextNode == null && newPlayerContext is INode newNode && !playerIndicator.IsVisible)
            {
                playerIndicator.IsVisible = true;
                ShowPlayerIndicator(newNode);
            }

            playerIndicator.PlayerContext = newPlayerContext;

            if (playerIndicator.PlayerContext == null && playerIndicator.IsVisible)
            {
                HidePlayerIndicator();
                playerIndicator.IsVisible = false;
            }
        }

        public void TickPlayer(double deltaTimeMsec) { playerIndicator.Tick(deltaTimeMsec); }

        public void OnNodeGenderChanged(INode node) { OnModelChanged(Model, nameof(GraphVM<GraphM>.Stats)); }

        public void OnNodePositionChanged(INode node, ENodeUpdateFlags updateFlags) { UpdateLocalPosition(node, updateFlags); }

        public void OnNodeSizeChanged(INode node, ENodeUpdateFlags updateFlags) { UpdateLocalPosition(node, updateFlags); }


        protected virtual string CanLinkNodes(INode from, INode to) { return nameof(NotImplementedException); }
        protected virtual void PreLinkNodes(INode from, INode to) { }

        protected void ShowPreviewLink(INode nodeViewModel)
        {
            previewLink.FromX = nodeViewModel.PositionX;
            previewLink.FromY = nodeViewModel.PositionY;
            previewLink.ToX = nodeViewModel.PositionX;
            previewLink.ToY = nodeViewModel.PositionY;
            UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

            ItemVms.Add(previewLink);
        }
        protected void HidePreviewLink() { ItemVms.Remove(previewLink); }

        protected void ShowSelectionBox(double positionX, double positionY)
        {
            selectionBox.FromX = positionX;
            selectionBox.FromY = positionY;
            selectionBox.ToX = positionX;
            selectionBox.ToY = positionY;
            UpdateSelectionBoxLocalPosition(selectionBox, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

            ItemVms.Add(selectionBox);
        }
        protected void HideSelectionBox() { ItemVms.Remove(selectionBox); }

        protected void ShowPlayerIndicator(INode node) { ItemVms.Add(playerIndicator); }
        protected void HidePlayerIndicator() { ItemVms.Remove(playerIndicator); }

        protected void ProcessSelectionBox()
        {
            HashSet<Notifier> selectionBoxCapture = new HashSet<Notifier>();

            double selectionLeft = Math.Min(selectionBox.FromX, selectionBox.ToX);
            double selectionRight = Math.Max(selectionBox.FromX, selectionBox.ToX);
            double selectionBottom = Math.Min(selectionBox.FromY, selectionBox.ToY);
            double selectionTop = Math.Max(selectionBox.FromY, selectionBox.ToY);

            double multiplier = 0.5;// 0.5 * 1;

            foreach (var nodeEntry in NodesVMs)
            {
                var viewModel = nodeEntry.Value;

                if (viewModel is INode nodeViewModel)
                {
                    if (nodeViewModel.PositionX - nodeViewModel.Width * multiplier <= selectionRight &&
                        nodeViewModel.PositionX + nodeViewModel.Width * multiplier >= selectionLeft &&
                        nodeViewModel.PositionY - nodeViewModel.Height * multiplier <= selectionTop &&
                        nodeViewModel.PositionY + nodeViewModel.Height * multiplier >= selectionBottom)
                    {
                        selectionBoxCapture.Add(viewModel);
                    }
                }
            }

            bool resetSelection = true;
            foreach (var viewModel in selectionBoxCapture)
            {
                AddToSelection(viewModel, resetSelection);
                resetSelection = false;
            }
        }



        public string Name
        {
            get => Model.name;
            set
            {
                if (Model.name != value)
                {
                    Model.name = value;
                    OnModelChanged(Model, nameof(Name));

                    if (ActiveContext.ActiveTab is ICollection_Base collectionBase)
                    {
                        collectionBase.Refresh();
                    }
                }
            }
        }

        public string Description
        {
            get => Model.description;
            set
            {
                if (Model.description != value)
                {
                    Model.description = value;
                    OnModelChanged(Model, nameof(Description));
                }
            }
        }

        protected void OnPostFilterChangedHandler(string filter)
        {
            foreach (var linkEntry in LinksVMs)
            {
                if (NodesVMs.ContainsKey(linkEntry.Value.FromNodeId) && NodesVMs.ContainsKey(linkEntry.Value.ToNodeId))
                {
                    linkEntry.Value.IsFilterPassed = NodesVMs[linkEntry.Value.FromNodeId].IsFilterPassed && NodesVMs[linkEntry.Value.ToNodeId].IsFilterPassed;
                }
            }
        }

        protected void UpdateExitNames(BaseM model)
        {
            if (model != null)
            {
                int count = Model.nodes.Count(node => node is Node_ExitM);
                model.name = (count).ToString();
            }
            else
            {
                int i = 1;
                foreach (Node_BaseM node in Model.nodes)
                {
                    if (node is Node_ExitM)
                    {
                        if (NodesVMs.ContainsKey(node.id))
                        {
                            if (NodesVMs[node.id] is INode nodeViewModel)
                            {
                                nodeViewModel.Name = i.ToString();
                            }
                        }
                        else
                        {
                            node.name = i.ToString();
                        }

                        i++;
                    }
                }
            }
        }

        public override void OnEnter()
        {
            if (SelectionEditor != null)
            {
                SelectionEditor = _evmCreator(NodesVMs[selection.First()]); ////// TODO
            }
        }
    }
}