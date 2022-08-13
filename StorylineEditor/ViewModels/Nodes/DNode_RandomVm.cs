/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Случайная вершина")]
    [XmlRoot]
    public class DNode_RandomVm : Node_InteractiveVm
    {
        public DNode_RandomVm(TreeVm Parent) : base(Parent) { }

        public DNode_RandomVm() : this(null) { }

        public override bool IsValid => !string.IsNullOrEmpty(id) &&
            GameEvents.All(gameEvent => gameEvent?.IsValid ?? false) &&
            Predicates.All(predicate => predicate?.IsValid ?? false);
    }
}