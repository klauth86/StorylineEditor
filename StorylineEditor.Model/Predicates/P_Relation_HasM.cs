/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Model.Predicates
{
    public class P_Relation_HasM : P_BaseM
    {
        public P_Relation_HasM(long additionalTicks) : base(additionalTicks)
        {
            npcId = null;
            compareType = COMPARE_TYPE.LESS;
            value = 0;
        }

        public P_Relation_HasM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            P_Relation_HasM clone = new P_Relation_HasM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_Relation_HasM casted)
            {
                casted.npcId = npcId;
                casted.compareType = compareType;
                casted.value = value;
            }
        }

        public string npcId { get; set; }
        public byte compareType { get; set; }
        public float value { get; set; }
    }
}