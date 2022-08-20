﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Tabs;
using System.Windows.Media;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class JournalTagVm : BaseNamedVm<BaseVm<FullContextVm>>
    {
        public JournalTagVm(GlobalTagsTabVm inParent, long additionalTicks) : base(inParent, additionalTicks) { tagCategory = "NONE"; }

        public JournalTagVm() : this(null, 0) { }

        protected string tagCategory;
        public string TagCategory
        {
            get => tagCategory;
            set
            {
                if (tagCategory != value)
                {
                    tagCategory = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected Color? tagColor;
        public Color? TagColor
        {
            get => tagColor;
            set {
                if (tagColor != value) {
                    tagColor = value;
                    NotifyWithCallerPropName();
                }
            }
        }
    }
}