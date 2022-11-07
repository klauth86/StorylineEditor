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
using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_InteractiveVM<T> : Node_BaseVM<T> where T : Node_InteractiveM
    {
        public Node_InteractiveVM(T model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        public Type SelectedPredicateType
        {
            set
            {
                if (value != null)
                {
                    if (value == typeof(P_CompositeM)) Predicates.Add(new P_CompositeM(0));
                    else if (value == typeof(P_Dialog_HasM)) Predicates.Add(new P_Dialog_HasM(0));
                    else if (value == typeof(P_Dialog_Node_Has_ActiveSession_CmpM)) Predicates.Add(new P_Dialog_Node_Has_ActiveSession_CmpM(0));
                    else if (value == typeof(P_Dialog_Node_Has_ActiveSessionM)) Predicates.Add(new P_Dialog_Node_Has_ActiveSessionM(0));
                    else if (value == typeof(P_Dialog_Node_Has_PrevSessions_CmpM)) Predicates.Add(new P_Dialog_Node_Has_PrevSessions_CmpM(0));
                    else if (value == typeof(P_Dialog_Node_Has_PrevSessionsM)) Predicates.Add(new P_Dialog_Node_Has_PrevSessionsM(0));
                    else if (value == typeof(P_Item_HasM)) Predicates.Add(new P_Item_HasM(0));
                    else if (value == typeof(P_Quest_AddedM)) Predicates.Add(new P_Quest_AddedM(0));
                    else if (value == typeof(P_Quest_FinishedM)) Predicates.Add(new P_Quest_FinishedM(0));
                    else if (value == typeof(P_Quest_Node_AddedM)) Predicates.Add(new P_Quest_Node_AddedM(0));
                    else if (value == typeof(P_Quest_Node_PassedM)) Predicates.Add(new P_Quest_Node_PassedM(0));
                    else if (value == typeof(P_Relation_HasM)) Predicates.Add(new P_Relation_HasM(0));

                    CollectionViewSource.GetDefaultView(Predicates)?.Refresh();
                }
            }      
        }

        public List<P_BaseM> Predicates => Model.predicates;

        public Type SelectedGameEventType
        {
            set
            {
                if (value != null)
                {
                    if (value == typeof(GE_Item_DropM)) GameEvents.Add(new GE_Item_DropM(0));
                    else if (value == typeof(GE_Item_PickUpM)) GameEvents.Add(new GE_Item_PickUpM(0));
                    else if (value == typeof(GE_Quest_AddM)) GameEvents.Add(new GE_Quest_AddM(0));
                    else if (value == typeof(GE_Quest_Node_AddM)) GameEvents.Add(new GE_Quest_Node_AddM(0));
                    else if (value == typeof(GE_Quest_Node_PassM)) GameEvents.Add(new GE_Quest_Node_PassM(0));
                    else if (value == typeof(GE_Relation_ChangeM)) GameEvents.Add(new GE_Relation_ChangeM(0));

                    CollectionViewSource.GetDefaultView(GameEvents)?.Refresh();
                }
            }
        }

        public List<GE_BaseM> GameEvents => Model.gameEvents;

        protected ICommand removeElementCommand;
        public ICommand RemoveElementCommand => removeElementCommand ?? (removeElementCommand = new RelayCommand<object>((model) =>
        {
            if (model is P_BaseM predicate)
            {
                Predicates.Add(predicate);
                CollectionViewSource.GetDefaultView(Predicates)?.Refresh();
            }
            else if (model is GE_BaseM gameEventModel)
            {
                GameEvents.Add(gameEventModel);
                CollectionViewSource.GetDefaultView(GameEvents)?.Refresh();
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