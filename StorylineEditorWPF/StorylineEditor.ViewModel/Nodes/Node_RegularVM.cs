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
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel.Interface;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_RegularVM<T, U>
        : Node_InteractiveVM<T, U>
        , IRegularNode
        where T : Node_RegularM
        where U : class
    {
        public CollectionViewSource FilteredCharacterCVS { get; }

        public Node_RegularVM(T model, U parent) : base(model, parent)
        {
            Model.characterId = Model.characterId ?? CharacterM.PLAYER_ID;

            FilteredCharacterCVS = new CollectionViewSource() { Source = ActiveContext.Characters };

            if (FilteredCharacterCVS.View != null)
            {
                FilteredCharacterCVS.View.Filter = OnFilter;
                FilteredCharacterCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredCharacterCVS.View.MoveCurrentTo(Character);
            }
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(characterFilter) || model.PassFilter(characterFilter);
            }
            return false;
        }

        protected string characterFilter;
        public string CharacterFilter
        {
            set
            {
                if (value != characterFilter)
                {
                    characterFilter = value;
                    FilteredCharacterCVS.View?.Refresh();
                }
            }
        }

        public BaseM Character
        {
            get => ActiveContext.GetCharacter(Model.characterId);
            set
            {
                if (value?.id != Model.characterId)
                {
                    Model.characterId = value?.id;
                    OnModelChanged(Model, nameof(Character));
                    Notify(nameof(Character));

                    RefreshModelName();
                }
            }
        }

        public string OverrideName
        {
            get => string.IsNullOrWhiteSpace(Model.overrideName) ? null : Model.overrideName;
            set
            {
                if (Model.overrideName != value)
                {
                    Model.overrideName = value;
                    OnModelChanged(Model, nameof(OverrideName));
                }
            }
        }

        public byte FileStorageType
        {
            get => Model.fileStorageType;
            set
            {
                if (Model.fileStorageType != value)
                {
                    Model.fileStorageType = value;
                    OnModelChanged(Model, nameof(FileStorageType));
                }
            }
        }

        public string FileHttpRef
        {
            get => Model.fileHttpRef;
            set
            {
                if (Model.fileHttpRef != value)
                {
                    Model.fileHttpRef = value;
                    OnModelChanged(Model, nameof(FileHttpRef));
                }
            }
        }

        public string ShortDescription
        {
            get => Model.shortDescription;
            set
            {
                if (Model.shortDescription != value)
                {
                    Model.shortDescription = value;
                    OnModelChanged(Model, nameof(ShortDescription));
                }
            }
        }

        public override void SetRichText(string propName, ref TextRangeM textRangeModel)
        {
            base.SetRichText(propName, ref textRangeModel);

            RefreshModelName();
        }

        protected void RefreshModelName() { Name = string.Format("[{0}]: {1}", Character?.name ?? "???", Model.rtDescription.ToString()); } ////// TODO DUPLICATION
    }

    public class Node_ReplicaVM : Node_RegularVM<Node_ReplicaM, object>
    {
        public Node_ReplicaVM(Node_ReplicaM model, object parent) : base(model, parent) { }
    }

    public class Node_ReplicaEditorVM : Node_ReplicaVM
    {
        public Node_ReplicaEditorVM(Node_ReplicaVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }

    public class Node_DialogVM : Node_RegularVM<Node_DialogM, object>
    {
        public Node_DialogVM(Node_DialogM model, object parent) : base(model, parent) { }
    }

    public class Node_DialogEditorVM : Node_DialogVM
    {
        public Node_DialogEditorVM(Node_DialogVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }
}