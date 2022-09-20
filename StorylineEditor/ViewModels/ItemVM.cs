/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Tabs;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class ItemVm : NonFolderVm
    {
        public ItemVm(ItemsTabVm inParent, long additionalTicks) : base(inParent, additionalTicks)
        {
            hasGenderDescription = false;
            descriptionFemale = null;
            hasInternalDescription = false;
            descriptionInternal = null;
            hasGenderInternalDescription = false;
            descriptionInternalFemale = null;
        }

        public ItemVm() : this(null, 0) { }


        protected bool hasGenderDescription;
        public bool HasGenderDescription
        {
            get => hasGenderDescription;
            set
            {
                if (hasGenderDescription != value)
                {
                    hasGenderDescription = value;
                    NotifyWithCallerPropName();
                }
            }
        }


        protected string descriptionFemale;
        public string DescriptionFemale
        {
            get => descriptionFemale;
            set
            {
                if (descriptionFemale != value)
                {
                    descriptionFemale = value;
                    NotifyWithCallerPropName();
                }
            }
        }


        protected bool hasInternalDescription;
        public bool HasInternalDescription
        {
            get => hasInternalDescription;
            set
            {
                if (hasInternalDescription != value)
                {
                    hasInternalDescription = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected string descriptionInternal;
        public string DescriptionInternal
        {
            get => descriptionInternal;
            set
            {
                if (descriptionInternal != value)
                {
                    descriptionInternal = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool hasGenderInternalDescription;
        public bool HasGenderInternalDescription
        {
            get => hasGenderInternalDescription;
            set
            {
                if (hasGenderInternalDescription != value)
                {
                    hasGenderInternalDescription = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected string descriptionInternalFemale;
        public string DescriptionInternalFemale
        {
            get => descriptionInternalFemale;
            set
            {
                if (descriptionInternalFemale != value)
                {
                    descriptionInternalFemale = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is ItemVm casted)
            {
                casted.hasGenderDescription = hasGenderDescription;
                casted.descriptionFemale = descriptionFemale;
                casted.hasInternalDescription = hasInternalDescription;
                casted.descriptionInternal = descriptionInternal;
                casted.hasGenderInternalDescription = hasGenderInternalDescription;
                casted.descriptionInternalFemale = descriptionInternalFemale;
            }
        }
    }
}