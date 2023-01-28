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
    public class ActorM : BaseM
    {
        public ActorM(long additionalTicks) : base(additionalTicks)
        {
            hasDescriptionFemale = false;
            descriptionFemale = null;
            actorName = null;
            classPathName = null;
        }

        public ActorM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        { 
            ActorM clone = new ActorM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is ActorM casted)
            {
                casted.hasDescriptionFemale = hasDescriptionFemale;
                casted.descriptionFemale = descriptionFemale;
                casted.actorName = actorName;
                casted.classPathName = classPathName;
            }
        }

        public override bool PassFilter(string filter)
        {
            return
                ((descriptionFemale?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                ((actorName?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                ((classPathName?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0) ||
                base.PassFilter(filter);
        }

        public bool hasDescriptionFemale { get; set; }
        public string descriptionFemale { get; set; }
        public string actorName { get; set; }
        public string classPathName { get; set; }
    }
}