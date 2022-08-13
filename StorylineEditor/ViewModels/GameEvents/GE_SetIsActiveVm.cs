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
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Установить активность")]
    [XmlRoot]
    public class GE_SetIsActiveVm : GE_BaseVm
    {
        public GE_SetIsActiveVm(Node_BaseVm inParent) : base(inParent)
        {
            ObjectWithActivationId = null;
            searchByName = false;
            affectAll = false;
            isActive = false;
        }

        public GE_SetIsActiveVm() : this(null) { }

        public override bool IsValid => base.IsValid && ObjectWithActivation != null;

        public string ObjectWithActivationId { get; set; }

        [XmlIgnore]
        public BaseVm ObjectWithActivation
        {
            get
            {
                return Parent.Parent.Parent.Parent.ObjectsWithActivation
                  .FirstOrDefault(item => item?.Id == ObjectWithActivationId);
            }
            set
            {
                if (ObjectWithActivationId != value?.Id)
                {
                    ObjectWithActivationId = value?.Id;
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

        protected bool isActive;
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            ObjectWithActivation = null;
            SearchByName = false;
            AffectAll = false;
            IsActive = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_SetIsActiveVm casted)
            {
                casted.ObjectWithActivationId = ObjectWithActivationId;
                casted.searchByName = searchByName;
                casted.affectAll = affectAll;
                casted.isActive = isActive;
            }
        }
    }
}