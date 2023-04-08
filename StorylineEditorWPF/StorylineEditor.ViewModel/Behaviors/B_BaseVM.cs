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

using StorylineEditor.Model.Behaviors;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Behaviors
{
    public abstract class B_BaseVM<T, U>
        : BaseVM<T, U>
        , IBehavior
        where T : B_BaseM
        where U : class
    {
        public B_BaseVM(T model, U parent) : base(model, parent) { }

        public Type BehaviorType => Model?.GetType();
    }
}