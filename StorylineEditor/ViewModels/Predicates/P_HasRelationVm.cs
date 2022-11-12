/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Model;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModels.Nodes;
using System.Collections.Generic;
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

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var newP = new P_Relation_HasM(ticks);
            model = newP;

            var times = id.Replace(GetType().Name + "_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            idReplacer.Add(id, model.id);

            newP.name = Name;
            newP.description = Description;
            newP.isInversed = IsInversed;
            newP.npcId = Character?.GetModel(ticks, idReplacer)?.id;
            newP.compareType = GetCompareType();
            newP.value = Relation;

            return model;
        }

        protected byte GetCompareType()
        {
            if (IsMore) return COMPARE_TYPE.GREATER;
            if (IsMoreOrEqual) return COMPARE_TYPE.EQUAL_OR_GREATER;
            if (IsEqual) return COMPARE_TYPE.EQUAL;
            if (IsLessOrEqual) return COMPARE_TYPE.LESS_OR_EQUAL;
            if (IsLess) return COMPARE_TYPE.LESS;

            return COMPARE_TYPE.UNSET;
        }

        public override bool IsValid => base.IsValid && Character != null && (isMore || isMoreOrEqual || isEqual || isLessOrEqual || isLess);

        public override bool IsConditionMet => !IsValid ||
            !isInversed && NumericCondition(Parent.Parent.Parent.Parent.TreePlayerHistory.GetRelation(Character)) ||
            isInversed && !NumericCondition(Parent.Parent.Parent.Parent.TreePlayerHistory.GetRelation(Character));

        private bool NumericCondition(float relation)
        {
            if (IsMore) return relation > Relation;
            if (IsMoreOrEqual) return relation >= Relation;
            if (IsEqual) return relation == Relation;
            if (IsLessOrEqual) return relation <= Relation;
            if (IsLess) return relation < Relation;

            return false;
        }

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