/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Models.Graphs;
using System.Collections.Generic;

namespace StorylineEditor.Models
{
    public static class GENDER
    {
        public const byte UNSET = 0;
        public const byte MALE = 1;
        public const byte FEMALE = 2;
    }

    public static class EXECUTION_MODE
    {
        public const byte UNSET = 0;
        public const byte BEFORE = 1;
        public const byte AFTER = 2;
    }

    public static class COMPOSITION_TYPE
    {
        public const byte UNSET = 0;
        public const byte AND = 1;
        public const byte OR = 2;
        public const byte XOR = 3;
    }

    public class StorylineM : BaseM
    {
        public StorylineM(long additionalTicks) : base(additionalTicks)
        {
            characters = new List<CharacterM>();
            items = new List<ItemM>();
            actors = new List<ActorM>();
            journal = new List<QuestM>();
            dialogs = new List<ActiveDialogM>();
            replicas = new List<PassiveDialogM>();
        }

        public StorylineM() : this(0) { }

        public List<CharacterM> characters { get; set; }
        public List<ItemM> items { get; set; }
        public List<ActorM> actors { get; set; }
        public List<QuestM> journal { get; set; }
        public List<ActiveDialogM> dialogs { get; set; }
        public List<PassiveDialogM> replicas { get; set; }
    }
}