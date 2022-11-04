/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Model
{
    public class ItemM : ActorM
    {
        public ItemM(long additionalTicks) : base(additionalTicks)
        {
            hasInternalDescription = false;
            internalDescription = null;
            hasInternalDescriptionFemale = false;
            internalDescriptionFemale = null;
        }

        public ItemM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            ItemM clone = new ItemM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is ItemM casted)
            {
                casted.hasInternalDescription = hasInternalDescription;
                casted.internalDescription = internalDescription;
                casted.hasInternalDescriptionFemale = hasInternalDescriptionFemale;
                casted.internalDescriptionFemale = internalDescriptionFemale;
            }
        }

        public bool hasInternalDescription { get; set; }
        public string internalDescription { get; set; }
        public bool hasInternalDescriptionFemale { get; set; }
        public string internalDescriptionFemale { get; set; }
    }
}