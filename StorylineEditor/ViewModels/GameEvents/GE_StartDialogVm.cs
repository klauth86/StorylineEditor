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
using System.Windows.Data;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Начать диалог")]
    [XmlRoot]
    public class GE_StartDialogVm : GE_BaseVm
    {
        public GE_StartDialogVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            CharacterAId = null;
            CharacterBId = null;
        }

        public GE_StartDialogVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && CharacterA != null && CharacterB != null;

        public string CharacterAId { get; set; }

        [XmlIgnore]
        public FolderedVm CharacterA
        {
            get => Parent?.Parent?.Parent?.Parent?.NPCharacters.FirstOrDefault(item => item?.Id == CharacterAId);
            set
            {
                if (CharacterAId != value?.Id)
                {
                    CharacterAId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public string CharacterBId { get; set; }
        
        [XmlIgnore]
        public FolderedVm CharacterB
        {
            get => Parent?.Parent?.Parent?.Parent?.Characters.FirstOrDefault(item => item?.Id == CharacterBId);
            set
            {
                if (CharacterBId != value?.Id)
                {
                    CharacterBId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected CollectionViewSource actualCharactersSource;
        [XmlIgnore]
        public ICollectionView ActualCharacters
        {
            get
            {
                if (actualCharactersSource == null)
                {
                    actualCharactersSource = new CollectionViewSource() { Source = Parent?.Parent?.Parent?.Parent?.NPCharacters };

                    if (actualCharactersSource.View != null)
                    {
                        actualCharactersSource.View.MoveCurrentTo(null);
                    }
                }

                if (actualCharactersSource.View != null)
                {
                    actualCharactersSource.View.Filter = (object obj) => string.IsNullOrEmpty(characterFilter) || obj != null && ((BaseVm)obj).PassFilter(characterFilter);
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

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            CharacterA = null;
            CharacterB = null;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is GE_StartDialogVm casted)
            {
                casted.CharacterAId = CharacterAId;
                casted.CharacterBId = CharacterBId;
            }
        }
    }
}
