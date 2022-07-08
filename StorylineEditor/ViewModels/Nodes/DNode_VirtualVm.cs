/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Виртуальная вершина")]
    [XmlRoot]
    public class DNode_VirtualVm : Node_BaseVm, IOwnered
    {
        public DNode_VirtualVm(TreeVm Parent) : base(Parent)
        {
            OwnerId = CharacterVm.PlayerId;
        }

        public DNode_VirtualVm() : this(null) { }

        public override bool IsValid => !string.IsNullOrEmpty(id) && Parent != null && Owner != null;

        public string OwnerId { get; set; }

        [XmlIgnore]
        public FolderedVm Owner
        {
            get
            {
                return Parent?.Parent.Parent.CharactersTab?.Items
                    .FirstOrDefault(item => item?.Id == OwnerId);
            }
            set
            {
                if (OwnerId != value?.Id)
                {
                    Parent?.RemoveParticipant(Owner);
                    OwnerId = value?.Id;
                    Parent?.AddParticipant(Owner);

                    NotifyWithCallerPropName();
                    Notify(nameof(OwnerId));
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is DNode_VirtualVm casted)
            {
                casted.OwnerId = OwnerId;
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Owner != null && Owner.PassFilter(filter);
    }
}