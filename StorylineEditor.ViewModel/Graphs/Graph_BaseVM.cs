﻿/*
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

            NodesVMs = new Dictionary<BaseM, Notifier>();
            LinksVMs = new Dictionary<BaseM, Notifier>();
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ?? (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type));

        protected ICommand addNodeCommand;
        public ICommand AddNodeCommand => addNodeCommand ?? (addNodeCommand = new RelayCommand<UIElement>((uiElement) =>
        {
            Point position = Mouse.GetPosition(uiElement);
            
            FromLocalToAbsolute(position);

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
        protected Point prevDragPosition;

        protected ICommand startDragCommand;
        public ICommand StartDragCommand => startDragCommand ?? (startDragCommand = new RelayCommand<MouseButtonEventArgs>((eventArgs) =>
        {
            prevDragPosition = eventArgs.GetPosition((IInputElement)eventArgs.OriginalSource);
            isDragging = true;

            CommandManager.InvalidateRequerySuggested();
        }, (eventArgs) => eventArgs != null && !isDragging));

        protected ICommand endDragCommand;
        public ICommand EndDragCommand => endDragCommand ?? (endDragCommand = new RelayCommand<MouseButtonEventArgs>((eventArgs) =>
        {
            isDragging = false;

            CommandManager.InvalidateRequerySuggested();
        }, (eventArgs) => eventArgs != null && isDragging));

        protected ICommand moveCommand;
        public ICommand MoveCommand => moveCommand ?? (moveCommand = new RelayCommand<MouseEventArgs>((eventArgs) =>
        {
            if (eventArgs.OriginalSource is UIElement uiElement)
            {
                Point dragPosition = eventArgs.GetPosition(uiElement);

                TranslateView(dragPosition.X - prevDragPosition.X, dragPosition.Y - prevDragPosition.Y, uiElement.RenderSize.Width, uiElement.RenderSize.Height);

                prevDragPosition = dragPosition;
            }
        }, (eventArgs) => eventArgs != null && isDragging));

        protected ICommand initCommand;
        public ICommand InitCommand => initCommand ?? (initCommand = new RelayCommand<RoutedEventArgs>((eventArgs) =>
        {
            if (eventArgs.OriginalSource is UIElement uiElement)
            {
                TranslateView(OffsetX, offsetY, uiElement.RenderSize.Width, uiElement.RenderSize.Height);
            }
        }, (eventArgs) => eventArgs != null));



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



        private Dictionary<BaseM, Notifier> NodesVMs = new Dictionary<BaseM, Notifier>();
        private Dictionary<BaseM, Notifier> LinksVMs = new Dictionary<BaseM, Notifier>();



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
            double result = x / scaleX;    // Scale
            result += offsetX;              // Transaltion
            return result;
        }
        protected double FromLocalToAbsoluteY(double y)
        {
            double result = y / scaleY;    // Scale
            result += offsetY;              // Transaltion
            return result;
        }

        protected double FromAbsoluteToLocalX(double x)
        {
            double result = x - offsetX;    // Transaltion
            result *= scaleX;               // Scale
            return result;
        }
        protected double FromAbsoluteToLocalY(double y)
        {
            double result = y - offsetY;    // Transaltion
            result *= scaleY;               // Scale
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

        private void TranslateView(double deltaX, double deltaY, double sizeX, double sizeY)
        {
            OffsetX -= deltaX / scaleX;
            OffsetY -= deltaY / scaleX;

            double absSizeX = sizeX / scaleX;
            double absSizeY = sizeY / scaleY;

            Rect viewRect = new Rect(OffsetX, OffsetY, absSizeX, absSizeY);
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
    }
}