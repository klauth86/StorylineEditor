﻿/*
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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Config
{
    public static class LANGUAGE
    {
        public const byte UNSET = 0;

        public const byte ENG = 1;
        public const byte RUS = 2;
        public const byte UKR = 3;
        public const byte ESP = 4;
    }

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

        public const byte SCALE = 104;
        public const byte ALIGN_HOR = 105;
        public const byte ALIGN_VER = 106;

        public const byte COPY = 107;
        public const byte PASTE = 108;
        public const byte CUT = 109;
        public const byte DELETE = 110;
    }

    public class ConfigM
    {
        public static ConfigM Config { get; set; }

        public ConfigM()
        {
            Language = LANGUAGE.UNSET;
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
            
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.ALIGN_HOR, KeyboardButton = Key.H });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.ALIGN_VER, KeyboardButton = Key.V });

            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.COPY, KeyboardButton = Key.C, ModifierKeys = ModifierKeys.Control });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.PASTE, KeyboardButton = Key.V, ModifierKeys = ModifierKeys.Control });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.CUT, KeyboardButton = Key.X, ModifierKeys = ModifierKeys.Control });
            Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.DELETE, KeyboardButton = Key.Delete });

            Config.PlayRate = 100;
            Config.Duration = 4;
        }

        public static bool CompleteToDefaultConfig()
        {
            bool hasChanges = false;

            if (!Config.UserActions.Any((userAction) => userAction.UserActionType == USER_ACTION_TYPE.ALIGN_HOR))
            {
                Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.ALIGN_HOR, KeyboardButton = Key.H });
                hasChanges = true;
            }

            if (!Config.UserActions.Any((userAction) => userAction.UserActionType == USER_ACTION_TYPE.ALIGN_VER))
            {
                Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.ALIGN_VER, KeyboardButton = Key.V });
                hasChanges = true;
            }

            if (!Config.UserActions.Any((userAction) => userAction.UserActionType == USER_ACTION_TYPE.COPY))
            {
                Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.COPY, KeyboardButton = Key.C, ModifierKeys = ModifierKeys.Control });
            }

            if (!Config.UserActions.Any((userAction) => userAction.UserActionType == USER_ACTION_TYPE.PASTE))
            {
                Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.PASTE, KeyboardButton = Key.V, ModifierKeys = ModifierKeys.Control });
            }

            if (!Config.UserActions.Any((userAction) => userAction.UserActionType == USER_ACTION_TYPE.CUT))
            {
                Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.CUT, KeyboardButton = Key.X, ModifierKeys = ModifierKeys.Control });
            }

            if (!Config.UserActions.Any((userAction) => userAction.UserActionType == USER_ACTION_TYPE.DELETE))
            {
                Config.UserActions.Add(new UserActionM() { UserActionType = USER_ACTION_TYPE.DELETE, KeyboardButton = Key.Delete });
            }

            return hasChanges;
        }


        // LANGUAGE
        public byte Language { get; set; }

        // CANVAS COMMANDS
        public List<UserActionM> UserActions { get; set; }


        // DIALOG PLAYER
        public float PlayRate { get; set; }
        public float Duration { get; set; }
    }
}