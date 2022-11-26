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
        }

        public List<UserActionM> UserActions { get; set; }
    }
}