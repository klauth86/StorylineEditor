/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_JournalM : Node_InteractiveM
    {
        public Node_JournalM(long additionalTicks) : base(additionalTicks)
        {
            result = null;
        }

        public Node_JournalM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_JournalM casted)
            {
                casted.result = result;
            }
        }

        public string result { get; set; }
    }

    public class Node_StepM : Node_JournalM
    {
        public Node_StepM(long additionalTicks) : base(additionalTicks) { }

        public Node_StepM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_StepM clone = new Node_StepM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_AlternativeM : Node_JournalM
    {
        public Node_AlternativeM(long additionalTicks) : base(additionalTicks) { }

        public Node_AlternativeM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_AlternativeM clone = new Node_AlternativeM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}