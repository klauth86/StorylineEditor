﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels.Tabs;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class CharacterVm : NonFolderVm
    {
        public static string PlayerId => "PLAYER";

        public CharacterVm(CharactersTabVm inParent, long additionalTicks) : base(inParent, additionalTicks) { FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged; }

        public CharacterVm() : this(null, 0) { }

        ~CharacterVm() { FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged; }

        public static void AddPlayerIfHasNoOne(CharactersTabVm Parent)
        {
            if (!Parent.Items.Any(character => character.Id == PlayerId)) {
                var playerCharacter = new CharacterVm(Parent, 0)
                {
                    id = PlayerId,
                    Name = "Основной персонаж"
                };
                Parent.Items.Add(playerCharacter);
            }
        }
    }
}