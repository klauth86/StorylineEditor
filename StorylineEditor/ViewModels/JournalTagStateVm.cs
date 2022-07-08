/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class JournalTagStateVm : BaseVm<BaseVm>
    {
        public JournalTagStateVm(BaseVm Parent, JournalTagVm tag) : base(Parent)
        {
            TagId = tag?.Id;
            Tag = tag;
            HasTag = false;
        }

        public JournalTagStateVm() : this(null, null) { }

        public string TagId { get; set; }

        [XmlIgnore]
        public JournalTagVm Tag { get; set; }

        protected bool hasTag;
        public bool HasTag
        {
            get => hasTag;
            set
            {
                if (value != hasTag)
                {
                    hasTag = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public override void SetupParenthood()
        {
            if (Parent is BaseVm<Node_BaseVm> nodeChild)
            {
                Tag = nodeChild.Parent.Parent.Parent.Parent.GlobalTagsTab.Items.FirstOrDefault((tag) => tag.Id == TagId);
            }
        }
    }
}
