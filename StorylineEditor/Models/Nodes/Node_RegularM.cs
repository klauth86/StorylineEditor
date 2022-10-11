/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

namespace StorylineEditor.Models.Nodes
{
    public abstract class Node_RegularM : Node_InteractiveM
    {
        public Node_RegularM(long additionalTicks) : base(additionalTicks)
        {
            characterId = null;
        }

        public Node_RegularM() : this(0) { }

        public string characterId { get; set; }
    }

    public class Node_ReplicaM : Node_RegularM { }

    public class Node_DialogM : Node_RegularM { }
}