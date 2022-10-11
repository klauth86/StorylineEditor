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

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Отношение: имеет")]
    [XmlRoot]
    public class P_HasRelationVm : P_BaseVm
    {
        public P_HasRelationVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks)
        {
            CharacterId = null;
            relation = 0;
            isMore = false;
            isMoreOrEqual = false;
            isEqual = false;
            isLessOrEqual = false;
            isLess = false;
        }

        public P_HasRelationVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && Character != null && (isMore || isMoreOrEqual || isEqual || isLessOrEqual || isLess);

        public override bool IsConditionMet => throw new System.Exception(); ////// TODO

        public string CharacterId { get; set; }

        [XmlIgnore]
        public FolderedVm Character
        {
            get => Parent?.Parent?.Parent?.Parent?.NPCharacters.FirstOrDefault(item => item?.Id == CharacterId);
            set
            {
                if (CharacterId != value?.Id)
                {
                    CharacterId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected float relation;
        public float Relation
        {
            get => relation;
            set
            {
                if (relation != value)
                {
                    relation = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public bool isMore;
        public bool IsMore
        {
            get => isMore;
            set
            {
                if (isMore != value)
                {
                    isMore = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isMoreOrEqual;
        public bool IsMoreOrEqual
        {
            get => isMoreOrEqual;
            set
            {
                if (isMoreOrEqual != value)
                {
                    isMoreOrEqual = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isEqual;
        public bool IsEqual
        {
            get => isEqual;
            set
            {
                if (isEqual != value)
                {
                    isEqual = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isLessOrEqual;
        public bool IsLessOrEqual
        {
            get => isLessOrEqual;
            set
            {
                if (isLessOrEqual != value)
                {
                    isLessOrEqual = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isLess;
        public bool IsLess
        {
            get => isLess;
            set
            {
                if (isLess != value)
                {
                    isLess = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            Character = null;
            Relation = 0;
            IsMore = false;
            IsMoreOrEqual = false;
            IsEqual = false;
            IsLessOrEqual = false;
            IsLess = false;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasRelationVm casted)
            {
                casted.CharacterId = CharacterId;
                casted.relation = relation;
                casted.isMore = isMore;
                casted.isMoreOrEqual = isMoreOrEqual;
                casted.isEqual = isEqual;
                casted.isLessOrEqual = isLessOrEqual;
                casted.isLess = isLess;
            }
        }
    }
}