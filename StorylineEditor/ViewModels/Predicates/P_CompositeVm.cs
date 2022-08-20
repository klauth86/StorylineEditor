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
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Составной")]
    [XmlRoot]
    public class P_CompositeVm : P_BaseVm
    {
        public P_CompositeVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks)
        {
            isOR = false;
            isAND = false;
            TypeA = null;
            TypeB = null;
        }

        public P_CompositeVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && itemA != null && itemB != null && (isOR || isAND);

        public bool isOR;
        public bool IsOR
        {
            get => isOR;
            set
            {
                if (isOR != value)
                {
                    isOR = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool isAND;
        public bool IsAND
        {
            get => isAND;
            set
            {
                if (isAND != value)
                {
                    isAND = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        [XmlIgnore]
        public Type TypeA
        {
            get
            {
                return itemA?.GetType();
            }
            set
            {
                ItemA = null;
                if (value != null) ItemA = CustomByteConverter.CreateByName(value.Name, Parent, 0) as P_BaseVm;
                NotifyWithCallerPropName();
            }
        }

        protected P_BaseVm itemA;
        public P_BaseVm ItemA
        {
            get => itemA;
            set
            {
                if (itemA != value)
                {
                    itemA = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        [XmlIgnore]
        public Type TypeB
        {
            get
            {
                return itemB?.GetType();
            }
            set
            {
                ItemB = null;
                if (value != null) ItemB = CustomByteConverter.CreateByName(value.Name, Parent, 0) as P_BaseVm;
                NotifyWithCallerPropName();
            }
        }

        protected P_BaseVm itemB;
        public P_BaseVm ItemB
        {
            get => itemB;
            set
            {
                if (itemB != value)
                {
                    itemB = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            IsOR = false;
            IsAND = false;
            TypeA = null;
            TypeB = null;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is P_CompositeVm casted)
            {
                casted.isOR = isOR;
                casted.isAND = isAND;
                casted.itemA = itemA?.Clone<P_BaseVm>(Parent, 0);
                casted.itemB = itemB?.Clone<P_BaseVm>(Parent, 1);
            }
        }
    }
}