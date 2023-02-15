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

using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel
{
    public class ActorEditorVM : ActorVM, IRichTextSource
    {
        public ActorEditorVM(ActorVM viewModel) : base(viewModel.Model, viewModel.Parent) { }

        public bool HasDescriptionFemale
        {
            get => Model.hasDescriptionFemale;
            set
            {
                if (Model.hasDescriptionFemale != value)
                {
                    Model.hasDescriptionFemale = value;
                    OnModelChanged(Model, nameof(HasDescriptionFemale));
                }
            }
        }

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

        public int RtDescriptionFemaleVersion
        {
            get => Model.rtDescriptionFemaleVersion;
            private set
            {
                if (value != Model.rtDescriptionFemaleVersion)
                {
                    Model.rtDescriptionFemaleVersion = value;
                    OnModelChanged(Model, nameof(RtDescriptionFemaleVersion));
                }
            }
        }

        public TextRangeM GetRichText(string propName)
        {
            if (propName == nameof(Model.rtDescription))
            {
                return Model.rtDescription;
            }
            else if (propName == nameof(Model.rtDescriptionFemale))
            {
                return Model.rtDescriptionFemale;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }

        public void SetRichText(string propName, ref TextRangeM textRangeModel)
        {
            if (propName == nameof(Model.rtDescription))
            {
                Model.rtDescription = textRangeModel;
                RtDescriptionVersion = (RtDescriptionVersion + 1) % TextRangeM.CYCLE;
            }
            else if (propName == nameof(Model.rtDescriptionFemale))
            {
                Model.rtDescriptionFemale = textRangeModel;
                RtDescriptionFemaleVersion = (RtDescriptionFemaleVersion + 1) % TextRangeM.CYCLE;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
    }
}