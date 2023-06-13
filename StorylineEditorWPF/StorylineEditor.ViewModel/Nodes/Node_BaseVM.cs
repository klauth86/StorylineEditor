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

using StorylineEditor.Model.Behaviors;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel.Behaviors;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.GameEvents;
using StorylineEditor.ViewModel.Interface;
using StorylineEditor.ViewModel.Predicates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_BaseVM<T, U>
        : BaseVM<T, U>
        , INode
        , IRichTextSource
        where T : Node_BaseM
        where U : class
    {
        public Node_BaseVM(
            T model
            , U parent
            )
            : base(
                  model
                  , parent
                  )
        {
            zIndex = 100;

            Behaviors = new ObservableCollection<IBehavior>();
            foreach (var behaviorModel in Model.behaviors) Behaviors.Add(BehaviorHelper.CreateBehaviorByModel(behaviorModel, this));
        }

        public byte Gender
        {
            get => Model.gender;
            set
            {
                if (Model.gender != value)
                {
                    Model.gender = value;
                    OnModelChanged(Model, nameof(Gender));

                    ActiveContext.ActiveGraph.OnNodeGenderChanged(this);
                }
            }
        }

        public double PositionX
        {
            get => Model.positionX;
            set
            {
                if (Model.positionX != value)
                {
                    Model.positionX = value;
                    OnModelChanged(Model, nameof(PositionX));

                    ActiveContext.ActiveGraph.OnNodePositionChanged(this, ENodeUpdateFlags.X);
                }
            }
        }

        public double PositionY
        {
            get => Model.positionY;
            set
            {
                if (Model.positionY != value)
                {
                    Model.positionY = value;
                    OnModelChanged(Model, nameof(PositionY));

                    ActiveContext.ActiveGraph.OnNodePositionChanged(this, ENodeUpdateFlags.Y);
                }
            }
        }

        public string CharacterId => null;

        protected double width;
        public double Width
        {
            get => width;
            set
            {
                if (width != value)
                {
                    width = value;

                    ActiveContext.ActiveGraph.OnNodeSizeChanged(this, ENodeUpdateFlags.X);
                }
            }
        }

        protected double height;
        public double Height
        {
            get => height;
            set
            {
                if (height != value)
                {
                    height = value;

                    ActiveContext.ActiveGraph.OnNodeSizeChanged(this, ENodeUpdateFlags.Y);
                }
            }
        }

        protected double left;
        public double Left
        {
            get => left;
            set
            {
                if (left != value)
                {
                    left = value;
                    Notify(nameof(Left));
                }
            }
        }

        protected double top;
        public double Top
        {
            get => top;
            set
            {
                if (top != value)
                {
                    top = value;
                    Notify(nameof(Top));
                }
            }
        }

        private ICommand toggleGenderCommand;
        public ICommand ToggleGenderCommand => toggleGenderCommand ?? (toggleGenderCommand = new RelayCommand(() => { Gender = (byte)((Gender + 1) % 3); }));

        protected int zIndex;
        public int ZIndex => zIndex;

        public int RtDescriptionVersion
        {
            get => Model.rtDescriptionVersion;
            private set
            {
                if (value != Model.rtDescriptionVersion)
                {
                    Model.rtDescriptionVersion = value;
                    OnModelChanged(Model, nameof(RtDescriptionVersion));
                }
            }
        }

        public TextRangeM GetRichText(string propName)
        {
            return Model.rtDescription;
        }

        public virtual void SetRichText(string propName, ref TextRangeM textRangeModel)
        {
            Model.rtDescription = textRangeModel;
            RtDescriptionVersion = (RtDescriptionVersion + 1) % TextRangeM.CYCLE;
        }

        public Type SelectedBehaviorType
        {
            get => null;
            set
            {
                if (value != null)
                {
                    IBehavior behaviorViewModel = BehaviorHelper.CreateBehaviorByType(value, this);
                    Model.behaviors.Add(behaviorViewModel.GetModel<B_BaseM>());
                    Behaviors.Add(behaviorViewModel);

                    OnModelChanged(Model, nameof(HasBehaviors));
                }

                Notify(nameof(SelectedBehaviorType));
            }
        }
        public ObservableCollection<IBehavior> Behaviors { get; }
        public bool HasBehaviors => Model.behaviors.Count > 0;

        protected ICommand removeElementCommand;
        public ICommand RemoveElementCommand => removeElementCommand ?? (removeElementCommand = new RelayCommand<IWithModel>((viewModel) =>
        {
            RemoveElementInternal(viewModel);
        }));

        protected virtual void RemoveElementInternal(IWithModel viewModel)
        {
            if (viewModel is IBehavior behaviorViewModel && Behaviors.Remove(behaviorViewModel))
            {
                Model.behaviors.Remove(behaviorViewModel.GetModel<B_BaseM>());
                OnModelChanged(Model, nameof(HasBehaviors));
            }
        }
    }

    public class Node_DelayVM : Node_BaseVM<Node_DelayM, object>
    {
        public Node_DelayVM(Node_DelayM model, object parent) : base(model, parent) { }

        public double Delay
        {
            get => Model.delay;
            set
            {
                if (Model.delay != value)
                {
                    Model.delay = value;
                    OnModelChanged(Model, nameof(Delay));
                    Notify(nameof(Delay));
                }
            }
        }
    }

    public class Node_DelayEditorVM : Node_DelayVM
    {
        public Node_DelayEditorVM(Node_DelayVM viewModel) : base(viewModel.Model, viewModel.Parent) { }
    }
}