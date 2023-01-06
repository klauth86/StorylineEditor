/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using System;

namespace StorylineEditor.ViewModel.Predicates
{
    public abstract class P_BaseVM<T, U>
        : BaseVM<T, U>
        where T : P_BaseM
        where U : class
    {
        public P_BaseVM(T model, U parent) : base(model, parent) { }

        public Type PredicateType => Model?.GetType();

        public bool IsInversed
        {
            get => Model.isInversed;
            set
            {
                if (Model.isInversed != value)
                {
                    Model.isInversed = value;
                    Notify(nameof(IsInversed));
                }
            
            }
        }
    }
}