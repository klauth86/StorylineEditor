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

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.GameEvents
{
    public abstract class GE_BaseVM<T, U>
        : BaseVM<T, U>
        , IGameEvent
        where T : GE_BaseM
        where U : class
    {
        public GE_BaseVM(T model, U parent) : base(model, parent) { }

        public Type GameEventType => Model?.GetType();

        public bool IsExecutedOnLeave
        {
            get => Model.executionMode == EXECUTION_MODE.ON_LEAVE;
            set
            {
                if (value != IsExecutedOnLeave)
                {
                    Model.executionMode = value ? EXECUTION_MODE.ON_LEAVE : EXECUTION_MODE.ON_ENTER;
                    Notify(nameof(IsExecutedOnLeave));
                }
            }
        }

        public abstract void Execute();
    }
}