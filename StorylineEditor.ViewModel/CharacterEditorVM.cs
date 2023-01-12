/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Helpers;
using StorylineEditor.ViewModel.Interface;
using System.Windows.Documents;

namespace StorylineEditor.ViewModel
{
    public class CharacterEditorVM : CharacterVM, IPartiallyStored
    {
        public CharacterEditorVM(CharacterVM viewModel) : base(viewModel.Model, viewModel.Parent) { }

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

        public float InitialRelation
        {
            get => Model.initialRelation;
            set
            {
                if (Model.initialRelation != value)
                {
                    Model.initialRelation = value;
                    OnModelChanged(Model, nameof(InitialRelation));
                }
            }
        }

        public float InitialRelationFemale
        {
            get => Model.initialRelationFemale;
            set
            {
                if (Model.initialRelationFemale != value)
                {
                    Model.initialRelationFemale = value;
                    OnModelChanged(Model, nameof(InitialRelationFemale));
                }
            }
        }

        protected FlowDocument descriptionFlow;
        public FlowDocument DescriptionFlow
        {
            get
            {
                if (descriptionFlow == null)
                {
                    descriptionFlow = FlowDocumentHelper.ConvertBack(Description);
                    descriptionFlow.Name = Id;
                }

                return descriptionFlow;
            }
        }

        protected bool documentChangedFlag;
        public bool DocumentChangedFlag
        {
            get => documentChangedFlag;
            set
            {
                if (value != documentChangedFlag)
                {
                    documentChangedFlag = value;

                    Description = DescriptionFlow != null ? FlowDocumentHelper.ConvertTo(DescriptionFlow) : null;
                }
            }
        }

        protected FlowDocument descriptionFlowFemale;
        public FlowDocument DescriptionFlowFemale
        {
            get
            {
                if (descriptionFlowFemale == null)
                {
                    descriptionFlowFemale = FlowDocumentHelper.ConvertBack(DescriptionFemale);
                    descriptionFlowFemale.Name = Id;
                }

                return descriptionFlowFemale;
            }
        }

        protected bool documentChangedFlagFemale;
        public bool DocumentChangedFlagFemale
        {
            get => documentChangedFlagFemale;
            set
            {
                if (value != documentChangedFlagFemale)
                {
                    documentChangedFlagFemale = value;

                    DescriptionFemale = DescriptionFlowFemale != null ? FlowDocumentHelper.ConvertTo(DescriptionFlowFemale) : null;
                }
            }
        }

        public void OnEnter() { }

        public void OnLeave()
        {
            descriptionFlow = null;
            descriptionFlowFemale = null;
        }
    }
}