﻿/*
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
    public class ItemEditorVM : ItemVM, IRichTextSource
    {
        public ItemEditorVM(ItemVM viewModel) : base(viewModel.Model, viewModel.Parent) { }

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

        public string DescriptionFemale
        {
            get => Model.descriptionFemale;
            set
            {
                if (Model.descriptionFemale != value)
                {
                    Model.descriptionFemale = value;
                    OnModelChanged(Model, nameof(DescriptionFemale));
                }
            }
        }

        public bool HasInternalDescription
        {
            get => Model.hasInternalDescription;
            set
            {
                if (Model.hasInternalDescription != value)
                {
                    Model.hasInternalDescription = value;
                    OnModelChanged(Model, nameof(HasInternalDescription));
                }
            }
        }

        public string InternalDescription
        {
            get => Model.internalDescription;
            set
            {
                if (Model.internalDescription != value)
                {
                    Model.internalDescription = value;
                    OnModelChanged(Model, nameof(InternalDescription));
                }
            }
        }

        public bool HasInternalDescriptionFemale
        {
            get => Model.hasInternalDescriptionFemale;
            set
            {
                if (Model.hasInternalDescriptionFemale != value)
                {
                    Model.hasInternalDescriptionFemale = value;
                    OnModelChanged(Model, nameof(HasInternalDescriptionFemale));
                }
            }
        }

        public string InternalDescriptionFemale
        {
            get => Model.internalDescriptionFemale;
            set
            {
                if (Model.internalDescriptionFemale != value)
                {
                    Model.internalDescriptionFemale = value;
                    OnModelChanged(Model, nameof(InternalDescriptionFemale));
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

        public int RtInternalDescriptionVersion
        {
            get => Model.rtInternalDescriptionVersion;
            private set
            {
                if (value != Model.rtInternalDescriptionVersion)
                {
                    Model.rtInternalDescriptionVersion = value;
                    OnModelChanged(Model, nameof(RtInternalDescriptionVersion));
                }
            }
        }

        public int RtInternalDescriptionFemaleVersion
        {
            get => Model.rtInternalDescriptionFemaleVersion;
            private set
            {
                if (value != Model.rtInternalDescriptionFemaleVersion)
                {
                    Model.rtInternalDescriptionFemaleVersion = value;
                    OnModelChanged(Model, nameof(RtInternalDescriptionFemaleVersion));
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
            if (propName == nameof(Model.rtInternalDescription))
            {
                return Model.rtInternalDescription;
            }
            else if (propName == nameof(Model.rtInternalDescriptionFemale))
            {
                return Model.rtInternalDescriptionFemale;
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
            if (propName == nameof(Model.rtInternalDescription))
            {
                Model.rtInternalDescription = textRangeModel;
                RtInternalDescriptionVersion = (RtInternalDescriptionVersion + 1) % TextRangeM.CYCLE;
            }
            else if (propName == nameof(Model.rtInternalDescriptionFemale))
            {
                Model.rtInternalDescriptionFemale = textRangeModel;
                RtInternalDescriptionFemaleVersion = (RtInternalDescriptionFemaleVersion + 1) % TextRangeM.CYCLE;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
    }
}