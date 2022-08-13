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
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Уничтожить актор")]
    [XmlRoot]
    public class GE_DestroyActorVm : GE_BaseVm
    {
        public GE_DestroyActorVm(Node_BaseVm inParent) : base(inParent) {
            ActorToDestroyId = null;
            searchByName = false;
            affectAll = false;
        }

        public GE_DestroyActorVm() : this(null) { }

        public override bool IsValid => base.IsValid && ActorToDestroy != null;

        public string ActorToDestroyId { get; set; }
        
        [XmlIgnore]
        public BaseVm ActorToDestroy
        {
            get
            {
                return Parent.Parent.Parent.Parent.AllActors
                  .FirstOrDefault(item => item?.Id == ActorToDestroyId);
            }
            set
            {
                if (ActorToDestroyId != value?.Id)
                {
                    ActorToDestroyId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected bool searchByName;
        public bool SearchByName
        {
            get => searchByName;
            set
            {
                if (searchByName != value)
                {
                    searchByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool affectAll;
        public bool AffectAll
        {
            get => affectAll;
            set
            {
                if (affectAll != value)
                {
                    affectAll = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData() {
            base.ResetInternalData();
            ActorToDestroy = null;
            SearchByName = false;
            AffectAll = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_DestroyActorVm casted)
            {
                casted.ActorToDestroyId = ActorToDestroyId;
                casted.searchByName = searchByName;
                casted.affectAll = affectAll;
            }
        }
    }
}