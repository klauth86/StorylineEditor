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
using StorylineEditor.ViewModel.Common;

namespace StorylineEditor.ViewModel
{
    public class FolderVM : BaseVM<FolderM, object>
    {
        public FolderVM(FolderM model, object parent) : base(model, parent) { }

        public override bool IsFolder => true;

        protected bool isDragOver;
        public bool IsDragOver
        {
            get => isDragOver;
            set
            {
                if (value != isDragOver)
                {
                    isDragOver = value;
                    Notify(nameof(IsDragOver));
                }
            }
        }
    }
}