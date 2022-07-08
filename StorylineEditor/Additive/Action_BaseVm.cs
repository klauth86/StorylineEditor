/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels;
using System.Xml.Serialization;
using System.Linq;

namespace StorylineEditor.Additive
{
    public abstract class Action_BaseVm
    {
        [XmlIgnore]
        public FullContextVm Parent { get; set; }
        public abstract void Do(object parameter);
        public abstract void Undo();
    }

    #region ACTIONS CharacterVm

    public class Action_SelectCharacterVm : Action_BaseVm
    {
        public string OldSelectedItemId { get; set; }
        public string NewSelectedItemId { get; set; }
        public override void Do(object parameter)
        {
            var characterToSelect = string.IsNullOrEmpty(OldSelectedItemId) ? Parent?.CharactersTab.Items.First(item => item.Id == NewSelectedItemId) : null;
            if (characterToSelect != null) characterToSelect.IsSelected = true;
        }
        public override void Undo()
        {
            var characterToSelect = string.IsNullOrEmpty(NewSelectedItemId) ? Parent?.CharactersTab.Items.First(item => item.Id == OldSelectedItemId) : null;
            if (characterToSelect != null) characterToSelect.IsSelected = true;
        }
    }

    public class Action_CreateCharacterVm : Action_BaseVm
    {
        public string SelectedItemId { get; set; }
        public string CharacterId { get; set; }
        public override void Do(object parameter)
        {
            var characterToCreate = Parent?.CharactersTab.CreateItem(parameter); 
            CharacterId = characterToCreate?.Id; 
            if (characterToCreate != null) Parent?.CharactersTab.AddImpl(characterToCreate);
        }
        public override void Undo()
        {
            Parent.CharactersTab?.RemoveImpl(Parent.CharactersTab.Items.First(item => item.Id == CharacterId));

            var characterToSelect = string.IsNullOrEmpty(SelectedItemId) ? Parent?.CharactersTab.Items.First(item => item.Id == SelectedItemId) : null;
            if (characterToSelect != null) characterToSelect.IsSelected = true;
        }
    }

    public class Action_RemoveCharacterVm : Action_BaseVm
    {
        public CharacterVm Character { get; set; }
        public override void Do(object parameter) => Parent.CharactersTab?.RemoveImpl(Character);
        public override void Undo() => Parent?.CharactersTab.AddImpl(Character);
    }

    public class Action_EditCharacter_NameVm : Action_BaseVm
    {
        public string CharacterId { get; set; }
        public string OldName { get; set; }
        public string NewName { get; set; }

        public override void Do(object parameter)
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            OldName = character.Name;
            character.Name = NewName;
        }

        public override void Undo()
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            character.Name = OldName;
            NewName = null;
        }
    }

    public class Action_EditCharacter_ActorNameVm : Action_BaseVm
    {
        public string CharacterId { get; set; }
        public string OldActorName { get; set; }
        public string NewActorName { get; set; }

        public override void Do(object parameter)
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            OldActorName = character.ActorName;
            character.ActorName = NewActorName;
        }

        public override void Undo()
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            character.ActorName = OldActorName;
            NewActorName = null;
        }
    }

    public class Action_EditCharacter_ClassPathNameVm : Action_BaseVm
    {
        public string CharacterId { get; set; }
        public string OldClassPathName { get; set; }
        public string NewClassPathName { get; set; }

        public override void Do(object parameter)
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            OldClassPathName = character.ClassPathName;
            character.ClassPathName = NewClassPathName;
        }

        public override void Undo()
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            character.ClassPathName = OldClassPathName;
            NewClassPathName = null;
        }
    }

    public class Action_EditCharacter_DescriptionVm : Action_BaseVm
    {
        public string CharacterId { get; set; }
        public string OldDescription { get; set; }
        public string NewDescription { get; set; }

        public override void Do(object parameter)
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            OldDescription = character.Description;
            character.Description = NewDescription;
        }

        public override void Undo()
        {
            var character = Parent.CharactersTab.Items.First(item => item.Id == CharacterId);
            character.Description = OldDescription;
            NewDescription = null;
        }
    }

    #endregion
}
