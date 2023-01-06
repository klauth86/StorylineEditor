/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using System.IO;
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

                    SaveConfig();
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

                SaveConfig();
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

                SaveConfig();
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

                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            using (var fileStream = ServiceFacade.FileService.OpenFile(ServiceFacade.ConfigXmlPath, FileMode.Create))
            {
                SerializeService.Serialize(fileStream, ConfigM.Config);
            }
        }

        public override string Id => throw new System.NotImplementedException();
    }
}