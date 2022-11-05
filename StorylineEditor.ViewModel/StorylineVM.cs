/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Graphs;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Windows;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class StorylineVM : SimpleVM<StorylineM>
    {
        private static double viewWidth;
        public static double ViewWidth { get => viewWidth; set => viewWidth = value > 0 ? value : viewWidth; }

        private static double viewHeight;
        public static double ViewHeight { get => viewHeight; set => viewHeight = value > 0 ? value : viewHeight; }



        public StorylineVM(StorylineM model) : base(model, null) { }



        private ICommand charactersTabCommand;
        public ICommand CharactersTabCommand => charactersTabCommand ?? (charactersTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.characters,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new CharacterM() { name = "Новый персонаж" }; },
                (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM, callbackContext); else return new CharacterVM((CharacterM)model, callbackContext); },
                (Notifier viewModel, ICallbackContext callbackContext) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM, callbackContext); else return new CharacterEditorVM((CharacterVM)viewModel, callbackContext); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((CharacterVM)viewModel).Model; },
                (Notifier viewModel) => { });
            SelectionModel = Model.characters;
        }, () => SelectionModel != Model.characters));

        private ICommand itemsTabCommand;
        public ICommand ItemsTabCommand => itemsTabCommand ?? (itemsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.items,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ItemM() { name = "Новый предмет" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM, callbackContext); else return new ItemVM((ItemM)model, callbackContext); },
            (Notifier viewModel, ICallbackContext callbackContext) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM, callbackContext); else return new ItemEditorVM((ItemVM)viewModel, callbackContext); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ItemVM)viewModel).Model; },
            (Notifier viewModel) => { });
            SelectionModel = Model.items;
        }, () => SelectionModel != Model.items));

        private ICommand actorsTabCommand;
        public ICommand ActorsTabCommand => actorsTabCommand ?? (actorsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.actors,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ActorM() { name = "Новый актор" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM, callbackContext); else return new ActorVM((ActorM)model, callbackContext); },
            (Notifier viewModel, ICallbackContext callbackContext) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM, callbackContext); else return new ActorEditorVM((ActorVM)viewModel, callbackContext); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ActorVM)viewModel).Model; },
            (Notifier viewModel) => { });
            SelectionModel = Model.actors;
        }, () => SelectionModel != Model.actors));

        private ICommand journalTabCommand;
        public ICommand JournalTabCommand => journalTabCommand ?? (journalTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.journal,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new QuestM() { name = "Новый квест" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM, callbackContext); else return new QuestVM((QuestM)model, callbackContext); },
            (Notifier viewModel, ICallbackContext callbackContext) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM, callbackContext); else return CreateQuestEditorVM((QuestVM)viewModel, callbackContext); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((QuestVM)viewModel).Model; },
            (Notifier viewModel) => { });
            SelectionModel = Model.journal;
        }, () => SelectionModel != Model.journal));

        private Notifier CreateQuestEditorVM(QuestVM inViewModel, ICallbackContext outerCallbackContext)
        {
            return new QuestEditorVM(inViewModel, outerCallbackContext,
            (Type type, Point position) =>
            {
                if (type == typeof(LinkM)) return new LinkM();
                if (type == typeof(Node_StepM)) return new Node_StepM() { positionX = position.X, positionY = position.Y };
                if (type == typeof(Node_AlternativeM)) return new Node_AlternativeM() { positionX = position.X, positionY = position.Y };

                throw new ArgumentOutOfRangeException(nameof(type));
            },
            (BaseM model, ICallbackContext callbackContext) =>
            {
                if (model is LinkM modelLink) return new LinkVM(modelLink, callbackContext);
                if (model is Node_StepM modelStep) return new Node_Journal_StepVM(modelStep, callbackContext);
                if (model is Node_AlternativeM modelAlternative) return new Node_Journal_AlternativeVM(modelAlternative, callbackContext);

                throw new ArgumentOutOfRangeException(nameof(model));
            },
            (Notifier viewModel, ICallbackContext callbackContext) =>
            {
                if (viewModel is Node_Journal_StepVM viewModelStep) return new Node_Journal_StepEditorVM(viewModelStep);
                if (viewModel is Node_Journal_AlternativeVM viewModelAlternative) return new Node_Journal_AlternativeEditorVM(viewModelAlternative);

                throw new ArgumentOutOfRangeException(nameof(viewModel));
            },
            (Notifier viewModel) =>
            {
                if (viewModel is LinkVM viewModelLink) return viewModelLink.Model;
                if (viewModel is Node_Journal_StepVM viewModelStep) return viewModelStep.Model;
                if (viewModel is Node_Journal_AlternativeVM viewModelAlternative) return viewModelAlternative.Model;

                throw new ArgumentOutOfRangeException(nameof(viewModel));
            },
            typeof(Node_StepM),
            (Type type) =>
            {
                if (type == typeof(Node_StepM)) return "Шаг";
                if (type == typeof(Node_AlternativeM)) return "Альтернатива";

                throw new ArgumentOutOfRangeException(nameof(type));
            }
            );
        }

        private ICommand dialogsTabCommand;
        public ICommand DialogsTabCommand => dialogsTabCommand ?? (dialogsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.dialogs,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new DialogM() { name = "Новый диалог" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM, callbackContext); else return new DialogVM((DialogM)model, callbackContext); },
            (Notifier viewModel, ICallbackContext callbackContext) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM, callbackContext); else return new DialogEditorVM((DialogVM)viewModel, callbackContext); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((DialogVM)viewModel).Model; },
            (Notifier viewModel) => { });
            SelectionModel = Model.dialogs;
        }, () => SelectionModel != Model.dialogs));

        private ICommand replicasTabCommand;
        public ICommand ReplicasTabCommand => replicasTabCommand ?? (replicasTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.replicas,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ReplicaM() { name = "Новый реплика" }; },
                (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM, callbackContext); else return new ReplicaVM((ReplicaM)model, callbackContext); },
                (Notifier viewModel, ICallbackContext callbackContext) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM, callbackContext); else return CreateReplicaEditorVM((ReplicaVM)viewModel, callbackContext); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ReplicaVM)viewModel).Model; },
                (Notifier viewModel) => { });
            SelectionModel = Model.replicas;
        }, () => SelectionModel != Model.replicas));

        private Notifier CreateReplicaEditorVM(ReplicaVM inViewModel, ICallbackContext outerCallbackContext)
        {
            return new ReplicaEditorVM(inViewModel, outerCallbackContext,
            (Type type, Point position) =>
            {
                if (type == typeof(LinkM)) return new LinkM();
                if (type == typeof(Node_ReplicaM)) return new Node_ReplicaM() { positionX = position.X, positionY = position.Y };
                if (type == typeof(Node_RandomM)) return new Node_RandomM() { positionX = position.X, positionY = position.Y };
                if (type == typeof(Node_TransitM)) return new Node_TransitM() { positionX = position.X, positionY = position.Y };

                throw new ArgumentOutOfRangeException(nameof(type));
            },
            (BaseM model, ICallbackContext callbackContext) =>
            {
                if (model is LinkM modelLink) return new LinkVM(modelLink, callbackContext);
                if (model is Node_ReplicaM replicaModel) return new Node_ReplicaVM(replicaModel, callbackContext);
                if (model is Node_RandomM randomModel) return new Node_RandomVM(randomModel, callbackContext);
                if (model is Node_TransitM transitModel) return new Node_TransitVM(transitModel, callbackContext);

                throw new ArgumentOutOfRangeException(nameof(model));
            },
            (Notifier viewModel, ICallbackContext callbackContext) =>
            {
                if (viewModel is Node_ReplicaVM replicaViewModel) return new Node_ReplicaEditorVM(replicaViewModel);
                if (viewModel is Node_RandomVM randomViewModel) return new Node_RandomEditorVM(randomViewModel);
                if (viewModel is Node_TransitVM transitViewModel) return new Node_TransitEditorVM(transitViewModel);

                throw new ArgumentOutOfRangeException(nameof(viewModel));
            },
            (Notifier viewModel) =>
            {
                if (viewModel is LinkVM viewModelLink) return viewModelLink.Model;
                if (viewModel is Node_ReplicaVM replicaViewModel) return replicaViewModel.Model;
                if (viewModel is Node_RandomVM randomViewModel) return randomViewModel.Model;
                if (viewModel is Node_TransitVM transitViewModel) return transitViewModel.Model;

                throw new ArgumentOutOfRangeException(nameof(viewModel));
            },
            typeof(Node_ReplicaVM),
            (Type type) =>
            {
                if (type == typeof(Node_StepM)) return "Шаг";
                if (type == typeof(Node_AlternativeM)) return "Альтернатива";

                throw new ArgumentOutOfRangeException(nameof(type));
            }
            );
        }

        public override string Id => Model.id;

        private object selectionModel;
        public object SelectionModel
        {
            get => selectionModel;
            set
            {
                if (value != selectionModel)
                {
                    selectionModel = value;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private object selection;
        public object Selection
        {
            get => selection;
            set
            {
                if (value != selection)
                {
                    selection = value;
                    Notify(nameof(Selection));
                }
            }
        }

        private ICommand abstractCopyCommand;
        public ICommand AbstractCopyCommand => abstractCopyCommand ?? (abstractCopyCommand = new RelayCommand(() =>
        {
            Clipboard.SetText(ActiveContextService.ActiveContext?.Copy());
        }));

        private ICommand abstractPasteCommand;
        public ICommand AbstractPasteCommand => abstractPasteCommand ?? (abstractPasteCommand = new RelayCommand(() =>
        {
            ActiveContextService.ActiveContext?.Paste(Clipboard.GetText());
        }));

        private ICommand abstractDeleteCommand;
        public ICommand AbstractDeleteCommand => abstractDeleteCommand ?? (abstractDeleteCommand = new RelayCommand(() => { ActiveContextService.ActiveContext?.Delete(); }));
    }
}