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
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
    public enum ELinkVMUpdate : byte
    {
        FromX = 1,
        FromY = 2,
        ToX = 4,
        ToY = 8
    }

    enum ENodeVMUpdate
    {
        X = 1,
        Y = 2,
    }

    public class Graph_BaseVM<T> : Collection_BaseVM<T, Point>, ICallbackContext where T : GraphM
    {
        public Graph_BaseVM(T model, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
            Func<Notifier, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor, Type defaultNodeType, Func<Type, string> typeDescriptor) : base(model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, modelExtractor)
        {
            offsetY = offsetX = 0;
            scaleY = scaleX = 1;
            selectedNodeType = defaultNodeType;
            _typeDescriptor = typeDescriptor ?? throw new ArgumentNullException(nameof(typeDescriptor));

            fromNodeViewModel = null;
            isDragging = false;
            draggedNodeViewModel = null;

            previewLinkIsAdded = false;
            previewLink = new LinkVM(new LinkM(), this);

            viewRect = new Rect();
            nodeRect = new Rect();
            absMaxHeight = (double)Application.Current.FindResource("Double_Node_MaxHeight");
            absMaxWidth = (double)Application.Current.FindResource("Double_Node_MaxWidth");

            NodesVMs = new Dictionary<string, Notifier>();
            LinksVMs = new Dictionary<string, LinkVM>();

            selection = new List<Notifier>();
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ?? (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type));

        protected ICommand addCommand;
        public override ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (isDragging) return;

            if ((args.OriginalSource as FrameworkElement)?.DataContext is INodeVM nodeViewModel)
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
                    Notifier viewModel = _viewModelCreator(model, this);

                    Add(model, null);
                    NodesVMs.Add(model.id, viewModel);
                    Add(null, viewModel);

                    bool resetSelection = !Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
                    AddToSelection(viewModel, resetSelection);

                    if (resetSelection) fromNodeViewModel = viewModel as INodeVM;
                }
            }
        }));

        protected ICommand selectCommand;
        public override ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((viewModel) => { }));

        protected ICommand linkCommand;
        public ICommand LinkCommand => linkCommand ?? (linkCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (fromNodeViewModel != null)
            {
                if ((args.OriginalSource as FrameworkElement)?.DataContext is INodeVM toNodeViewModel)
                {
                    string message = CanLinkNodes(fromNodeViewModel, toNodeViewModel);
                    if (message == null)
                    {
                        LinkM model = (LinkM)_modelCreator(typeof(LinkM), new Point());
                        model.fromNodeId = fromNodeViewModel.Id;
                        model.toNodeId = toNodeViewModel.Id;
                        
                        LinkVM viewModel = (LinkVM)_viewModelCreator(model, this);

                        Add(model, null);
                        LinksVMs.Add(model.id, viewModel);
                        Add(null, viewModel);
                    }
                }

                HidePreviewLink();

                fromNodeViewModel = null;
            }
        }));



        protected INodeVM fromNodeViewModel;
        protected bool isDragging;
        protected INodeVM draggedNodeViewModel;
        protected Point prevPosition;
        protected bool previewLinkIsAdded;
        protected LinkVM previewLink;
        protected Rect viewRect;
        protected Rect nodeRect;
        protected readonly double absMaxHeight;
        protected readonly double absMaxWidth;

        protected ICommand dragCommand;
        public ICommand DragCommand => dragCommand ?? (dragCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (fromNodeViewModel != null) return;

            prevPosition = args.GetPosition(null);

            draggedNodeViewModel = (args.OriginalSource as FrameworkElement)?.DataContext as INodeVM;
            isDragging = true;

            CommandManager.InvalidateRequerySuggested();
        }));

        protected ICommand moveCommand;
        public ICommand MoveCommand => moveCommand ?? (moveCommand = new RelayCommand<MouseEventArgs>((args) =>
        {
            if (isDragging)
            {
                Point position = args.GetPosition(null);

                if (Mouse.RightButton != MouseButtonState.Pressed)
                {
                    isDragging = false;
                    draggedNodeViewModel = null;
                }
                else
                {
                    double deltaX = (position.X - prevPosition.X) / ScaleX;
                    double deltaY = (position.Y - prevPosition.Y) / ScaleY;

                    if (draggedNodeViewModel != null)
                    {
                        if (!draggedNodeViewModel.IsSelected)
                        {
                            draggedNodeViewModel.PositionX += deltaX;
                            draggedNodeViewModel.PositionY += deltaY;
                        }

                        foreach (INodeVM nodeViewModel in selection)
                        {
                            nodeViewModel.PositionX += deltaX;
                            nodeViewModel.PositionY += deltaY;
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
                            UpdateLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY);

                            previewLink.Description = CanLinkNodes(fromNodeViewModel, toNodeViewModel);

                            ShowPreviewLink(fromNodeViewModel);
                        }
                    }
                    else
                    {
                        Point position = args.GetPosition(args.Source as UIElement);

                        previewLink.ToX = FromLocalToAbsoluteX(position.X);
                        previewLink.ToY = FromLocalToAbsoluteY(position.Y);
                        UpdateLocalPosition(previewLink, ELinkVMUpdate.ToX | ELinkVMUpdate.ToY);

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
            if (fromNodeViewModel != null) return;

            if (isDragging) return;

            if (args.Source is UIElement uiElement)
            {
                Point position = args.GetPosition(uiElement);

                double oldX = position.X / ScaleX;
                double oldY = position.Y / ScaleY;

                ScaleX = Math.Max(Math.Min(ScaleX + args.Delta * 0.0002, 4), 1.0 / 64);
                ScaleY = Math.Max(Math.Min(ScaleY + args.Delta * 0.0002, 4), 1.0 / 64);

                double newX = position.X / ScaleX;
                double newY = position.Y / ScaleY;

                TranslateView(newX - oldX, newY - oldY);
            }
        }));



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

        protected double scaleX;
        public double ScaleX
        {
            get => scaleX;
            set
            {
                if (scaleX != value)
                {
                    scaleX = value;
                    Notify(nameof(ScaleX));
                }
            }
        }

        protected double scaleY;
        public double ScaleY
        {
            get => scaleY;
            set
            {
                if (scaleY != value)
                {
                    scaleY = value;
                    Notify(nameof(ScaleY));
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

        public override IList GetContext(BaseM model) { if (model is LinkM) return Model.links; return Model.nodes; }



        private readonly Dictionary<string, Notifier> NodesVMs;
        private readonly Dictionary<string, LinkVM> LinksVMs;



        protected double FromLocalToAbsoluteX(double x)
        {
            double result = x / ScaleX;     // Scale
            result += OffsetX;              // Transaltion
            return result;
        }
        protected double FromLocalToAbsoluteY(double y)
        {
            double result = y / ScaleY;     // Scale
            result += OffsetY;              // Transaltion
            return result;
        }

        protected double FromAbsoluteToLocalX(double x)
        {
            double result = x - OffsetX;    // Transaltion
            result *= ScaleX;               // Scale
            return result;
        }
        protected double FromAbsoluteToLocalY(double y)
        {
            double result = y - OffsetY;    // Transaltion
            result *= ScaleY;               // Scale
            return result;
        }



        public void Callback(object viewModelObj, string propName)
        {
            if (viewModelObj is INodeVM nodeViewModel)
            {
                if (propName == nameof(INodeVM.PositionX))
                {
                    UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.X);
                }
                else if (propName == nameof(INodeVM.PositionY))
                {
                    UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.Y);
                }
            }
        }
        private void UpdateLocalPosition(INodeVM nodeViewModel, ENodeVMUpdate updateTarget)
        {
            if ((updateTarget & ENodeVMUpdate.X) > 0) nodeViewModel.Left = FromAbsoluteToLocalX(nodeViewModel.PositionX) - nodeViewModel.Width / 2;
            if ((updateTarget & ENodeVMUpdate.Y) > 0) nodeViewModel.Top = FromAbsoluteToLocalY(nodeViewModel.PositionY) - nodeViewModel.Height / 2;
        }

        private void UpdateLocalPosition(ILinkVM linkViewModel, ELinkVMUpdate updateTarget)
        {
            if ((updateTarget & ELinkVMUpdate.FromX) > 0) linkViewModel.Left = FromAbsoluteToLocalX(linkViewModel.FromX);
            if ((updateTarget & ELinkVMUpdate.FromY) > 0) linkViewModel.Top = FromAbsoluteToLocalY(linkViewModel.FromY);
            if ((updateTarget & ELinkVMUpdate.ToX) > 0) linkViewModel.HandleX = FromAbsoluteToLocalX(linkViewModel.ToX) - linkViewModel.Left;
            if ((updateTarget & ELinkVMUpdate.ToY) > 0) linkViewModel.HandleY = FromAbsoluteToLocalY(linkViewModel.ToY) - linkViewModel.Top;

            linkViewModel.RefreshStepPoints();
        }



        private void TranslateView(double absoluteDeltaX, double absoluteDeltaY)
        {
            OffsetX -= absoluteDeltaX;
            OffsetY -= absoluteDeltaY;

            viewRect.X = OffsetX;
            viewRect.Y = OffsetY;
            viewRect.Width = ViewWidth / ScaleX;
            viewRect.Height = ViewHeight / ScaleY;

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

            foreach (var model in removeMs) { ItemsVMs.Remove(NodesVMs[model.id]); NodesVMs.Remove(model.id); }

            foreach (var model in addMs) { if (!NodesVMs.ContainsKey(model.id)) { NodesVMs.Add(model.id, _viewModelCreator(model, this)); ItemsVMs.Add(NodesVMs[model.id]); } }

            foreach (INodeVM nodeViewModel in ItemsVMs)
            {
                UpdateLocalPosition(nodeViewModel, ENodeVMUpdate.X | ENodeVMUpdate.Y);
            }
        }



        public double ViewWidth { get; set; }
        public double ViewHeight { get; set; }
        public bool SizeChangedFlag { set => TranslateView(0, 0); }



        protected readonly List<Notifier> selection;
        public override void AddToSelection(Notifier viewModel, bool resetSelection)
        {
            if (resetSelection)
            {
                foreach (var selectedVewModel in selection) selectedVewModel.IsSelected = false;
                selection.Clear();
            }

            if (!selection.Contains(viewModel) && viewModel != null)
            {
                selection.Add(viewModel);
                viewModel.IsSelected = true;
            }

            selectionEditor = selection.Count == 1 ? _editorCreator(selection[0]) : null;

            CommandManager.InvalidateRequerySuggested();
        }
        public override void GetSelection(IList outSelection)
        {
            foreach (var selectedViewModel in selection) outSelection.Add(selectedViewModel);
        }
        public override bool HasSelection() => selection.Count > 0;



        protected virtual string CanLinkNodes(INodeVM from, INodeVM to) { return nameof(NotImplementedException); }
        protected void ShowPreviewLink(INodeVM nodeViewModel)
        {
            if (!previewLinkIsAdded)
            {
                previewLink.FromX = nodeViewModel.PositionX;
                previewLink.FromY = nodeViewModel.PositionY;
                UpdateLocalPosition(previewLink, ELinkVMUpdate.FromX | ELinkVMUpdate.FromY);

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
    }
}