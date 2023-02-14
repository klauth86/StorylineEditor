/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Config;
using System.Collections.Generic;

namespace StorylineEditor.App.Config
{
    public class ConfigVM : SimpleVM<ConfigM, object>
    {
        public ConfigVM(ConfigM model) : base(model, null)
        {
            UserActions = new List<UserActionVM>();
            
            foreach (var userAction in Model.UserActions)
            {
                UserActions.Add(new UserActionVM(userAction));
            }
        }

        public List<UserActionVM> UserActions { get; }

        public override string Id => throw new System.NotImplementedException();
    }
}