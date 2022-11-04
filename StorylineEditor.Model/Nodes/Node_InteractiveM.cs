/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Predicates;
using System.Collections.Generic;

namespace StorylineEditor.Model.Nodes
{
    public abstract class Node_InteractiveM : Node_BaseM
    {
        public Node_InteractiveM(long additionalTicks) : base(additionalTicks)
        {
            predicates = new List<P_BaseM>();
            gameEvents = new List<GE_BaseM>();
        }

        public Node_InteractiveM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_InteractiveM casted)
            {
                for (int i = 0; i < predicates.Count; i++)
                {
                    casted.predicates.Add(predicates[i].CloneAs<P_BaseM>(i));
                }

                for (int i = 0; i < gameEvents.Count; i++)
                {
                    casted.gameEvents.Add(gameEvents[i].CloneAs<GE_BaseM>(i));
                }
            }
        }

        public List<P_BaseM> predicates { get; set; }
        public List<GE_BaseM> gameEvents { get; set; }
    }

    public class Node_RandomM : Node_InteractiveM
    {
        public Node_RandomM(long additionalTicks) : base(additionalTicks) { }

        public Node_RandomM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_RandomM clone = new Node_RandomM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_TransitM : Node_InteractiveM
    {
        public Node_TransitM(long additionalTicks) : base(additionalTicks) { }

        public Node_TransitM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_TransitM clone = new Node_TransitM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}