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
            descriptionFemale = null;
            descriptionInternal = null;
            FullContextVm.OnSearchFilterChangedEvent += OnSearchFilterChanged;
        }

        public ItemVm() : this(null, 0) { }


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


        public override bool OnRemoval() { FullContextVm.OnSearchFilterChangedEvent -= OnSearchFilterChanged; return base.OnRemoval(); }


        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is ItemVm casted)
            {
                casted.descriptionFemale = descriptionFemale;
                casted.descriptionInternal = descriptionInternal;
            }
        }
    }
}