/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public class NodePairVm : Notifier
    {
        public NodePairVm() { }

        public string FromId { get; set; }

        public string ToId { get; set; }


        protected string description;
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        [XmlIgnore]
        public TreeVm Parent { get; set; }

        [XmlIgnore]
        public Node_BaseVm From => Parent?.Nodes.FirstOrDefault(node => node.Id == FromId);

        [XmlIgnore]
        public Node_BaseVm To => Parent?.Nodes.FirstOrDefault(node => node.Id == ToId);

        protected bool isVisible;
        [XmlIgnore]
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (value != isVisible)
                {
                    isVisible = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public bool PassFilter(string filter) => 
            Description != null && Description.Contains(filter) ||
            From != null && From.PassFilter(filter) && 
            To != null && To.PassFilter(filter);

        ////// TODO We dont have all BaseVm things here, however this dublicated code also is not good enough
    }
}