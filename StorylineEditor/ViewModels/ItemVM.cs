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
            hasDescriptionFemale = false;
            descriptionFemale = null;
            hasInternalDescription = false;
            internalDescription = null;
            hasInternalDescriptionFemale = false;
            internalDescriptionFemale = null;
        }

        public ItemVm() : this(null, 0) { }


        protected bool hasDescriptionFemale;
        public bool HasDescriptionFemale
        {
            get => hasDescriptionFemale;
            set
            {
                if (hasDescriptionFemale != value)
                {
                    hasDescriptionFemale = value;
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

        protected string internalDescription;
        public string InternalDescription
        {
            get => internalDescription;
            set
            {
                if (internalDescription != value)
                {
                    internalDescription = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool hasInternalDescriptionFemale;
        public bool HasInternalDescriptionFemale
        {
            get => hasInternalDescriptionFemale;
            set
            {
                if (hasInternalDescriptionFemale != value)
                {
                    hasInternalDescriptionFemale = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected string internalDescriptionFemale;
        public string InternalDescriptionFemale
        {
            get => internalDescriptionFemale;
            set
            {
                if (internalDescriptionFemale != value)
                {
                    internalDescriptionFemale = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is ItemVm casted)
            {
                casted.hasDescriptionFemale = hasDescriptionFemale;
                casted.descriptionFemale = descriptionFemale;
                casted.hasInternalDescription = hasInternalDescription;
                casted.internalDescription = internalDescription;
                casted.hasInternalDescriptionFemale = hasInternalDescriptionFemale;
                casted.internalDescriptionFemale = internalDescriptionFemale;
            }
        }
    }
}