/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using System.Windows.Input;

namespace StorylineEditor.App.Config
{
    public class UserActionVM : SimpleVM<UserActionM, object>
    {
        public UserActionVM(UserActionM model) : base(model, null) { }

        public MouseButton MouseButton
        {
            get => Model.MouseButton;
            set
            {
                if (value != Model.MouseButton)
                {
                    Model.MouseButton = value;
                    Notify(nameof(MouseButton));

                    ActiveContext.FileService.SaveConfig();
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

                ActiveContext.FileService.SaveConfig();
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

                ActiveContext.FileService.SaveConfig();
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

                ActiveContext.FileService.SaveConfig();
            }
        }

        public override string Id => throw new System.NotImplementedException();
    }
}