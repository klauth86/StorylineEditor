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
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public abstract class BaseTreesTabVm : FolderedTabVm
    {
        public BaseTreesTabVm(FullContextVm Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public BaseTreesTabVm() : this(null, 0) { }

        protected abstract string GetItemDefaultName();

        public override FolderedVm CreateItem(object parameter)
        {
            if (parameter == FolderedVm.FolderFlag) return new TreeFolderVm(this, 0) { Name = "Новая папка" };

            return new TreeVm(this, 0) { Name = GetItemDefaultName() };
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

        public virtual bool EditItemInPlace => true;
    }
}