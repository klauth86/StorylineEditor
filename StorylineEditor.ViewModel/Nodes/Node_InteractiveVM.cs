/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Predicates;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_InteractiveVM<T> : Node_BaseVM<T> where T : Node_InteractiveM
    {
        public Node_InteractiveVM(T model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            Predicates = new ObservableCollection<Notifier>();
            foreach (var predicateModel in Model.predicates) Predicates.Add(PredicatesHelper.CreatePredicateByModel(predicateModel, CallbackContext));

            GameEvents = new ObservableCollection<Notifier>();
        }

        public Type SelectedPredicateType
        {
            get => null;
            set
            {
                if (value != null)
                {
                    Notifier viewModel = PredicatesHelper.CreatePredicateByType(value, CallbackContext);
                    if (viewModel is IWithModel withModel)
                    {
                        P_BaseM predicateModel = withModel.GetModel<P_BaseM>();
                        Model.predicates.Add(predicateModel);

                        Predicates.Add(viewModel);
                    }
                }

                Notify(nameof(SelectedPredicateType));
            }
        }

        public ObservableCollection<Notifier> Predicates { get; }

        public Type SelectedGameEventType
        {
            set
            {
                if (value != null)
                {
                    //if (value == typeof(GE_Item_DropM)) GameEvents.Add(new GE_Item_DropM(0));
                    //else if (value == typeof(GE_Item_PickUpM)) GameEvents.Add(new GE_Item_PickUpM(0));
                    //else if (value == typeof(GE_Quest_AddM)) GameEvents.Add(new GE_Quest_AddM(0));
                    //else if (value == typeof(GE_Quest_Node_AddM)) GameEvents.Add(new GE_Quest_Node_AddM(0));
                    //else if (value == typeof(GE_Quest_Node_PassM)) GameEvents.Add(new GE_Quest_Node_PassM(0));
                    //else if (value == typeof(GE_Relation_ChangeM)) GameEvents.Add(new GE_Relation_ChangeM(0));
                }
            }
        }

        public ObservableCollection<Notifier> GameEvents { get; }

        protected ICommand removeElementCommand;
        public ICommand RemoveElementCommand => removeElementCommand ?? (removeElementCommand = new RelayCommand<object>((obj) =>
        {
            if (obj is Notifier viewModel)
            {
                if (viewModel is IWithModel withModel)
                {
                    if (Predicates.Remove(viewModel))
                    {
                        Model.predicates.Remove(withModel.GetModel<P_BaseM>());
                    }
                    else if (GameEvents.Remove(viewModel))
                    {
                        Model.gameEvents.Remove(withModel.GetModel<GE_BaseM>());
                    }
                }
            }
        }));
    }

    public class Node_RandomVM : Node_InteractiveVM<Node_RandomM>
    {
        public Node_RandomVM(Node_RandomM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_RandomEditorVM : Node_RandomVM
    {
        public Node_RandomEditorVM(Node_RandomVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }

    public class Node_TransitVM : Node_InteractiveVM<Node_TransitM>
    {
        public Node_TransitVM(Node_TransitM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_TransitEditorVM : Node_TransitVM
    {
        public Node_TransitEditorVM(Node_TransitVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }
}