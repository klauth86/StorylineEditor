/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.GameEvents
{
    public abstract class GE_BaseVM<T, U>
        : BaseVM<T, U>
        , IGameEvent
        where T : GE_BaseM
        where U : class
    {
        public GE_BaseVM(T model, U parent) : base(model, parent) { }

        public Type GameEventType => Model?.GetType();

        public byte ExecutionMode
        {
            get => Model.executionMode;
            set
            {
                if (Model.executionMode != value)
                {
                    Model.executionMode = value;
                    Notify(nameof(ExecutionMode));
                }

            }
        }

        public abstract void Execute();
    }
}