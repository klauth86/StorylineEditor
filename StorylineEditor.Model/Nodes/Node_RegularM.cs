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
    public abstract class Node_RegularM : Node_InteractiveM
    {
        public Node_RegularM(long additionalTicks) : base(additionalTicks)
        {
            characterId = null;
            overrideName = null;
            fileHttpRef = null;
            shortDescription = null;
        }

        public Node_RegularM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is Node_RegularM casted)
            {
                casted.characterId = characterId;
                casted.overrideName = overrideName;
                casted.fileHttpRef = fileHttpRef;
                casted.shortDescription = shortDescription;
            }
        }

        public string characterId { get; set; }
        public string overrideName { get; set; }
        public string fileHttpRef { get; set; }
        public string shortDescription { get; set; }
    }

    public class Node_ReplicaM : Node_RegularM
    {
        public Node_ReplicaM(long additionalTicks) : base(additionalTicks) { }

        public Node_ReplicaM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_ReplicaM clone = new Node_ReplicaM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }

    public class Node_DialogM : Node_RegularM
    {
        public Node_DialogM(long additionalTicks) : base(additionalTicks) { }

        public Node_DialogM() : this(0) { }

        public override BaseM Clone(long additionalTicks)
        {
            Node_DialogM clone = new Node_DialogM(additionalTicks);
            CloneInternal(clone);
            return clone;
        }
    }
}