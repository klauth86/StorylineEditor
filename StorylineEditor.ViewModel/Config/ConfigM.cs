using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Config
{
    public static class USER_ACTION_MODE
    {
        public const byte UNSET = 0;
        public const byte CREATE_NODE = 1;
        public const byte DUPLICATE_NODE = 2;
        public const byte DRAG_AND_SCROLL = 3;
        public const byte LINK = 4;
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

            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_MODE.CREATE_NODE, MouseButton = MouseButton.Left });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_MODE.LINK, MouseButton = MouseButton.Left });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_MODE.DUPLICATE_NODE, MouseButton = MouseButton.Right, ModifierKeys = ModifierKeys.Shift });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_MODE.DRAG_AND_SCROLL, MouseButton = MouseButton.Right });
        }

        public List<UserActionM> UserActions { get; set; }
    }
}