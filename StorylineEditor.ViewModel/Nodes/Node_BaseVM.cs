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
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Nodes
{
    public interface INodeVM
    {
        double PositionX { get; set; }
        double PositionY { get; set; }
        double LocalPositionX { get; set; }
        double LocalPositionY { get; set; }
    }

    public abstract class Node_BaseVM<T> : BaseVM<T>, INodeVM where T : Node_BaseM
    {
        public Node_BaseVM(T model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            CallbackContext?.Callback(this, nameof(PositionX));
            CallbackContext?.Callback(this, nameof(PositionY));
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
                    CallbackContext?.Callback(this, nameof(PositionX));
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
                    CallbackContext?.Callback(this, nameof(PositionY));
                }
            }
        }



        double localPositionX;
        public double LocalPositionX
        {
            get => localPositionX;
            set
            {
                if (localPositionX != value)
                {
                    localPositionX = value;
                    Notify(nameof(LocalPositionX));
                }
            }
        }

        double localPositionY;
        public double LocalPositionY
        {
            get => localPositionY;
            set
            {
                if (localPositionY != value)
                {
                    localPositionY = value;
                    Notify(nameof(LocalPositionY));
                }
            }
        }



        private ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand(() => { Gender = (byte)((Gender + 1) % 3); }));
    }
}