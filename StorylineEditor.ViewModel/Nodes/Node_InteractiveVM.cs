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
            foreach (var gameEventModel in Model.gameEvents) GameEvents.Add(PredicatesHelper.CreateGameEventByModel(gameEventModel, CallbackContext));
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
                        OnModelChanged(Model, nameof(HasPredicates));
                    }
                }

                Notify(nameof(SelectedPredicateType));
            }
        }
        public ObservableCollection<Notifier> Predicates { get; }
        public bool HasPredicates => Model.predicates.Count > 0;

        public Type SelectedGameEventType
        {
            set
            {
                if (value != null)
                {
                    Notifier viewModel = PredicatesHelper.CreateGameEventByType(value, CallbackContext);
                    if (viewModel is IWithModel withModel)
                    {
                        GE_BaseM gameEventModel = withModel.GetModel<GE_BaseM>();
                        Model.gameEvents.Add(gameEventModel);

                        GameEvents.Add(viewModel);
                        OnModelChanged(Model, nameof(HasGameEvents));
                    }
                }

                Notify(nameof(SelectedGameEventType));
            }
        }
        public ObservableCollection<Notifier> GameEvents { get; }
        public bool HasGameEvents => Model.gameEvents.Count > 0;


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
                        OnModelChanged(Model, nameof(HasPredicates));
                    }
                    else if (GameEvents.Remove(viewModel))
                    {
                        Model.gameEvents.Remove(withModel.GetModel<GE_BaseM>());
                        OnModelChanged(Model, nameof(HasGameEvents));
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

    public class Node_GateVM : Node_InteractiveVM<Node_GateM>
    {
        public Node_GateVM(Node_GateM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_GateEditorVM : Node_GateVM
    {
        public Node_GateEditorVM(Node_GateVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }

    public class Node_ExitVM : Node_BaseVM<Node_ExitM>
    {
        public Node_ExitVM(Node_ExitM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_ExitEditorVM : Node_ExitVM
    {
        public Node_ExitEditorVM(Node_ExitVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }
}