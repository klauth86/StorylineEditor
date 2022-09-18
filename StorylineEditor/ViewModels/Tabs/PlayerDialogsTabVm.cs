/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels.Nodes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public class PlayerDialogsTabVm : BaseTreesTabVm
    {
        public IEnumerable<Type> NodeTypes
        {
            get
            {
                yield return typeof(DNode_DialogVm);
                yield return typeof(DNode_RandomVm);
                yield return typeof(DNode_TransitVm);
            }
        }

        public PlayerDialogsTabVm(FullContextVm Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            selectedNodeType = typeof(DNode_DialogVm);
        }

        public PlayerDialogsTabVm() : this(null, 0) { }

        public override FolderedVm CreateItem(object parameter)
        {
            if (parameter == FolderedVm.FolderFlag) return new TreeFolderVm(this, 0);

            return new TreeVm(this, 0) { Name = "Новый диалог" };
        }
    }
}