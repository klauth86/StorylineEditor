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
using System.Windows.Data;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
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
            get => Parent?.Parent?.Parent?.Characters.FirstOrDefault(item => item?.Id == OwnerId);
            set
            {
                if (OwnerId != value?.Id)
                {
                    OwnerId = value?.Id;

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

        protected CollectionViewSource actualCharactersSource;
        [XmlIgnore]
        public ICollectionView ActualCharacters
        {
            get
            {
                if (actualCharactersSource == null)
                {
                    actualCharactersSource = new CollectionViewSource() { Source = Parent?.Parent?.Parent?.Characters };
                }

                if (actualCharactersSource.View != null)
                {
                    actualCharactersSource.View.Filter = (object obj) => string.IsNullOrEmpty(characterFilter) || obj != null && ((BaseVm)obj).PassFilter(characterFilter);
                    actualCharactersSource.View.MoveCurrentTo(null);
                }

                return actualCharactersSource.View;
            }
        }

        protected string characterFilter;
        [XmlIgnore]
        public string CharacterFilter
        {
            get => characterFilter;
            set
            {
                if (value != characterFilter)
                {
                    characterFilter = value;
                    ActualCharacters?.Refresh();
                }
            }
        }

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