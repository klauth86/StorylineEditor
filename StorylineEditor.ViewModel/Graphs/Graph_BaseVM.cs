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
using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using StorylineEditor.ViewModel.Helpers;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    enum ENodeVMUpdate
    {
        X = 1,
        Y = 2,
    }

    public abstract class Graph_BaseVM<T> : Collection_BaseVM<T, Point>, ICallbackContext, ICopyPaste, IGraph where T : GraphM
    {
        public Graph_BaseVM(T model, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
            Func<Notifier, ICallbackContext, Notifier> editorCreator, Type defaultNodeType) : base(model, callbackContext,
                modelCreator, viewModelCreator, editorCreator)
        {
            offsetY = offsetX = 0;
            scale = 1;
            selectedNodeType = defaultNodeType;

            targetNodeViewModel = null;

            previewLink = new PreviewLinkVM(new LinkM(), this);

            selectionBox = new SelectionBoxVM();

            playerIndicator = new PlayerIndicatorVM(2, 0.1, 0.2);

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

                if (nodeModel is Node_ReplicaM replicaNodeModel)
                {
                    var flow = FlowDocumentHelper.ConvertBack(replicaNodeModel.description);
                    var flowDocText = FlowDocumentHelper.GetTextFromFlowDoc(flow);
                    replicaNodeModel.name = string.Format("[{0}]: {1}", ActiveContextService.GetCharacter(replicaNodeModel.characterId)?.name ?? "???", flowDocText);
                }
                else if (nodeModel is Node_DialogM dialogNodeModel)
                {
                    var flow = FlowDocumentHelper.ConvertBack(dialogNodeModel.description);
                    var flowDocText = FlowDocumentHelper.GetTextFromFlowDoc(flow);
                    dialogNodeModel.name = string.Format("[{0}]: {1}", ActiveContextService.GetCharacter(dialogNodeModel.characterId)?.name ?? "???", flowDocText);
                }
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

        void StartScrollingTask(IPositioned positioned, Action<TaskStatus> callbackAction)
        {
            const int durationMsec = 256;
            const int stepCount = 256;
            const int stepDuration = durationMsec / stepCount;

            MonoTaskFacade.Start((token, alpha) =>
            {
                if (token.IsCancellationRequested) return TaskStatus.Canceled;

                double stepX = OffsetX - (positioned.PositionX - StorylineVM.ViewWidth / 2 / Scale);
                double stepY = OffsetY - (positioned.PositionY - StorylineVM.ViewHeight / 2 / Scale);

                if (Math.Abs(stepX) < 0.01) return TaskStatus.RanToCompletion;

                playerIndicator.Tick(alpha);

                TranslateView(stepX * alpha, stepY * alpha);

                return TaskStatus.Running;
            },
            TimeSpan.FromMilliseconds(stepDuration),
            1.0 / stepCount,
            (taskStatus) =>
            {
                if (taskStatus == TaskStatus.RanToCompletion)
                {
                    double stepX = OffsetX - (positioned.PositionX - StorylineVM.ViewWidth / 2 / Scale);
                    double stepY = OffsetY - (positioned.PositionY - StorylineVM.ViewHeight / 2 / Scale);

                    playerIndicator.Tick(1);

                    TranslateView(stepX, stepY);
                }
            }, callbackAction);
        }

        void StartScalingTask()
        {
            const int durationMsec = 256;
            const int stepCount = 256;
            const int stepDuration = durationMsec / stepCount;

            MonoTaskFacade.Start((token, alpha) =>
            {
                if (token.IsCancellationRequested) return TaskStatus.Canceled;

                double newScale = Scale * (1 - alpha) + alpha;

                if (Math.Abs(newScale - 1) < 0.01) return TaskStatus.RanToCompletion;

                SetScale(StorylineVM.ViewWidth / 2, StorylineVM.ViewHeight / 2, newScale);

                return TaskStatus.Running;
            },
            TimeSpan.FromMilliseconds(stepDuration),
            1.0 / stepCount,
            (taskStatus) =>
            {
                if (taskStatus == TaskStatus.RanToCompletion)
                {
                    SetScale(StorylineVM.ViewWidth / 2, StorylineVM.ViewHeight / 2, 1);
                }
            }, null);
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
                    StartScrollingTask(new PositionVM(rootNodeModel.positionX, rootNodeModel.positionY, rootNodeModel.id, characterId), null);
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
                    StartScrollingTask(new PositionVM(rootNodeModel.positionX, rootNodeModel.positionY, rootNodeModel.id, characterId), null);
                }
            }
        }, () => RootNodeIds.Count > 0));

        protected ICommand resetScaleCommand;
        public ICommand ResetScaleCommand => resetScaleCommand ?? (resetScaleCommand = new RelayCommand(() => StartScalingTask()));

        protected ICommand goToOriginCommand;
        public ICommand GoToOriginCommand => goToOriginCommand ?? (goToOriginCommand = new RelayCommand(() => StartScrollingTask(OriginVM.GetOrigin(), null)));

        protected ICommand playCommand;
        public ICommand PlayCommand => playCommand ?? (playCommand = new RelayCommand(() => { CallbackContext?.Callback(this, nameof(HistoryVM)); }, () => HasSelection()));

        protected ICommand selectCommand;
        public override ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((viewModel) => { }));
        protected void AddLinkVM(LinkM model, string fromId, double fromX, double fromY, string toId, double toX, double toY)
        {
            LinkVM viewModel = (LinkVM)_viewModelCreator(model, this);

            viewModel.FromX = fromX;
            viewModel.FromY = fromY;
            viewModel.ToX = toX;
            viewModel.ToY = toY;

            LinksVMs.Add(model.id, viewModel);
            Add(null, viewModel);

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
                            Node_BaseM nodeModel = Model.nodes.FirstOrDefault((node) => node.id == selectedId); ////// TODO Cache if will be slow
                            if (nodeModel != null)
                            {
                                nodeModel.positionX += deltaX;
                                nodeModel.positionY += deltaY;
                            }
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
                            Node_BaseM nodeModel = Model.nodes.FirstOrDefault((node) => node.id == selectedId); ////// TODO Cache if will be slow
                            if (nodeModel != null)
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

            PostFilterChangedEvent += OnPostFilterChangedHandler;
        }, (args) => args != null));

        protected ICommand unInitCommand;
        public ICommand UnInitCommand => unInitCommand ?? (unInitCommand = new RelayCommand<RoutedEventArgs>((args) =>
        {
            PostFilterChangedEvent -= OnPostFilterChangedHandler;
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

        protected ICommand facadeCommand;
        public ICommand FacadeCommand => facadeCommand ?? (facadeCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
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

        private void CreateNodeImpl(MouseButtonEventArgs args)
        {
            Point position = args.GetPosition((IInputElement)args.Source);

            position.X = FromLocalToAbsoluteX(position.X);
            position.Y = FromLocalToAbsoluteY(position.Y);

            BaseM model = _modelCreator(SelectedNodeType, position);

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

                                    LinkM model = (LinkM)_modelCreator(typeof(LinkM), new Point());
                                    model.fromNodeId = targetNodeViewModel.Id;
                                    model.toNodeId = toNodeViewModel.Id;

                                    Add(model, null);

                                    AddLinkVM(model,
                                        targetNodeViewModel.Id, targetNodeViewModel.PositionX, targetNodeViewModel.PositionY,
                                        toNodeViewModel.Id, toNodeViewModel.PositionX, toNodeViewModel.PositionY);

                                    toNodeViewModel.IsRoot = false;
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

            return SerializeService.Serialize(graphModelCopy);
        }

        public void Paste(string clipboard)
        {
            GraphM graphModel = SerializeService.Deserialize<GraphM>(clipboard);

            if (graphModel != null)
            {
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
                    Add(linkModel, null);

                    INode fromNodeViewModel = NodesVMs[linkModel.fromNodeId] as INode;
                    INode toNodeViewModel = NodesVMs[linkModel.toNodeId] as INode;

                    AddLinkVM(linkModel,
                        fromNodeViewModel.Id, fromNodeViewModel.PositionX, fromNodeViewModel.PositionY,
                        toNodeViewModel.Id, toNodeViewModel.PositionX, toNodeViewModel.PositionY);

                    toNodeViewModel.IsRoot = false;
                    RootNodeIds.Remove(toNodeViewModel.Id);
                }
            }
        }

        void AddNode(BaseM model, bool resetSelection)
        {
            Add(model, null);

            if (model is Node_ExitM) UpdateExitNames(model);

            Notifier viewModel = _viewModelCreator(model, this);

            NodesVMs.Add(model.id, viewModel);
            Add(null, viewModel);

            FromNodesLinks.Add(model.id, new HashSet<string>());
            ToNodesLinks.Add(model.id, new HashSet<string>());

            RootNodeIds.Add(model.id);
            ((INode)NodesVMs[model.id]).IsRoot = true;

            AddToSelection(viewModel, resetSelection);

            OnModelChanged(Model, nameof(Stats));
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

                    OnModelChanged(Model, nameof(Stats));

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
                        if (NodesVMs.ContainsKey(linkViewModel.ToNodeId)) ((INode)NodesVMs[linkViewModel.ToNodeId]).IsRoot = true;
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



        public override string Id => null;
        public override IList GetContext(BaseM model) { if (model is LinkM) return Model.links; return Model.nodes; }



        protected readonly Dictionary<string, Notifier> NodesVMs;
        protected readonly Dictionary<string, LinkVM> LinksVMs;
        protected readonly Dictionary<string, HashSet<string>> FromNodesLinks;
        protected readonly Dictionary<string, HashSet<string>> ToNodesLinks;
        private readonly List<string> RootNodeIds;
        private int RootNodeIndex;

        protected double FromLocalToAbsoluteX(double x)
        {
            double result = x / Scale;     // Scale
            result += OffsetX;              // Transaltion
            return result;
        }
        protected double FromLocalToAbsoluteY(double y)
        {
            double result = y / Scale;     // Scale
            result += OffsetY;              // Transaltion
            return result;
        }

        protected double FromAbsoluteToLocalX(double x)
        {
            double result = x - OffsetX;    // Transaltion
            result *= Scale;               // Scale
            return result;
        }
        protected double FromAbsoluteToLocalY(double y)
        {
            double result = y - OffsetY;    // Transaltion
            result *= Scale;               // Scale
            return result;
        }



        public void Callback(object viewModelObj, string propName)
        {
            if (propName == nameof(ICallbackContext) || propName == nameof(HistoryVM))
            {
                CallbackContext?.Callback(viewModelObj, propName);
            }
            else
            {
                if (viewModelObj is INode nodeViewModel)
                {
                    if (propName == nameof(INode.PositionX)) UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.X);
                    else if (propName == nameof(INode.PositionY)) UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.Y);
                    else if (propName == nameof(INode.Gender)) OnModelChanged(Model, nameof(Stats));
                }
            }
        }

        private void UpdateLocalPosition(INode nodeViewModel, ENodeVMUpdate updateTarget)
        {
            if ((updateTarget & ENodeVMUpdate.X) > 0) nodeViewModel.Left = FromAbsoluteToLocalX(nodeViewModel.PositionX) - nodeViewModel.Width / 2;
            if ((updateTarget & ENodeVMUpdate.Y) > 0) nodeViewModel.Top = FromAbsoluteToLocalY(nodeViewModel.PositionY) - nodeViewModel.Height / 2;

            if (updateTarget > 0)
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

        private void TranslateView(double absoluteDeltaX, double absoluteDeltaY)
        {
            OffsetX -= absoluteDeltaX;
            OffsetY -= absoluteDeltaY;

            double maxWidth = (double)Application.Current.FindResource("Double_Node_MaxWidth");

            double viewportLeft = OffsetX;
            double viewportRight = viewportLeft + StorylineVM.ViewWidth / Scale;
            double viewportBottom = OffsetY;
            double viewportTop = viewportBottom + StorylineVM.ViewHeight / Scale;

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
                    Notifier viewModel = _viewModelCreator(model, this);
                    viewModel.IsSelected = selection.Contains(model.id);
                    ((INode)viewModel).IsRoot = RootNodeIds.Contains(model.id);

                    NodesVMs.Add(model.id, viewModel);
                    Add(null, viewModel);
                }
            }

            foreach (var nodeEntry in NodesVMs) UpdateLocalPosition((INode)nodeEntry.Value, ENodeVMUpdate.X | ENodeVMUpdate.Y);
            foreach (var linkEntry in LinksVMs) UpdateLinkLocalPosition(linkEntry.Value, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);
        }



        public bool SizeChangedFlag { set => TranslateView(0, 0); }



        protected readonly HashSet<string> selection;
        public override void AddToSelection(Notifier viewModel, bool resetSelection)
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

            if (viewModel != null && !selection.Contains(viewModel.Id))
            {
                if (selection.Add(viewModel.Id))
                {
                    viewModel.IsSelected = true;

                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                SelectionEditor = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? _editorCreator(NodesVMs[selection.First()], this) : null;

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
                    SelectionEditor = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? _editorCreator(NodesVMs[selection.First()], this) : null;

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
                return _viewModelCreator(targetNodeModel, this) as INode;
            }

            return null;
        }

        public void MoveTo(IPositioned positioned, Action<TaskStatus> callbackAction)
        {
            if (positioned != null)
            {
                StartScrollingTask(positioned, callbackAction);
            }
            else
            {
                callbackAction(TaskStatus.WaitingForActivation);
            }
        }

        public void MoveTo(string targetId, Action<TaskStatus> callbackAction)
        {
            Node_BaseM targetNodeModel = Model.nodes.FirstOrDefault(node => node.id == targetId);
            if (targetNodeModel != null)
            {
                string characterId = targetNodeModel is Node_RegularM regularNodeModel ? regularNodeModel.characterId : null;
                MoveTo(new PositionVM(targetNodeModel.positionX, targetNodeModel.positionY, targetId, characterId), callbackAction);
            }
            else
            {
                callbackAction(TaskStatus.WaitingForActivation);
            }
        }

        public Dictionary<string, List<IPositioned>> GetNext(string nodeId)
        {
            Dictionary<string, List<IPositioned>> result = new Dictionary<string, List<IPositioned>>();

            List<string> nodeIds = Model.links.Where(link => link.fromNodeId == nodeId).Select(link => link.toNodeId).ToList();

            List<Node_BaseM> nonTransitNodes = Model.nodes.Where((otherNode) => nodeIds.Contains(otherNode.id) && !(otherNode is Node_TransitM)).ToList();

            foreach (var nonTransitNodeModel in nonTransitNodes)
            {
                if (!result.ContainsKey(nonTransitNodeModel.id)) result.Add(nonTransitNodeModel.id, new List<IPositioned>());
                string characterId = nonTransitNodeModel is Node_RegularM regularNodeModel ? regularNodeModel.characterId : null;
                result[nonTransitNodeModel.id].Add(new PositionVM(nonTransitNodeModel.positionX, nonTransitNodeModel.positionY, nonTransitNodeModel.id, characterId));
            }

            foreach (var transitNode in Model.nodes.Where((otherNode) => nodeIds.Contains(otherNode.id) && !nonTransitNodes.Contains(otherNode)))
            {
                Dictionary<string, List<IPositioned>> childResult = GetNext(transitNode.id);

                foreach (var childResultEntry in childResult)
                {
                    if (result.ContainsKey(childResultEntry.Key))
                    {
                        continue; // One path to nonTransit node is enough
                    }
                    else
                    {
                        List<IPositioned> nodesPath = childResultEntry.Value ?? new List<IPositioned>();
                        nodesPath.Insert(0, new PositionVM(transitNode.positionX, transitNode.positionY, null, null));
                        result.Add(childResultEntry.Key, nodesPath);
                    }
                }
            }

            return result;
        }

        public void SetPlayerContext(object oldPlayerContext, object newPlayerContext)
        {
            INode playerContextNode = playerIndicator.PlayerContext as INode;

            if (playerContextNode == null && newPlayerContext is INode newNode && !playerIndicator.IsVisible)
            {
                playerIndicator.Show(scale);
                ShowPlayerIndicator(newNode);
            }

            playerIndicator.PlayerContext = newPlayerContext;

            if (playerIndicator.PlayerContext == null && playerIndicator.IsVisible)
            {
                HidePlayerIndicator();
                playerIndicator.Hide();
            }
        }

        public void TickPlayer(double alpha) { playerIndicator.Tick(alpha); }

        protected virtual string CanLinkNodes(INode from, INode to) { return nameof(NotImplementedException); }
        protected virtual void PreLinkNodes(INode from, INode to) { }

        protected void ShowPreviewLink(INode nodeViewModel)
        {
            previewLink.FromX = nodeViewModel.PositionX;
            previewLink.FromY = nodeViewModel.PositionY;
            previewLink.ToX = nodeViewModel.PositionX;
            previewLink.ToY = nodeViewModel.PositionY;
            UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

            ItemsVMs.Add(previewLink);
        }
        protected void HidePreviewLink() { ItemsVMs.Remove(previewLink); }

        protected void ShowSelectionBox(double positionX, double positionY)
        {
            selectionBox.FromX = positionX;
            selectionBox.FromY = positionY;
            selectionBox.ToX = positionX;
            selectionBox.ToY = positionY;
            UpdateSelectionBoxLocalPosition(selectionBox, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

            ItemsVMs.Add(selectionBox);
        }
        protected void HideSelectionBox() { ItemsVMs.Remove(selectionBox); }

        protected void ShowPlayerIndicator(INode node) { ItemsVMs.Add(playerIndicator); }
        protected void HidePlayerIndicator() { ItemsVMs.Remove(playerIndicator); }

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
                    CallbackContext?.Callback(this, nameof(Name));

                    OnModelChanged(Model, nameof(Title));

                    OnModelChanged(Model, nameof(Stats));
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

                    OnModelChanged(Model, nameof(Stats));
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

        public static string GetStats(GraphM graphModel)
        {
            string result = "";

            Dictionary<string, Dictionary<string, int>> countByCharacter = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, int> countByType = new Dictionary<string, int>();

            int characterKeyMaxLength = 0;
            int typeKeyMaxLength = 0;

            foreach (var node in graphModel.nodes)
            {
                // Characters

                string characterKey = node is Node_RegularM regularNode ? ActiveContextService.GetCharacter(regularNode.characterId)?.name : "N/A";

                string gender = " ";
                if (node.gender == GENDER.MALE) gender = Application.Current.FindResource("String_Icon_Gender_Male")?.ToString();
                if (node.gender == GENDER.FEMALE) gender = Application.Current.FindResource("String_Icon_Gender_Female")?.ToString();

                if (!countByCharacter.ContainsKey(characterKey))
                {
                    countByCharacter.Add(characterKey, new Dictionary<string, int>()
                        {
                            { " ", 0 },
                            { Application.Current.FindResource("String_Icon_Gender_Male")?.ToString(), 0 },
                            { Application.Current.FindResource("String_Icon_Gender_Female")?.ToString(), 0 },
                        }
                    );
                    characterKeyMaxLength = Math.Max(characterKey?.Length ?? 0, characterKeyMaxLength);
                }

                countByCharacter[characterKey][gender]++;

                // Types

                var typeName = node.GetType().Name;
                var typeKey = Application.Current.FindResource(string.Format("String_Stats_{0}_TmpDescription", typeName))?.ToString();

                if (!countByType.ContainsKey(typeKey))
                {
                    countByType.Add(typeKey, 0);
                    typeKeyMaxLength = Math.Max(typeKey?.Length ?? 0, typeKeyMaxLength);
                }

                countByType[typeKey]++;
            }

            if (countByCharacter.Count > 0)
            {
                // Delimiter
                result += Environment.NewLine;

                result += Application.Current.FindResource("String_Stats_Characters")?.ToString() + Environment.NewLine;
                result += Environment.NewLine;

                foreach (var entry in countByCharacter.OrderBy(pair => pair.Key))
                {
                    result += string.Format("{0, -" + (characterKeyMaxLength + 6) + "}{1}",
                        "- " + entry.Key + ":",
                        string.Join("\t", entry.Value.Select(pair => string.Format("{0}{1, -6}", pair.Key, pair.Value)))
                        ) + Environment.NewLine;
                }
            }

            if (countByType.Count > 0)
            {
                // Delimiter
                result += Environment.NewLine;

                result += Application.Current.FindResource("String_Stats_Types")?.ToString() + Environment.NewLine;
                result += Environment.NewLine;

                foreach (var entry in countByType.OrderBy(pair => pair.Key))
                {
                    result += string.Format("{0, -" + (typeKeyMaxLength + 6) + "}{1}",
                        "- " + entry.Key + ": ",
                        entry.Value
                        ) + Environment.NewLine;
                }
            }

            return result;
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
                                nodeViewModel.Name = (i).ToString();
                            }
                        }
                        else
                        {
                            node.name = (i).ToString();
                        }

                        i++;
                    }
                }
            }
        }
    }
}