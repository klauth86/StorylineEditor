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
                yield return typeof(DNode_VirtualVm);
            }
        }

        public PlayerDialogsTabVm(FullContextVm Parent) : base(Parent)
        {
            selectedNodeType = typeof(DNode_DialogVm);
        }

        public PlayerDialogsTabVm() : this(null) { }

        protected override string GetElementTitle(bool isCreate) => isCreate ? "Создать диалог" : "Редактировать диалог";

        [XmlIgnore]
        public bool HasManyRoots => false;
    }
}