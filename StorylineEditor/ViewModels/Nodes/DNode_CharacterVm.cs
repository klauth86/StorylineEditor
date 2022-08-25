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
    [XmlRoot]
    public class ParticipantStateVm : BaseVm<DNode_CharacterVm>
    {
        public ParticipantStateVm(DNode_CharacterVm Parent, long additionalTicks, FolderedVm inCharacter) : base(Parent, additionalTicks)
        {
            character = inCharacter;
            CharacterId = character?.Id ?? null;

            SetupParenthood();
        }

        public ParticipantStateVm() : this(null, 0, null) { }

        public string CharacterId { get; set; }

        protected FolderedVm character;
        public FolderedVm Character => character;

        protected bool isLocked;
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (value != isLocked)
                {
                    isLocked = value;
                    NotifyWithCallerPropName();

                    Notify(nameof(IsSet));
                }
            }
        }

        protected bool isUnlocked;
        public bool IsUnlocked
        {
            get => isUnlocked;
            set
            {
                if (value != isUnlocked)
                {
                    isUnlocked = value;
                    NotifyWithCallerPropName();

                    Notify(nameof(IsSet));
                }
            }
        }

        public string FocusActorId { get; set; }

        [XmlIgnore]
        public BaseVm FocusActor {
            get => Parent?.Parent.Parent.Parent.AllActors.FirstOrDefault(item=>item.Id == FocusActorId);
            set {
                if (value != FocusActor) {}
            }
        }

        protected bool searchByName;
        public bool SearchByName
        {
            get => searchByName;
            set
            {
                if (value != searchByName)
                {
                    searchByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public bool IsSet => IsLocked || IsUnlocked;

        public void Reset()
        {
            IsLocked = false;
            IsUnlocked = false;
            FocusActor = null;
            SearchByName = false;
        }

        public override void SetupParenthood()
        {
            if (Parent != null)
            {
                character = Parent.Parent.Parent.Parent.CharactersTab.Items.FirstOrDefault((charac) => charac.Id == CharacterId);
            }
            else {
                isLocked = false;
                isUnlocked = false;
                FocusActorId = null;
                searchByName = false;
            }
        }
    }

    [Description("Регулярная вершина")]
    [XmlRoot]
    public class DNode_CharacterVm : Node_InteractiveVm, IOwnered
    {
        public DNode_CharacterVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            OwnerId = CharacterVm.PlayerId;
        }

        public DNode_CharacterVm() : this(null, 0) { }


        public string OwnerId { get; set; }


        protected string overrideOwnerName;
        public string OverrideOwnerName {
            get => overrideOwnerName;
            set
            {
                if (value != overrideOwnerName)
                {
                    overrideOwnerName = value;
                    NotifyWithCallerPropName();

                    Notify(nameof(OwnerName));
                }
            } 
        }


        public string OwnerName => string.IsNullOrEmpty(OverrideOwnerName) ? Owner?.Name : string.Format("{0} [{1}]", Owner?.Name, OverrideOwnerName);


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
                    Notify(nameof(OwnerName));
                    NotifyIsValidChanged();
                }
            }
        }

        protected string attachedFile;
        public string AttachedFile
        {
            get => string.IsNullOrEmpty(attachedFile) ? null : attachedFile;
            set
            {
                if (value != attachedFile)
                {
                    attachedFile = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public override bool IsValid => base.IsValid && Owner != null;

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is DNode_CharacterVm casted)
            {
                casted.OwnerId = OwnerId;                
                casted.attachedFile = attachedFile;
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Owner != null && Owner.PassFilter(filter);
    }
}