/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Журнальная вершина")]
    [XmlRoot]
    public class JNode_BaseVm : JNode_AlternativeVm
    {
        public JNode_BaseVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public JNode_BaseVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel()
        {
            if (model != null) return model;

            model = new Node_StepM()
            {
                name = Name,
                description = Description,
                gender = (byte)Gender,
                positionX = PositionX,
                positionY = PositionY,
                gameEvents = GameEvents.Select((ge) => (GE_BaseM)ge.GetModel()).ToList(),
                predicates = Predicates.Select((p) => (P_BaseM)p.GetModel()).ToList(), 
                result = null
            };

            return model;
        }

        public override bool IsValid => !string.IsNullOrEmpty(id) &&
            GameEvents.All(gameEvent => gameEvent?.IsValid ?? false) &&
            Predicates.All(predicate => predicate?.IsValid ?? false);

        public override bool AllowsManyChildren => false;
    }
}