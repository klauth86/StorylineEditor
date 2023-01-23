/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;

namespace StorylineEditor.Model
{
    public static class STORAGE_TYPE
    {
        public const byte UNSET = 0;
        public const byte GOOGLE_DRIVE = 1;
    }

    public static class GENDER
    {
        public const byte UNSET = 0;
        public const byte MALE = 1;
        public const byte FEMALE = 2;
    }

    public static class EXECUTION_MODE
    {
        public const byte UNSET = 0;
        public const byte ON_ENTER = 1;
        public const byte ON_LEAVE = 2;
    }

    public static class COMPOSITION_TYPE
    {
        public const byte UNSET = 0;
        public const byte AND = 1;
        public const byte OR = 2;
        public const byte XOR = 3;
    }

    public static class COMPARE_TYPE
    {
        public const byte UNSET = 0;
        public const byte LESS = 1;
        public const byte LESS_OR_EQUAL = 2;
        public const byte EQUAL = 3;
        public const byte EQUAL_OR_GREATER = 4;
        public const byte GREATER = 5;
    }

    public class StorylineM : BaseM
    {
        public StorylineM(long additionalTicks) : base(additionalTicks)
        {
            locations = new List<BaseM>();
            characters = new List<BaseM>();
            items = new List<BaseM>();
            actors = new List<BaseM>();
            journal = new List<BaseM>();
            dialogs = new List<BaseM>();
            replicas = new List<BaseM>();
        }

        public StorylineM() : this(0) { }

        public override BaseM Clone(long additionalTicks) => throw new NotImplementedException();
        protected override void CloneInternal(BaseM targetObject) => throw new NotImplementedException();

        public List<BaseM> locations { get; set; }
        public List<BaseM> characters { get; set; }
        public List<BaseM> items { get; set; }
        public List<BaseM> actors { get; set; }
        public List<BaseM> journal { get; set; }
        public List<BaseM> dialogs { get; set; }
        public List<BaseM> replicas { get; set; }
    }
}