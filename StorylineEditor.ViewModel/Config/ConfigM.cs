/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Config
{
    public static class USER_ACTION_TYPE
    {
        public const byte UNSET = 0;

        public const byte CREATE_NODE = 1;
        public const byte DUPLICATE_NODE = 2;
        public const byte LINK = 3;

        public const byte DRAG_AND_SCROLL = 50;
        
        public const byte SELECTION_SIMPLE = 101;
        public const byte SELECTION_ADDITIVE = 102;
        public const byte SELECTION_BOX = 103;
    }

    public class ConfigM
    {
        public static ConfigM Config { get; set; }

        public ConfigM()
        {
            UserActions = new List<UserActionM>();
        }

        public static void InitDefaultConfig()
        {
            Config = new ConfigM();

            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.CREATE_NODE, MouseButton = MouseButton.Left });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.DUPLICATE_NODE, MouseButton = MouseButton.Right, ModifierKeys = ModifierKeys.Shift });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.LINK, MouseButton = MouseButton.Right });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.DRAG_AND_SCROLL, MouseButton = MouseButton.Right, ModifierKeys = ModifierKeys.Control });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.SELECTION_SIMPLE, MouseButton = MouseButton.Left });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.SELECTION_ADDITIVE, MouseButton = MouseButton.Left, ModifierKeys = ModifierKeys.Control });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.SELECTION_BOX, MouseButton = MouseButton.Left, ModifierKeys = ModifierKeys.Shift });

            Config.PlayRate = 100;
            Config.Duration = 4;
        }


        // CANVAS COMMANDS
        public List<UserActionM> UserActions { get; set; }


        // DIALOG PLAYER
        public float PlayRate { get; set; }
        public float Duration { get; set; }
    }
}