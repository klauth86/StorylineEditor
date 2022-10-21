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
    enum EUpdateTarget
    {
        None,
        X,
        Y,
        Both
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

            isDragging = false;
            draggedNodeViewModel = null;

            NodesVMs = new Dictionary<BaseM, Notifier>();
            LinksVMs = new Dictionary<BaseM, Notifier>();
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ?? (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type));

        protected ICommand addNodeCommand;
        public ICommand AddNodeCommand => addNodeCommand ?? (addNodeCommand = new RelayCommand<UIElement>((uiElement) =>
        {
            Point position = Mouse.GetPosition(uiElement);

            position.X /= ScaleX;
            position.Y /= ScaleY;

            position.X += OffsetX;
            position.Y += OffsetY;

            BaseM model = _modelCreator(SelectedNodeType, position);
            Notifier viewModel = _viewModelCreator(model, this);

            Add(model, null);
            ((viewModel is INodeVM) ? NodesVMs : LinksVMs).Add(model, viewModel);
            Add(null, viewModel);

            Selection = viewModel;

            CommandManager.InvalidateRequerySuggested();
        }, (uiElement) => uiElement != null && SelectedNodeType != null));


        protected bool isDragging;
        protected INodeVM draggedNodeViewModel;
        protected Point prevPosition;

        protected ICommand startDragCommand;
        public ICommand StartDragCommand => startDragCommand ?? (startDragCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            if (args.Source is FrameworkElement frameworkElement)
            {
                prevPosition = args.GetPosition(null);
                draggedNodeViewModel = frameworkElement.DataContext as INodeVM;
                isDragging = true;

                CommandManager.InvalidateRequerySuggested();
            }
        }, (args) => args != null && !isDragging));

        protected ICommand endDragCommand;
        public ICommand EndDragCommand => endDragCommand ?? (endDragCommand = new RelayCommand<MouseButtonEventArgs>((args) =>
        {
            isDragging = false;
            draggedNodeViewModel = null;

            CommandManager.InvalidateRequerySuggested();
        }, (args) => args != null && isDragging));

        protected ICommand moveCommand;
        public ICommand MoveCommand => moveCommand ?? (moveCommand = new RelayCommand<MouseEventArgs>((args) =>
        {
            Point position = args.GetPosition(null);

            if (draggedNodeViewModel != null)
            {
                draggedNodeViewModel.PositionX += (position.X - prevPosition.X) / ScaleX;
                draggedNodeViewModel.PositionY += (position.Y - prevPosition.Y) / ScaleY;
            }
            else
            {
                TranslateView((position.X - prevPosition.X) / ScaleX, (position.Y - prevPosition.Y) / ScaleY);
            }

            prevPosition = position;
        }, (args) => args != null && isDragging));

        protected ICommand initCommand;
        public ICommand InitCommand => initCommand ?? (initCommand = new RelayCommand<RoutedEventArgs>((args) => TranslateView(0, 0), (args) => args != null));

        protected ICommand scaleCommand;
        public ICommand ScaleCommand => scaleCommand ?? (scaleCommand = new RelayCommand<MouseWheelEventArgs>((args) =>
        {
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
        }, (args) => args != null));



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



        private readonly Dictionary<BaseM, Notifier> NodesVMs;
        private readonly Dictionary<BaseM, Notifier> LinksVMs;



        protected void FromLocalToAbsolute(Point point)
        {
            if (point != null)
            {
                point.X = FromLocalToAbsoluteX(point.X);
                point.Y = FromLocalToAbsoluteY(point.Y);
            }
        }

        protected double FromLocalToAbsoluteX(double x)
        {
            double result = x / ScaleX;    // Scale
            result += OffsetX;              // Transaltion
            return result;
        }
        protected double FromLocalToAbsoluteY(double y)
        {
            double result = y / ScaleY;    // Scale
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
                if (propName == nameof(Node_BaseVM<Node_BaseM>.PositionX))
                {
                    UpdateLocalPosition(nodeViewModel, EUpdateTarget.X);
                }
                else if (propName == nameof(Node_BaseVM<Node_BaseM>.PositionY))
                {
                    UpdateLocalPosition(nodeViewModel, EUpdateTarget.Y);
                }
            }
        }

        private void UpdateLocalPosition(INodeVM nodeViewModel, EUpdateTarget updateTarget)
        {
            switch (updateTarget)
            {
                case EUpdateTarget.None:
                    break;
                case EUpdateTarget.X:
                    nodeViewModel.Left = FromAbsoluteToLocalX(nodeViewModel.PositionX) - nodeViewModel.Width / 2;
                    break;
                case EUpdateTarget.Y:
                    nodeViewModel.Top = FromAbsoluteToLocalY(nodeViewModel.PositionY) - nodeViewModel.Height / 2;
                    break;
                case EUpdateTarget.Both:
                    nodeViewModel.Left = FromAbsoluteToLocalX(nodeViewModel.PositionX) - nodeViewModel.Width / 2;
                    nodeViewModel.Top = FromAbsoluteToLocalY(nodeViewModel.PositionY) - nodeViewModel.Height / 2;
                    break;
                default:
                    break;
            }
        }

        private void TranslateView(double absoluteDeltaX, double absoluteDeltaY)
        {
            OffsetX -= absoluteDeltaX;
            OffsetY -= absoluteDeltaY;

            Rect viewRect = new Rect(OffsetX, OffsetY, ViewWidth / ScaleX, ViewHeight / ScaleY);
            Rect nodeRect = new Rect();

            double doubleMaxHeight = 2 * (double)Application.Current.FindResource("Double_Node_MaxHeight");
            double doubleMaxWidth = 2 * (double)Application.Current.FindResource("Double_Node_MaxWidth");

            HashSet<BaseM> keepMs = new HashSet<BaseM>();
            HashSet<BaseM> addMs = new HashSet<BaseM>();
            HashSet<BaseM> removeMs = new HashSet<BaseM>();

            foreach (var entry in NodesVMs)
            {
                var model = entry.Key;
                var viewModel = entry.Value;

                if (viewModel is INodeVM nodeViewModel)
                {
                    nodeRect.X = nodeViewModel.PositionX - doubleMaxHeight / 2;
                    nodeRect.Y = nodeViewModel.PositionY - doubleMaxWidth / 2;
                    nodeRect.Width = doubleMaxHeight;
                    nodeRect.Height = doubleMaxWidth;

                    (viewRect.IntersectsWith(nodeRect) ? keepMs : removeMs).Add(_modelExtractor(viewModel));
                }
            }

            foreach (var nodeModel in Model.nodes)
            {
                if (keepMs.Contains(nodeModel)) continue;

                if (removeMs.Contains(nodeModel)) continue;

                nodeRect.X = nodeModel.positionX - doubleMaxHeight / 2;
                nodeRect.Y = nodeModel.positionY - doubleMaxWidth / 2;
                nodeRect.Width = doubleMaxHeight;
                nodeRect.Height = doubleMaxWidth;

                if (viewRect.IntersectsWith(nodeRect)) addMs.Add(nodeModel);
            }

            foreach (var model in removeMs) { if (NodesVMs.ContainsKey(model)) { ItemsVMs.Remove(NodesVMs[model]); NodesVMs.Remove(model); } }

            foreach (var model in addMs) { if (!NodesVMs.ContainsKey(model)) { NodesVMs.Add(model, _viewModelCreator(model, this)); ItemsVMs.Add(NodesVMs[model]); } }

            foreach (var viewModel in ItemsVMs)
            {
                if (viewModel is INodeVM nodeViewModel) UpdateLocalPosition(nodeViewModel, EUpdateTarget.Both);
            }
        }



        public double ViewWidth { get; set; }
        public double ViewHeight { get; set; }
    }
}