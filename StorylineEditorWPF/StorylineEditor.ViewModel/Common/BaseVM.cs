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

using StorylineEditor.Model;
using StorylineEditor.ViewModel.Interface;

namespace StorylineEditor.ViewModel.Common
{
    public abstract class BaseVM<T, U> : SimpleVM<T, U>
        where T : BaseM
        where U : class
    {
        public BaseVM(T model, U parent) : base(model, parent) { }

        public override string Id => Model?.id;

        public string Name
        {
            get => Model.name;
            set
            {
                if (Model.name != value)
                {
                    Model.name = value;
                    OnModelChanged(Model, nameof(Name));

                    if (ActiveContext.ActiveTab is ICollection_Base collectionBase)
                    {
                        collectionBase.Refresh();
                    }
                }
            }
        }

        public string Description
        {
            get => Model.description;
            set
            {
                if (Model.description != value)
                {
                    Model.description = value;
                    
                    OnModelChanged(Model, nameof(Description));
                }
            }
        }
    }
}