﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Model.GameEvents
{
    public class GE_Quest_AddM : GE_BaseM
    {
        public GE_Quest_AddM(long additionalTicks) : base(additionalTicks)
        {
            questId = null;
        }

        public GE_Quest_AddM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            GE_Quest_AddM clone = new GE_Quest_AddM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is GE_Quest_AddM casted)
            {
                casted.questId = questId;
            }
        }

        public string questId { get; set; }
    }
}