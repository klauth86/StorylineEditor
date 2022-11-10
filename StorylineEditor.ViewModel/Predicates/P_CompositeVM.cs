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
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_CompositeVM : P_BaseVM<P_CompositeM>
    {
        public P_CompositeVM(P_CompositeM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            IsFirstSelected = true;
        }

        Type subType;
        public Type SubType
        {
            get => subType;
            set
            {
                if (subType != value)
                {
                    subType = value;

                    if (value != null)
                    {
                        if (IsFirstSelected)
                        {
                            PredicateA = PredicatesHelper.CreatePredicateByType(subType, CallbackContext);
                            Model.predicateA = ((IWithModel)PredicateA).GetModel<P_BaseM>();
                            Notify(nameof(PredicateA));
                        }
                        else
                        {
                            PredicateB = PredicatesHelper.CreatePredicateByType(subType, CallbackContext);
                            Model.predicateB = ((IWithModel)PredicateB).GetModel<P_BaseM>();
                            Notify(nameof(PredicateB));
                        }
                    }
                }
            }
        }

        public bool IsFirstSelected { get; set; }

        public Notifier PredicateA { get; set; }

        public Notifier PredicateB { get; set; }

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


        ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<bool>((isFirstSelected) =>
        {
            IsFirstSelected = isFirstSelected;
            Notify(nameof(IsFirstSelected));

            subType = IsFirstSelected ? Model.predicateA?.GetType() : Model.predicateB?.GetType();
            Notify(nameof(SubType));
        }));
    }
}