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
using System.Windows.Input;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [XmlRoot]
    public abstract class GE_BaseVm : BaseVm<Node_BaseVm>
    {
        public GE_BaseVm(Node_BaseVm inParent) : base(inParent)
        {
            executeWhenLeaveDialogNode = false;

            ResetName();
        }

        public GE_BaseVm() : this(null) { }

        protected bool executeWhenLeaveDialogNode;
        public bool ExecuteWhenLeaveDialogNode
        {
            get => executeWhenLeaveDialogNode;
            set
            {
                if (executeWhenLeaveDialogNode != value)
                {
                    executeWhenLeaveDialogNode = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        ICommand resetCommand;
        public ICommand ResetCommand => resetCommand ?? (resetCommand = new RelayCommand(() => ResetInternalData()));

        protected virtual void ResetInternalData() { }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_BaseVm casted)
            {
                casted.executeWhenLeaveDialogNode = executeWhenLeaveDialogNode;
            }
        }

        public virtual string GenerateCode(string eventName, string outerName) { throw new NotImplementedException(); }

        public void ResetName()
        {
            if (string.IsNullOrEmpty(Name)) Name = (GetType().GetCustomAttributes(true)[0] as DescriptionAttribute).Description;
        }
    }
}
