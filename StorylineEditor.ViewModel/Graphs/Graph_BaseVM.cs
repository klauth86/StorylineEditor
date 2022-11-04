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

    public class Graph_BaseVM<T> : Collection_BaseVM<T, Point>, ICallbackContext, IActiveContext where T : GraphM
    {
        public Graph_BaseVM(T model, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
            Func<Notifier, ICallbackContext, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor, Type defaultNodeType, Func<Type, string> typeDescriptor) : base(model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, modelExtractor)
        {
            offsetY = offsetX = 0;
            scale = 1;
            selectedNodeType = defaultNodeType;
            _typeDescriptor = typeDescriptor ?? throw new ArgumentNullException(nameof(typeDescriptor));

            fromNodeViewModel = null;
            isDragging = false;
            draggedNodeViewModel = null;

            previewLinkIsAdded = false;
            previewLink = new PreviewLinkVM(new LinkM(), this);

            selectionBoxIsAdded = false;
            selectionBox = new SelectionBoxVM();

            viewRect = new Rect();
            nodeRect = new Rect();
            absMaxHeight = (double)Application.Current.FindResource("Double_Node_MaxHeight");
            absMaxWidth = (double)Application.Current.FindResource("Double_Node_MaxWidth");

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
                    linkModel.fromNodeId, nodesPositions[linkModel.fromNodeId].Item2, nodesPositions[linkModel.fromNodeId].Item1,
                    linkModel.toNodeId, nodesPositions[linkModel.toNodeId].Item2, nodesPositions[linkModel.toNodeId].Item1);
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

        protected bool cancelFlag;        
        protected Task activeTask;
        protected double targetPositionX;
        protected double targetPositionY;

        async void StartScrollingTask()
        {
            const int waitDurationMsec = 8;

            if (activeTask != null && !activeTask.IsCompleted)
            {
                cancelFlag = true;
                while (!activeTask.IsCompleted) await Task.Delay(waitDurationMsec);
            }

            const int durationMsec = 256;
            const int stepCount = 256;
            const int stepDuration = durationMsec / stepCount;

            cancelFlag = false;
            activeTask = Task.Run(async () =>
            {
                double targetOffsetX;
                double targetOffsetY;

                double deltaAlpha = 1.0 / stepCount;
                double currentAlpha = deltaAlpha;

                for (int i = 1; i < stepCount; i++)
                {
                    if (Application.Current == null) return;

                    if (cancelFlag) return;

                    targetOffsetX = targetPositionX - StorylineVM.ViewWidth / 2 / Scale;
                    targetOffsetY = targetPositionY - StorylineVM.ViewHeight / 2 / Scale;

                    double stepX = (OffsetX - targetOffsetX) * currentAlpha;
                    double stepY = (OffsetY - targetOffsetY) * currentAlpha;

                    if (Math.Abs(stepX) < 0.01) break;

                    Application.Current?.Dispatcher?.Invoke(new Action(() => { TranslateView(stepX, stepY); }));

                    currentAlpha += deltaAlpha;

                    await Task.Delay(stepDuration);
                }

                targetOffsetX = targetPositionX - StorylineVM.ViewWidth / 2 / Scale;
                targetOffsetY = targetPositionY - StorylineVM.ViewHeight / 2 / Scale;

                double lastStepX = (OffsetX - targetOffsetX);
                double lastStepY = (OffsetY - targetOffsetY);
                Application.Current?.Dispatcher?.Invoke(new Action(() => { TranslateView(lastStepX, lastStepY); }));
            });
        }

        async void StartScalingTask()
        {
            const int waitDurationMsec = 8;

            if (activeTask != null && !activeTask.IsCompleted)
            {
                cancelFlag = true;
                while (!activeTask.IsCompleted) await Task.Delay(waitDurationMsec);
            }

            const int durationMsec = 256;
            const int stepCount = 256;
            const int stepDuration = durationMsec / stepCount;

            cancelFlag = false;
            activeTask = Task.Run(async () =>
            {
                double deltaAlpha = 1.0 / stepCount;
                double currentAlpha = deltaAlpha;

                for (int i = 1; i < stepCount; i++)
                {
                    if (Application.Current == null) return;

                    if (cancelFlag) return;

                    double newScale = Scale * (1 - currentAlpha) + currentAlpha;

                    if (Math.Abs(newScale - 1) < 0.01) break;

                    Application.Current?.Dispatcher?.Invoke(new Action(() => { SetScale(StorylineVM.ViewWidth / 2, StorylineVM.ViewHeight / 2, newScale); }));

                    currentAlpha += deltaAlpha;

                    await Task.Delay(stepDuration);
                }

                Application.Current?.Dispatcher?.Invoke(new Action(() => { SetScale(StorylineVM.ViewWidth / 2, StorylineVM.ViewHeight / 2, 1); }));
            });
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ?? (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type));

        protected ICommand prevRootNodeCommand;
        public ICommand PrevRootNodeCommand => prevRootNodeCommand ?? (prevRootNodeCommand = new RelayCommand(() =>
        {
            RootNodeIndex = (RootNodeIndex + 1 + RootNodeIds.Count) % RootNodeIds.Count;

            if (RootNodeIds.Count > 0 && RootNodeIndex >= 0 && RootNodeIndex < RootNodeIds.Count)
            {
                Node_BaseM targetNodeModel = Model.nodes.FirstOrDefault((nodeModel) => nodeModel?.id == RootNodeIds[RootNodeIndex]);
                if (targetNodeModel != null)
                {
                    targetPositionX = targetNodeModel.positionX;
                    targetPositionY = targetNodeModel.positionY;

                    StartScrollingTask();
                }
            }
        }, () => RootNodeIds.Count > 0));

        protected ICommand nextRootNodeCommand;
        public ICommand NextRootNodeCommand => nextRootNodeCommand ?? (nextRootNodeCommand = new RelayCommand(() =>
        {
            RootNodeIndex = (RootNodeIndex - 1 + RootNodeIds.Count) % RootNodeIds.Count;
            
            if (RootNodeIds.Count > 0 && RootNodeIndex >= 0 && RootNodeIndex < RootNodeIds.Count)
            {
                Node_BaseM targetNodeModel = Model.nodes.FirstOrDefault((nodeModel) => nodeModel?.id == RootNodeIds[RootNodeIndex]);
                if (targetNodeModel != null)
                {
                    targetPositionX = targetNodeModel.positionX;
                    targetPositionY = targetNodeModel.positionY;

                    StartScrollingTask();
                }
            }
        }, ()=> RootNodeIds.Count > 0));

        protected ICommand addCommand;
        public override ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (isDragging) return;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                if (args.Source is IInputElement inputElement)
                {
                    prevPosition = args.GetPosition(null);

                    Point position = args.GetPosition(inputElement);

                    double positionX = FromLocalToAbsoluteX(position.X);
                    double positionY = FromLocalToAbsoluteY(position.Y);

                    ShowSelectionBox(positionX, positionY);
                }
            }
            else if ((args.OriginalSource as FrameworkElement)?.DataContext is INodeVM nodeViewModel)
            {
                bool resetSelection = !Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                AddToSelection((Notifier)nodeViewModel, resetSelection);

                if (resetSelection) fromNodeViewModel = nodeViewModel;
            }
            else if (args.Source is IInputElement inputElement)
            {
                if (SelectedNodeType != null)
                {
                    Point position = args.GetPosition(inputElement);

                    position.X = FromLocalToAbsoluteX(position.X);
                    position.Y = FromLocalToAbsoluteY(position.Y);

                    BaseM model = _modelCreator(SelectedNodeType, position);

                    bool resetSelection = !Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                    
                    AddNode(model, resetSelection, true);
                }
            }
        }));

        protected ICommand resetScaleCommand;
        public ICommand ResetScaleCommand => resetScaleCommand ?? (resetScaleCommand = new RelayCommand(() => StartScalingTask()));

        protected ICommand goToOriginCommand;
        public ICommand GoToOriginCommand => goToOriginCommand ?? (goToOriginCommand = new RelayCommand(() =>
        {
            targetPositionX = StorylineVM.ViewWidth / 2 / Scale;
            targetPositionY = StorylineVM.ViewHeight / 2 / Scale;

            StartScrollingTask();
        }));

        protected ICommand selectCommand;
        public override ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((viewModel) => { }));

        protected ICommand linkCommand;
        public ICommand LinkCommand => linkCommand ?? (linkCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (selectionBoxIsAdded)
            {
                ProcessSelectionBox();
                HideSelectionBox();
            }
            else if (previewLinkIsAdded)
            {
                if (fromNodeViewModel != null)
                {
                    if ((args.OriginalSource as FrameworkElement)?.DataContext is INodeVM toNodeViewModel)
                    {
                        string message = CanLinkNodes(fromNodeViewModel, toNodeViewModel);
                        if (message == string.Empty)
                        {
                            PreLinkNodes(fromNodeViewModel, toNodeViewModel);

                            LinkM model = (LinkM)_modelCreator(typeof(LinkM), new Point());
                            model.fromNodeId = fromNodeViewModel.Id;
                            model.toNodeId = toNodeViewModel.Id;

                            Add(model, null);

                            AddLinkVM(model,
                                fromNodeViewModel.Id, fromNodeViewModel.PositionX, fromNodeViewModel.PositionY, 
                                toNodeViewModel.Id, toNodeViewModel.PositionX, toNodeViewModel.PositionY);

                            toNodeViewModel.IsRoot = false;
                            RootNodeIds.Remove(toNodeViewModel.Id);

                            CommandManager.InvalidateRequerySuggested();
                        }
                    }

                    HidePreviewLink();

                    fromNodeViewModel = null;
                }
            }
        }));
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

        protected INodeVM fromNodeViewModel;
        protected bool isDragging;
        protected INodeVM draggedNodeViewModel;
        protected Point prevPosition;
        protected bool previewLinkIsAdded;
        protected LinkVM previewLink;
        protected bool selectionBoxIsAdded;
        protected SelectionBoxVM selectionBox;
        protected Rect viewRect;
        protected Rect nodeRect;
        protected readonly double absMaxHeight;
        protected readonly double absMaxWidth;

        protected ICommand dragCommand;
        public ICommand DragCommand => dragCommand ?? (dragCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (fromNodeViewModel != null || selectionBoxIsAdded) return;

            prevPosition = args.GetPosition(null);

            draggedNodeViewModel = (args.OriginalSource as FrameworkElement)?.DataContext as INodeVM;
            isDragging = true;

            CommandManager.InvalidateRequerySuggested();
        }));

        protected ICommand moveCommand;
        public ICommand MoveCommand => moveCommand ?? (moveCommand = new RelayCommand<MouseEventArgs>((args) =>
        {
            if (selectionBoxIsAdded)
            {
                Point position = args.GetPosition(null);

                if (Mouse.LeftButton != MouseButtonState.Pressed)
                {
                    ProcessSelectionBox();
                    HideSelectionBox();
                }
                else
                {
                    double deltaX = (position.X - prevPosition.X) / Scale;
                    double deltaY = (position.Y - prevPosition.Y) / Scale;

                    selectionBox.ToX += deltaX;
                    selectionBox.ToY += deltaY;

                    UpdateSelectionBoxLocalPosition(selectionBox, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);
                }

                prevPosition = position;
            }
            else if (isDragging)
            {
                Point position = args.GetPosition(null);

                if (Mouse.RightButton != MouseButtonState.Pressed)
                {
                    isDragging = false;
                    draggedNodeViewModel = null;
                }
                else
                {
                    double deltaX = (position.X - prevPosition.X) / Scale;
                    double deltaY = (position.Y - prevPosition.Y) / Scale;

                    if (draggedNodeViewModel != null)
                    {
                        if (!draggedNodeViewModel.IsSelected)
                        {
                            draggedNodeViewModel.PositionX += deltaX;
                            draggedNodeViewModel.PositionY += deltaY;
                        }

                        foreach (string selectedId in selection)
                        {
                            if (NodesVMs.ContainsKey(selectedId))
                            {
                                if (NodesVMs[selectedId] is INodeVM nodeViewModel)
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
                }

                prevPosition = position;
            }
            else if (fromNodeViewModel != null)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if ((args.OriginalSource as FrameworkElement)?.DataContext is INodeVM toNodeViewModel)
                    {
                        if (fromNodeViewModel == toNodeViewModel)
                        {
                            HidePreviewLink();
                        }
                        else
                        {
                            previewLink.ToX = toNodeViewModel.PositionX;
                            previewLink.ToY = toNodeViewModel.PositionY;
                            UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                            previewLink.Description = CanLinkNodes(fromNodeViewModel, toNodeViewModel);

                            ShowPreviewLink(fromNodeViewModel);
                        }
                    }
                    else
                    {
                        Point position = args.GetPosition(args.Source as UIElement);

                        previewLink.ToX = FromLocalToAbsoluteX(position.X);
                        previewLink.ToY = FromLocalToAbsoluteY(position.Y);
                        UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                        previewLink.Description = null;

                        ShowPreviewLink(fromNodeViewModel);
                    }
                }
                else
                {
                    HidePreviewLink();

                    fromNodeViewModel = null;
                }
            }
        }));

        protected ICommand initCommand;
        public ICommand InitCommand => initCommand ?? (initCommand = new RelayCommand<RoutedEventArgs>((args) => TranslateView(0, 0), (args) => args != null));

        protected ICommand scaleCommand;
        public ICommand ScaleCommand => scaleCommand ?? (scaleCommand = new RelayCommand<MouseWheelEventArgs>((args) =>
        {
            if (fromNodeViewModel != null || selectionBoxIsAdded) return;

            if (isDragging) return;

            if (args.Source is UIElement uiElement)
            {
                Point position = args.GetPosition(uiElement);                
                SetScale(position.X, position.Y, Math.Max(Math.Min(Scale + args.Delta * 0.0002, 4), 1.0 / 64));
            }
        }));

        public void Copy()
        {
            ////// TODO Move to Model Clone method

            GraphM graphModel = new QuestM();

            foreach (string selectedId in selection)
            {
                if (NodesVMs.ContainsKey(selectedId))
                {
                    if (_modelExtractor(NodesVMs[selectedId]) is Node_BaseM nodeModel)
                    {
                        graphModel.nodes.Add(nodeModel);
                    }
                }
            }

            foreach (string selectedId in selection)
            {
                foreach (var linkId in FromNodesLinks[selectedId])
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

            // Replace ids

            Dictionary<string, string> idsMapping = new Dictionary<string, string>();

            for (int i = 0; i < graphModel.nodes.Count; i++)
            {
                if (graphModel.nodes[i] is Node_StepM stepNodeModel)
                {
                    Node_StepM dummyStepNodeModel = new Node_StepM(i);
                    idsMapping.Add(graphModel.nodes[i].id, dummyStepNodeModel.id);
                    graphModel.nodes[i].id = dummyStepNodeModel.id;
                }
                else if (graphModel.nodes[i] is Node_AlternativeM alternativeNodeModel)
                {
                    Node_AlternativeM dummyAlternativeNodeModel = new Node_AlternativeM(i);
                    idsMapping.Add(graphModel.nodes[i].id, dummyAlternativeNodeModel.id);
                    graphModel.nodes[i].id = dummyAlternativeNodeModel.id;
                }
            }

            for (int i = 0; i < graphModel.links.Count; i++)
            {
                LinkM dummyLinkModel = new LinkM(i);
                graphModel.links[i].id = dummyLinkModel.id;
                graphModel.links[i].fromNodeId = idsMapping[graphModel.links[i].fromNodeId];
                graphModel.links[i].toNodeId = idsMapping[graphModel.links[i].toNodeId];
            }


        }

        public void Paste()
        {
            object obj = null;

            if (obj is GraphM graphModel)
            {
                bool resetSelection = true;

                foreach (var model in graphModel.nodes)
                {
                    model.positionX += OffsetX;
                    model.positionY += OffsetY;

                    AddNode(model, resetSelection, false);
                    resetSelection = false;
                }

                foreach (var model in graphModel.links)
                {

                }
            }
        }

        void AddNode(BaseM model, bool resetSelection, bool tryStartLink)
        {
            Add(model, null);

            Notifier viewModel = _viewModelCreator(model, this);

            NodesVMs.Add(model.id, viewModel);
            Add(null, viewModel);

            FromNodesLinks.Add(model.id, new HashSet<string>());
            ToNodesLinks.Add(model.id, new HashSet<string>());

            RootNodeIds.Add(model.id);
            ((INodeVM)NodesVMs[model.id]).IsRoot = true;

            AddToSelection(viewModel, resetSelection);

            if (resetSelection && tryStartLink) fromNodeViewModel = viewModel as INodeVM;
        }

        public void Delete()
        {
            List<string> prevSelection = new List<string>();
            GetSelection(prevSelection);

            foreach (string selectedId in prevSelection)
            {
                if (NodesVMs.ContainsKey(selectedId))
                {
                    if (NodesVMs[selectedId] is INodeVM nodeViewModel)
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
            if (viewModel is INodeVM nodeViewModel)
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
                if (NodesVMs[nodeId] is INodeVM)
                {
                    RootNodeIds.Remove(nodeId);

                    FromNodesLinks.Remove(nodeId);
                    ToNodesLinks.Remove(nodeId);

                    BaseM nodeModel = _modelExtractor(NodesVMs[nodeId]);

                    Remove(NodesVMs[nodeId], null, null);
                    NodesVMs.Remove(nodeId);
                    Remove(null, nodeModel, GetContext(nodeModel));
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

                    BaseM linkModel = _modelExtractor(LinksVMs[linkId]);

                    Remove(LinksVMs[linkId], null, null);
                    LinksVMs.Remove(linkId);
                    Remove(null, linkModel, GetContext(linkModel));

                    if (ToNodesLinks[linkViewModel.ToNodeId].Count == 0)
                    {
                        RootNodeIds.Add(linkViewModel.ToNodeId);
                        if (NodesVMs.ContainsKey(linkViewModel.ToNodeId)) ((INodeVM)NodesVMs[linkViewModel.ToNodeId]).IsRoot = true;
                    }
                }            
            }
        }



        private readonly Func<Type, string> _typeDescriptor;



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
                    Notify(nameof(SelectedNodeTypeName));

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public string SelectedNodeTypeName => _typeDescriptor(SelectedNodeType);



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
            if (viewModelObj is INodeVM nodeViewModel)
            {
                if (propName == nameof(INodeVM.PositionX)) UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.X);
                else if (propName == nameof(INodeVM.PositionY)) UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.Y);
            }
        }

        private void UpdateLocalPosition(INodeVM nodeViewModel, ENodeVMUpdate updateTarget)
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

            viewRect.X = OffsetX;
            viewRect.Y = OffsetY;
            viewRect.Width = StorylineVM.ViewWidth / Scale;
            viewRect.Height = StorylineVM.ViewHeight / Scale;

            HashSet<BaseM> keepMs = new HashSet<BaseM>();
            HashSet<BaseM> addMs = new HashSet<BaseM>();
            HashSet<BaseM> removeMs = new HashSet<BaseM>();

            foreach (var entry in NodesVMs)
            {
                var model = entry.Key;
                var viewModel = entry.Value;

                if (viewModel is INodeVM nodeViewModel)
                {
                    nodeRect.X = nodeViewModel.PositionX - absMaxHeight;
                    nodeRect.Y = nodeViewModel.PositionY - absMaxWidth;
                    nodeRect.Width = absMaxHeight * 2;
                    nodeRect.Height = absMaxWidth * 2;

                    (viewRect.IntersectsWith(nodeRect) ? keepMs : removeMs).Add(_modelExtractor(viewModel));
                }
            }

            foreach (var nodeModel in Model.nodes)
            {
                if (keepMs.Contains(nodeModel)) continue;

                if (removeMs.Contains(nodeModel)) continue;

                nodeRect.X = nodeModel.positionX - absMaxHeight;
                nodeRect.Y = nodeModel.positionY - absMaxWidth;
                nodeRect.Width = absMaxHeight * 2;
                nodeRect.Height = absMaxWidth * 2;

                if (viewRect.IntersectsWith(nodeRect)) addMs.Add(nodeModel);
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
                    ((INodeVM)viewModel).IsRoot = RootNodeIds.Contains(model.id);

                    NodesVMs.Add(model.id, viewModel);
                    Add(null, viewModel);
                }
            }

            foreach (var nodeEntry in NodesVMs) UpdateLocalPosition((INodeVM)nodeEntry.Value, ENodeVMUpdate.X | ENodeVMUpdate.Y);
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

                SelectionNode = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? (INodeVM)NodesVMs[selection.First()] : null;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        void RemoveFromSelection(INodeVM nodeViewModel)
        {
            if (nodeViewModel != null)
            {
                if (selection.Remove(nodeViewModel.Id))
                {
                    SelectionEditor = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? _editorCreator(NodesVMs[selection.First()], this) : null;

                    SelectionNode = selection.Count == 1 && NodesVMs.ContainsKey(selection.First()) ? (INodeVM)NodesVMs[selection.First()] : null;

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public override void GetSelection(IList outSelection)
        {
            foreach (var selectedViewModel in selection) outSelection.Add(selectedViewModel);
        }
        public override bool HasSelection() => selection.Count > 0;



        protected INodeVM selectionNode;
        public INodeVM SelectionNode
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



        protected virtual string CanLinkNodes(INodeVM from, INodeVM to) { return nameof(NotImplementedException); }
        protected virtual void PreLinkNodes(INodeVM from, INodeVM to) { }

        protected void ShowPreviewLink(INodeVM nodeViewModel)
        {
            if (!previewLinkIsAdded)
            {
                previewLink.FromX = nodeViewModel.PositionX;
                previewLink.FromY = nodeViewModel.PositionY;
                UpdateLinkLocalPosition(previewLink, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.Scale);

                previewLinkIsAdded = true;
                ItemsVMs.Add(previewLink);
            }
        }
        protected void HidePreviewLink()
        {
            if (previewLinkIsAdded)
            {
                previewLinkIsAdded = false;
                ItemsVMs.Remove(previewLink);
            }
        }

        protected void ShowSelectionBox(double positionX, double positionY)
        {
            if (!previewLinkIsAdded)
            {
                selectionBox.FromX = positionX;
                selectionBox.FromY = positionY;
                selectionBox.ToX = positionX;
                selectionBox.ToY = positionY;
                UpdateSelectionBoxLocalPosition(selectionBox, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY | ELinkVMUpdate.ToX | ELinkVMUpdate.ToY | ELinkVMUpdate.Scale);

                selectionBoxIsAdded = true;
                ItemsVMs.Add(selectionBox);
            }
        }
        protected void HideSelectionBox()
        {
            if (selectionBoxIsAdded)
            {
                selectionBoxIsAdded = false;
                ItemsVMs.Remove(selectionBox);
            }
        }

        protected void ProcessSelectionBox()
        {
            if (selectionBoxIsAdded)
            {
                HashSet<Notifier> selectionBoxCapture = new HashSet<Notifier>();

                Rect selectionRect = new Rect
                {
                    X = Math.Min(selectionBox.FromX, selectionBox.ToX),
                    Y = Math.Min(selectionBox.FromY, selectionBox.ToY)
                };
                selectionRect.Width = Math.Max(selectionBox.FromX, selectionBox.ToX) - selectionRect.X;
                selectionRect.Height = Math.Max(selectionBox.FromY, selectionBox.ToY) - selectionRect.Y;

                foreach (var entry in NodesVMs)
                {
                    var viewModel = entry.Value;

                    if (viewModel is INodeVM nodeViewModel)
                    {
                        nodeRect.X = nodeViewModel.PositionX - nodeViewModel.Width / 2;
                        nodeRect.Y = nodeViewModel.PositionY - nodeViewModel.Height / 2;
                        nodeRect.Width = nodeViewModel.Width;
                        nodeRect.Height = nodeViewModel.Height;

                        if (selectionRect.IntersectsWith(nodeRect)) selectionBoxCapture.Add(viewModel);
                    }
                }

                bool resetSelection = true;
                foreach (var viewModel in selectionBoxCapture)
                {
                    AddToSelection(viewModel, resetSelection);
                    resetSelection = false;
                }
            }
        }
    }
}