﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
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

        public string ActorName
        {
            get => Model.actorName;
            set
            {
                if (Model.actorName != value)
                {
                    Model.actorName = value;
                    OnModelChanged(Model, nameof(ActorName));
                }
            }
        }

        public string ClassPathName
        {
            get => Model.classPathName;
            set
            {
                if (Model.classPathName != value)
                {
                    Model.classPathName = value;
                    OnModelChanged(Model, nameof(ClassPathName));
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

        public void OnRichTextChanged(string propName, ref TextRangeM textRangeModel)
        {
            if (propName == nameof(Model.rtDescription))
            {
                Model.rtDescription = textRangeModel;
                OnModelChanged(Model, nameof(Model.rtDescription));
            }
            else if (propName == nameof(Model.rtDescriptionFemale))
            {
                Model.rtDescriptionFemale = textRangeModel;
                OnModelChanged(Model, nameof(Model.rtDescriptionFemale));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propName));
            }
        }
    }
}