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
    [Description("Диалог: имеет вершину в активной сессии <>")]
    [XmlRoot]
    public class P_HasActiveDialogNodeMoreLessVm : P_BaseVm
    {
        public P_HasActiveDialogNodeMoreLessVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            num = 0;
            isMore = false;
            isMoreOrEqual = false;
            isEqual = false;
            isLessOrEqual = false;
            isLess = false;
        }

        public P_HasActiveDialogNodeMoreLessVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var newP = new P_Dialog_Node_Has_ActiveSession_CmpM(ticks);
            model = newP;

            var times = id.Replace(GetType().Name + "_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            idReplacer.Add(id, model.id);

            newP.name = Name;
            newP.description = Description;
            newP.isInversed = IsInversed;
            newP.nodeId = DialogNode?.GetModel(ticks, idReplacer)?.id;
            newP.compareType = GetCompareType();
            newP.value = num;

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

        public override bool IsValid=> base.IsValid && DialogNode != null && (isMore || isMoreOrEqual || isEqual || isLessOrEqual || isLess);

        public override bool IsConditionMet => !IsValid ||
            !isInversed && NumericCondition(Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.FirstOrDefault((treePath) => treePath?.Tree?.Id == Parent.Parent.Id && treePath.IsActive)?.PassedNodes.Count((node) => node == DialogNode) ?? 0) ||
            isInversed && !NumericCondition(Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.FirstOrDefault((treePath) => treePath?.Tree?.Id == Parent.Parent.Id && treePath.IsActive)?.PassedNodes.Count((node) => node == DialogNode) ?? 0);

        private bool NumericCondition(int num)
        {
            if (IsMore) return num > Num;
            if (IsMoreOrEqual) return num >= Num;
            if (IsEqual) return num == Num;
            if (IsLessOrEqual) return num <= Num;
            if (IsLess) return num < Num;

            return false;
        }

        public string DialogNodeId { get; set; }

        [XmlIgnore]
        public Node_BaseVm DialogNode
        {
            get => Parent.Parent.Nodes.FirstOrDefault(item => item?.Id == DialogNodeId);
            set
            {
                if (DialogNodeId != value?.Id)
                {
                    DialogNodeId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public int num;
        public int Num
        {
            get => num;
            set
            {
                if (num != value)
                {
                    num = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
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

            DialogNode = null;
            Num = 0;
            IsMore = false;
            IsMoreOrEqual = false;
            IsEqual = false;
            IsLessOrEqual = false;
            IsLess = false;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasActiveDialogNodeMoreLessVm casted)
            {
                casted.DialogNodeId = DialogNodeId;
                casted.num = num;
                casted.isMore = isMore;
                casted.isMoreOrEqual = isMoreOrEqual;
                casted.isEqual = isEqual;
                casted.isLessOrEqual = isLessOrEqual;
                casted.isLess = isLess;
            }
        }
    }
}