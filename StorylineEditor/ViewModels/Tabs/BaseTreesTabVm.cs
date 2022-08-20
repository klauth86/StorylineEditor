/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.FileDialog;
using StorylineEditor.ViewModels.Nodes;
using System;
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public abstract class BaseTreesTabVm : FolderedTabVm
    {
        public BaseTreesTabVm(FullContextVm Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public BaseTreesTabVm() : this(null, 0) { }

        protected abstract string GetElementTitle(bool isCreate);

        public override FolderedVm CreateItem(object parameter)
        {
            BaseTreesTab_CreateElementVm createElement = new BaseTreesTab_CreateElementVm(Parent);

            if (parameter == FolderedVm.FolderFlag)
            {
                IDialogService.DialogService.CreateElement(createElement, "Создать папку");
                if (createElement.IsValid) return new TreeFolderVm(this, 0) { Name = createElement.Name, Description = createElement.Description };
            }
            else
            {
                IDialogService.DialogService.CreateElement(createElement, GetElementTitle(true));
                if (createElement.IsValid) return new TreeVm(this, 0) { Name = createElement.Name, Description = createElement.Description };
            }

            return null;
        }

        public override void EditItem(FolderedVm item)
        {
            BaseTreesTab_CreateElementVm editElement = new BaseTreesTab_CreateElementVm(Parent) { Name = item.Name, Description = item.Description };
            
            IDialogService.DialogService.CreateElement(editElement, item.IsFolder ? "Редактировать папку" : GetElementTitle(false));

            if (editElement != null && editElement.IsValid)
            {
                item.Name = editElement.Name;
                item.Description = editElement.Description;
            }
        }

        public Node_BaseVm CreateNode(TreeVm tree)
        {
            return selectedNodeType != null
                ? CustomByteConverter.CreateByName(selectedNodeType.Name, tree, 0) as Node_BaseVm
                : null;
        }

        protected Type selectedNodeType;
        [XmlIgnore]
        public Type SelectedNodeType
        {
            get => selectedNodeType;
            set
            {
                if (selectedNodeType != value)
                {
                    selectedNodeType = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected ICommand selectNodeTypeCommand;
        public ICommand SelectNodeTypeCommand => selectNodeTypeCommand ??
            (selectNodeTypeCommand = new RelayCommand<Type>((type) => SelectedNodeType = type, type => type != null));
    }
}