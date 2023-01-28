/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace StorylineEditor.Model
{
    public class FolderM : BaseM
    {
        public FolderM(long additionalTicks) : base(additionalTicks)
        {
            content = new List<BaseM>();
        }

        public FolderM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            FolderM clone = new FolderM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is FolderM casted)
            {
                for (int i = 0; i < content.Count; i++)
                {
                    casted.content.Add(content[i].Clone(i));
                }
            }
        }

        public List<BaseM> content { get; set; }

        public override bool PassFilter(string filter)
        {
            for (int i = 0; i < content.Count; i++)
            {
                if (content[i].PassFilter(filter)) return true;
            }

            return base.PassFilter(filter);
        }
    }
}