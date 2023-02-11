/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_BaseVM<T, U>
        : BaseVM<T, U>
        , INode
        , IRichTextSource
        where T : Node_BaseM
        where U : class
    {
        public Node_BaseVM(
            T model
            , U parent
            )
            : base(
                  model
                  , parent
                  )
        {
            zIndex = 100;
        }

        public byte Gender
        {
            get => Model.gender;
            set
            {
                if (Model.gender != value)
                {
                    Model.gender = value;
                    OnModelChanged(Model, nameof(Gender));

                    ActiveContext.ActiveGraph.OnNodeGenderChanged(this);
                }
            }
        }

        public double PositionX
        {
            get => Model.positionX;
            set
            {
                if (Model.positionX != value)
                {
                    Model.positionX = value;
                    OnModelChanged(Model, nameof(PositionX));

                    ActiveContext.ActiveGraph.OnNodePositionChanged(this, ENodeUpdateFlags.X);
                }
            }
        }

        public double PositionY
        {
            get => Model.positionY;
            set
            {
                if (Model.positionY != value)
                {
                    Model.positionY = value;
                    OnModelChanged(Model, nameof(PositionY));

                    ActiveContext.ActiveGraph.OnNodePositionChanged(this, ENodeUpdateFlags.Y);
                }
            }
        }

        public string CharacterId => null;

        protected double width;
        public double Width
        {
            get => width;
            set
            {
                if (width != value)
                {
                    width = value;

                    ActiveContext.ActiveGraph.OnNodeSizeChanged(this, ENodeUpdateFlags.X);
                }
            }
        }

        protected double height;
        public double Height
        {
            get => height;
            set
            {
                if (height != value)
                {
                    height = value;

                    ActiveContext.ActiveGraph.OnNodeSizeChanged(this, ENodeUpdateFlags.Y);
                }
            }
        }

        protected double left;
        public double Left
        {
            get => left;
            set
            {
                if (left != value)
                {
                    left = value;
                    Notify(nameof(Left));
                }
            }
        }

        protected double top;
        public double Top
        {
            get => top;
            set
            {
                if (top != value)
                {
                    top = value;
                    Notify(nameof(Top));
                }
            }
        }

        private ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand(() => { Gender = (byte)((Gender + 1) % 3); }));

        protected int zIndex;
        public int ZIndex => zIndex;

        public virtual void OnRichTextChanged(string propName, string richTextModelString, string textString)
        {
            Description = richTextModelString;
        }

        public virtual IEnumerable<IPredicate> Predicates { get => Enumerable.Empty<IPredicate>(); }
        public virtual IEnumerable<IGameEvent> GameEvents { get => Enumerable.Empty<IGameEvent>(); }
    }
}