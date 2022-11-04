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
    public class P_CompositeM : P_BaseM
    {
        public P_CompositeM(long additionalTicks) : base(additionalTicks)
        {
            compositionType = COMPOSITION_TYPE.AND;
            predicateA = null;
            predicateB = null;
        }

        public P_CompositeM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            P_CompositeM clone = new P_CompositeM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is P_CompositeM casted)
            {
                casted.compositionType = compositionType;
                casted.predicateA = predicateA.CloneAs<P_BaseM>(0);
                casted.predicateB = predicateB.CloneAs<P_BaseM>(1);
            }
        }

        public byte compositionType { get; set; }
        public P_BaseM predicateA { get; set; }
        public P_BaseM predicateB { get; set; }
    }
}