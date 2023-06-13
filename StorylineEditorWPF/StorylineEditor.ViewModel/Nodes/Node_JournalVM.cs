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

using StorylineEditor.Model.Nodes;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_JournalVM<T, U>
        : Node_BaseVM<T, U>
        where T : Node_JournalM
        where U : class
    {
        public Node_JournalVM(
            T model
            , U parent
            )
            : base(
                  model
                  , parent
                  ) { }

        public string Result
        {
            get => Model.result;
            set
            {
                if (Model.result != value)
                {
                    Model.result = value;
                    OnModelChanged(Model, nameof(Result));
                }
            }
        }
    }

    public class Node_Journal_StepVM : Node_JournalVM<Node_StepM, object>
    {
        public Node_Journal_StepVM(Node_StepM model, object parent) : base(model, parent) { }
    }

    public class Node_Journal_StepEditorVM : Node_Journal_StepVM
    {
        public Node_Journal_StepEditorVM(Node_Journal_StepVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }

    public class Node_Journal_AlternativeVM : Node_JournalVM<Node_AlternativeM, object>
    {
        public Node_Journal_AlternativeVM(Node_AlternativeM model, object parent) : base(model, parent) { }
    }

    public class Node_Journal_AlternativeEditorVM : Node_Journal_AlternativeVM
    {
        public Node_Journal_AlternativeEditorVM(Node_Journal_AlternativeVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }
}