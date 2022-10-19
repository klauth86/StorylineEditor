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
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
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
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ?? (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type));

        protected ICommand addNodeCommand;
        public ICommand AddNodeCommand => addNodeCommand ?? (addNodeCommand = new RelayCommand<UIElement>((uiElement) =>
        {
            Point position = Mouse.GetPosition(uiElement);
            FromLocalToAbsolute(position);
            AddCommandInternal(SelectedNodeType, position, this);
        }, (uiElement) => uiElement != null && SelectedNodeType != null));



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
            double result = x /= scaleX;    // Scale
            result += offsetX;              // Transaltion
            return result;
        }
        protected double FromLocalToAbsoluteY(double y)
        {
            double result = y /= scaleY;    // Scale
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
                    nodeViewModel.LocalPositionX = FromAbsoluteToLocalX(nodeViewModel.PositionX);
                }
                else if (propName == nameof(Node_BaseVM<Node_BaseM>.PositionY))
                {
                    nodeViewModel.LocalPositionY = FromAbsoluteToLocalY(nodeViewModel.PositionY);
                }
            }
        }
    }
}