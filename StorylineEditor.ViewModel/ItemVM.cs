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

namespace StorylineEditor.ViewModel
{
    public class ItemVM : BaseVM<ItemM>
    {
        public ItemVM(ItemM model) : base(model, null) { }
    }

    public class ItemEditorVM : ItemVM
    {
        public ItemEditorVM(ItemVM viewModel) : base(viewModel.Model) { }

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
    }
}