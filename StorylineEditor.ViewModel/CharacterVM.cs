/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;

namespace StorylineEditor.ViewModel
{
    public class CharacterVM : BaseVM<CharacterM>
    {
        public CharacterVM(CharacterM model) : base(model) { }

        public string Id => Model.id;

        public string Name
        {
            get => Model.name;
            set
            {
                if (Model.name != value)
                {
                    Model.name = value;
                    Notify(nameof(Name));
                }
            }
        }
    }

    public class CharacterEditorVM : CharacterVM
    {
        public CharacterEditorVM(CharacterM model) : base(model) { }

        public string Description
        {
            get => Model.description;
            set
            {
                if (Model.description != value)
                {
                    Model.description = value;
                    Notify(nameof(Description));
                }
            }
        }

        public bool HasDescriptionFemale
        {
            get => Model.hasDescriptionFemale;
            set
            {
                if (Model.hasDescriptionFemale != value)
                {
                    Model.hasDescriptionFemale = value;
                    Notify(nameof(HasDescriptionFemale));
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
                    Notify(nameof(DescriptionFemale));
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
                    Notify(nameof(ActorName));
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
                    Notify(nameof(ClassPathName));
                }
            }
        }
    }
}