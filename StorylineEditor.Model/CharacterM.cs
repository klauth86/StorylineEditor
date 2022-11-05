﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Model
{
    public class CharacterM : ActorM
    {
        public static readonly string PLAYER_ID = "PLAYER";

        public CharacterM(long additionalTicks) : base(additionalTicks)
        {
            initialRelation = 0;
            initialRelationFemale = 0;
        }

        public CharacterM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            CharacterM clone = new CharacterM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is CharacterM casted)
            {
                casted.initialRelation = initialRelation;
                casted.initialRelationFemale = initialRelationFemale;
            }
        }

        public float initialRelation { get; set; }
        public float initialRelationFemale { get; set; }
    }
}