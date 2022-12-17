﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

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
                    CallbackContext?.Callback(this, nameof(MouseButton));
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

                CallbackContext?.Callback(this, nameof(IsAlt));
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

                CallbackContext?.Callback(this, nameof(IsControl));
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

                CallbackContext?.Callback(this, nameof(IsShift));
                Notify(nameof(IsShift));
            }
        }

        public override string Title => null;
        public override string Stats => null;
    }
}