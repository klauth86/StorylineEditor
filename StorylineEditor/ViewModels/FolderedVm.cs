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
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels
{
    public abstract class FolderedVm : BaseVm<BaseVm<FullContextVm>>, IDragOverable
    {
        protected static object folderedFlag;
        public static object FolderFlag => folderedFlag ?? (folderedFlag = new object());

        public FolderedVm(BaseVm<FullContextVm> Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public override bool IsValid => base.IsValid && !string.IsNullOrEmpty(name);

        public override void NotifyNameChanged() { base.NotifyNameChanged(); Folder?.NotifyItemNameChanged(this); }

        [XmlIgnore]
        public FolderedVm Folder { get; set; }

        protected bool isDragOver;
        [XmlIgnore]
        public bool IsDragOver
        {
            get => isDragOver;
            set
            {
                if (value != isDragOver)
                {
                    isDragOver = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected bool isSelected;
        [XmlIgnore]
        public virtual bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    NotifyWithCallerPropName();

                    if (Parent is FolderedTabVm folderedTab)
                    {
                        if (value && folderedTab.SelectedItem != this)
                        {
                            if (folderedTab.SelectedItem != null)
                            {
                                folderedTab.SelectedItem.isSelected = false;
                            }

                            folderedTab.SelectedItem = this;
                        }
                        else if (!value && folderedTab.SelectedItem == this)
                        {
                            folderedTab.SelectedItem = null;
                        }
                    }
                }
            }
        }

        protected bool isExpanded;
        [XmlIgnore]
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public abstract bool IsFolder { get; }

        public abstract void AddChild(FolderedVm foldered);

        public abstract bool RemoveChild(FolderedVm foldered);

        public abstract bool IsContaining(FolderedVm foldered, bool checkSubs);

        public abstract IEnumerable<FolderedVm> FoldersTraversal();

        public abstract void SortItems();
    }

    public class NonFolderVm : FolderedVm
    {
        public NonFolderVm(BaseVm<FullContextVm> Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public override bool IsFolder => false;

        public override void AddChild(FolderedVm foldered) { throw new NotImplementedException(); }

        public override bool RemoveChild(FolderedVm foldered) { throw new NotImplementedException(); }

        public override bool IsContaining(FolderedVm foldered, bool checkSubs) => false;

        public override IEnumerable<FolderedVm> FoldersTraversal() { yield return this; }

        public override void SortItems() { }
    }
}