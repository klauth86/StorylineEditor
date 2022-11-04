/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;

namespace StorylineEditor.Model
{
    public abstract class BaseM
    {
        public BaseM(long additionalTicks)
        {
            createdAt = DateTime.Now;
            id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", GetType().Name, createdAt, createdAt.Ticks, additionalTicks);
            name = null;
            description = null;
        }

        public BaseM() : this(0) { }

        public T CloneAs<T>(long additionalTicks) where T : BaseM { return (T)Clone(additionalTicks); }
        public abstract BaseM Clone(long additionalTicks);
        protected virtual void CloneInternal(BaseM targetObject)
        {
            targetObject.name = name;
            targetObject.description = description;
        }

        public DateTime createdAt { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }
}