using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using System.Windows.Input;

namespace StorylineEditor.App.Config
{
    public class UserActionVM : SimpleVM<UserActionM>
    {
        public UserActionVM(UserActionM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        public override string Id => throw new System.NotImplementedException();

        public MouseButton MouseButton
        {
            get => Model.MouseButton;
            set
            {
                if (value != Model.MouseButton)
                {
                    Model.MouseButton = value;
                    Notify(nameof(MouseButton));
                }
            }
        }

        public bool IsAlt
        {
            get => Model.ModifierKeys.HasFlag(ModifierKeys.Alt);
            set
            {
                if (value)
                {
                    Model.ModifierKeys |= ModifierKeys.Alt;
                }
                else
                {
                    Model.ModifierKeys &= ~ModifierKeys.Alt;
                }

                Notify(nameof(IsAlt));
            }
        }

        public bool IsControl
        {
            get => Model.ModifierKeys.HasFlag(ModifierKeys.Control);
            set
            {
                if (value)
                {
                    Model.ModifierKeys |= ModifierKeys.Control;
                }
                else
                {
                    Model.ModifierKeys &= ~ModifierKeys.Control;
                }

                Notify(nameof(IsControl));
            }
        }

        public bool IsShift
        {
            get => Model.ModifierKeys.HasFlag(ModifierKeys.Shift);
            set
            {
                if (value)
                {
                    Model.ModifierKeys |= ModifierKeys.Shift;
                }
                else
                {
                    Model.ModifierKeys &= ~ModifierKeys.Shift;
                }

                Notify(nameof(IsShift));
            }
        }
    }
}