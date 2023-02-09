/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Interface;
using System.Windows.Documents;

namespace StorylineEditor.ViewModel
{
    public class ItemEditorVM : ItemVM, IPartiallyStored
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

        protected FlowDocument descriptionFlow;
        public FlowDocument DescriptionFlow
        {
            get
            {
                if (descriptionFlow == null)
                {
                    descriptionFlow = ActiveContext.FlowDocumentService.ConvertBack(Description, ActiveContext.SerializationService);
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

                    Description = DescriptionFlow != null ? ActiveContext.FlowDocumentService.ConvertTo(DescriptionFlow, ActiveContext.SerializationService) : null;
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
                    descriptionFlowFemale = ActiveContext.FlowDocumentService.ConvertBack(DescriptionFemale, ActiveContext.SerializationService);
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

                    DescriptionFemale = DescriptionFlowFemale != null ? ActiveContext.FlowDocumentService.ConvertTo(DescriptionFlowFemale, ActiveContext.SerializationService) : null;
                }
            }
        }

        protected FlowDocument internalDescriptionFlow;
        public FlowDocument InternalDescriptionFlow
        {
            get
            {
                if (internalDescriptionFlow == null)
                {
                    internalDescriptionFlow = ActiveContext.FlowDocumentService.ConvertBack(InternalDescription, ActiveContext.SerializationService);
                    internalDescriptionFlow.Name = Id;
                }

                return internalDescriptionFlow;
            }
        }

        protected bool internalDocumentChangedFlag;
        public bool InternalDocumentChangedFlag
        {
            get => internalDocumentChangedFlag;
            set
            {
                if (value != internalDocumentChangedFlag)
                {
                    internalDocumentChangedFlag = value;

                    InternalDescription = InternalDescriptionFlow != null ? ActiveContext.FlowDocumentService.ConvertTo(InternalDescriptionFlow, ActiveContext.SerializationService) : null;
                }
            }
        }

        protected FlowDocument internalDescriptionFlowFemale;
        public FlowDocument InternalDescriptionFlowFemale
        {
            get
            {
                if (internalDescriptionFlowFemale == null)
                {
                    internalDescriptionFlowFemale = ActiveContext.FlowDocumentService.ConvertBack(InternalDescriptionFemale, ActiveContext.SerializationService);
                    internalDescriptionFlowFemale.Name = Id;
                }

                return internalDescriptionFlowFemale;
            }
        }

        protected bool internalDocumentChangedFlagFemale;
        public bool InternalDocumentChangedFlagFemale
        {
            get => internalDocumentChangedFlagFemale;
            set
            {
                if (value != internalDocumentChangedFlagFemale)
                {
                    internalDocumentChangedFlagFemale = value;

                    InternalDescriptionFemale = InternalDescriptionFlowFemale != null ? ActiveContext.FlowDocumentService.ConvertTo(InternalDescriptionFlowFemale, ActiveContext.SerializationService) : null;
                }
            }
        }

        public void OnEnter() { }

        public void OnLeave()
        {
            descriptionFlow = null;
            descriptionFlowFemale = null;
            internalDescriptionFlow = null;
            internalDescriptionFlowFemale = null;
        }
    }
}