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
    public class P_CompositeVM : P_BaseVM<P_CompositeM>
    {
        public P_CompositeVM(P_CompositeM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        Type typeA;
        public Type TypeA
        {
            get => typeA;
            set
            {
                if (typeA != value)
                {
                    typeA = value;
                    predicateA = null;
                    Notify(nameof(PredicateA));
                }
            }
        }

        Notifier predicateA;
        public Notifier PredicateA => predicateA ?? (predicateA = (typeA != null ? PredicatesHelper.CreatePredicateByType(typeA, CallbackContext) : null));

        Type typeB;
        public Type TypeB
        {
            get => typeB;
            set
            {
                if (typeB != value)
                {
                    typeB = value;
                    predicateB = null;
                    Notify(nameof(PredicateB));
                }
            }
        }

        Notifier predicateB;
        public Notifier PredicateB => predicateB ?? (predicateB = (typeB != null ? PredicatesHelper.CreatePredicateByType(typeB, CallbackContext) : null));

        public byte CompositionType
        {
            get => Model.compositionType;
            set
            {
                if (value != Model.compositionType)
                {
                    Model.compositionType = value;
                    Notify(nameof(CompositionType));
                }
            }
        }
    }
}