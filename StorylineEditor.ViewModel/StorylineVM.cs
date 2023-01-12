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
    public class StorylineVM : SimpleVM<StorylineM, object>
    {
        public StorylineVM(StorylineM model) : base(model, null) { }



        private ICommand locationsTabCommand;
        public ICommand LocationsTabCommand => locationsTabCommand ?? (locationsTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.LocationsTab ?? (ActiveContext.LocationsTab = new CollectionVM(Model.locations, null,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new LocationM() { name = "Новая локация" }; },
                (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new LocationVM((LocationM)model, this); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new LocationEditorVM((LocationVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.LocationsTab));

        private ICommand charactersTabCommand;
        public ICommand CharactersTabCommand => charactersTabCommand ?? (charactersTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.CharactersTab ?? (ActiveContext.CharactersTab = new CollectionVM(Model.characters, null,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new CharacterM() { name = "Новый персонаж" }; },
                (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new CharacterVM((CharacterM)model, this); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new CharacterEditorVM((CharacterVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.CharactersTab));

        private ICommand itemsTabCommand;
        public ICommand ItemsTabCommand => itemsTabCommand ?? (itemsTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.ItemsTab ?? (ActiveContext.ItemsTab = new CollectionVM(Model.items, null,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ItemM() { name = "Новый предмет" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new ItemVM((ItemM)model, this); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new ItemEditorVM((ItemVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.ItemsTab));

        private ICommand actorsTabCommand;
        public ICommand ActorsTabCommand => actorsTabCommand ?? (actorsTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.ActorsTab ?? (ActiveContext.ActorsTab = new CollectionVM(Model.actors, null,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ActorM() { name = "Новый актор" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new ActorVM((ActorM)model, this); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new ActorEditorVM((ActorVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.ActorsTab));

        private ICommand journalTabCommand;
        public ICommand JournalTabCommand => journalTabCommand ?? (journalTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.JournalTab ?? (ActiveContext.JournalTab = new CollectionVM(Model.journal, null,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new QuestM() { name = "Новый квест" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new QuestVM((QuestM)model, this); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return CreateQuestEditorVM((QuestVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.JournalTab));

        private Notifier CreateQuestEditorVM(QuestVM inViewModel)
        {
            return new QuestEditorVM(
                inViewModel
                , this
                , new[]
                {
                    typeof(Node_StepM)
                    , typeof(Node_AlternativeM)
                }
                , (Type type, Point position) =>
                {
                    if (type == typeof(LinkM)) return new LinkM();
                    if (type == typeof(Node_StepM)) return new Node_StepM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_AlternativeM)) return new Node_AlternativeM() { positionX = position.X, positionY = position.Y };

                    throw new ArgumentOutOfRangeException(nameof(type));
                }
                , (BaseM model) =>
                {
                    if (model is LinkM modelLink) return new LinkVM(modelLink, this);
                    if (model is Node_StepM modelStep) return new Node_Journal_StepVM(modelStep, this);
                    if (model is Node_AlternativeM modelAlternative) return new Node_Journal_AlternativeVM(modelAlternative, this);

                    throw new ArgumentOutOfRangeException(nameof(model));
                }
                , (Notifier viewModel) =>
                {
                    if (viewModel is Node_Journal_StepVM viewModelStep) return new Node_Journal_StepEditorVM(viewModelStep);
                    if (viewModel is Node_Journal_AlternativeVM viewModelAlternative) return new Node_Journal_AlternativeEditorVM(viewModelAlternative);

                    throw new ArgumentOutOfRangeException(nameof(viewModel));
                }
            );
        }

        private ICommand dialogsTabCommand;
        public ICommand DialogsTabCommand => dialogsTabCommand ?? (dialogsTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.DialogsTab ?? (ActiveContext.DialogsTab = new CollectionVM(Model.dialogs, null,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new DialogM() { name = "Новый диалог" }; },
                (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new DialogVM((DialogM)model, this); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return CreateDialogEditorVM((DialogVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.DialogsTab));

        private Notifier CreateDialogEditorVM(DialogVM inViewModel)
        {
            return new DialogEditorVM(
                inViewModel
                , this
                , new[]
                {
                    typeof(Node_DialogM)
                    , typeof(Node_ReplicaM)
                    , typeof(Node_RandomM)
                    , typeof(Node_TransitM)
                    , typeof(Node_GateM)
                    , typeof(Node_ExitM)
                }
                , (Type type, Point position) =>
                {
                    if (type == typeof(LinkM)) return new LinkM();
                    if (type == typeof(Node_DialogM)) return new Node_DialogM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_ReplicaM)) return new Node_ReplicaM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_RandomM)) return new Node_RandomM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_TransitM)) return new Node_TransitM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_GateM)) return new Node_GateM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_ExitM)) return new Node_ExitM() { positionX = position.X, positionY = position.Y };

                    throw new ArgumentOutOfRangeException(nameof(type));
                }
                , (BaseM model) =>
                {
                    if (model is LinkM modelLink) return new LinkVM(modelLink, this);
                    if (model is Node_DialogM dialogModel) return new Node_DialogVM(dialogModel, this);
                    if (model is Node_ReplicaM replicaModel) return new Node_ReplicaVM(replicaModel, this);
                    if (model is Node_RandomM randomModel) return new Node_RandomVM(randomModel, this);
                    if (model is Node_TransitM transitModel) return new Node_TransitVM(transitModel, this);
                    if (model is Node_GateM gateModel) return new Node_GateVM(gateModel, this);
                    if (model is Node_ExitM exitModel) return new Node_ExitVM(exitModel, this);

                    throw new ArgumentOutOfRangeException(nameof(model));
                }
                , (Notifier viewModel) =>
                {
                    if (viewModel is Node_DialogVM dialogViewModel) return new Node_DialogEditorVM(dialogViewModel);
                    if (viewModel is Node_ReplicaVM replicaViewModel) return new Node_ReplicaEditorVM(replicaViewModel);
                    if (viewModel is Node_RandomVM randomViewModel) return new Node_RandomEditorVM(randomViewModel);
                    if (viewModel is Node_TransitVM transitViewModel) return new Node_TransitEditorVM(transitViewModel);
                    if (viewModel is Node_GateVM gateViewModel) return new Node_GateEditorVM(gateViewModel);
                    if (viewModel is Node_ExitVM exitViewModel) return new Node_ExitEditorVM(exitViewModel);

                    throw new ArgumentOutOfRangeException(nameof(viewModel));
                }
            );
        }

        private ICommand replicasTabCommand;
        public ICommand ReplicasTabCommand => replicasTabCommand ?? (replicasTabCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveTab = ActiveContext.ReplicasTab ?? (ActiveContext.ReplicasTab = new CollectionVM(Model.replicas, null,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ReplicaM() { name = "Новая реплика" }; },
                (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM, this); else return new ReplicaVM((ReplicaM)model, this); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return CreateReplicaEditorVM((ReplicaVM)viewModel); }));
        }, () => ActiveContext.ActiveTab == null || ActiveContext.ActiveTab != ActiveContext.ReplicasTab));

        private Notifier CreateReplicaEditorVM(ReplicaVM inViewModel)
        {
            return new ReplicaEditorVM(
                inViewModel
                , this
                , new[]
                {
                    typeof(Node_ReplicaM)
                    , typeof(Node_RandomM)
                    , typeof(Node_TransitM)
                }
                , (Type type, Point position) =>
                {
                    if (type == typeof(LinkM)) return new LinkM();
                    if (type == typeof(Node_ReplicaM)) return new Node_ReplicaM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_RandomM)) return new Node_RandomM() { positionX = position.X, positionY = position.Y };
                    if (type == typeof(Node_TransitM)) return new Node_TransitM() { positionX = position.X, positionY = position.Y };

                    throw new ArgumentOutOfRangeException(nameof(type));
                }
                , (BaseM model) =>
                {
                    if (model is LinkM modelLink) return new LinkVM(modelLink, this);
                    if (model is Node_ReplicaM replicaModel) return new Node_ReplicaVM(replicaModel, this);
                    if (model is Node_RandomM randomModel) return new Node_RandomVM(randomModel, this);
                    if (model is Node_TransitM transitModel) return new Node_TransitVM(transitModel, this);

                    throw new ArgumentOutOfRangeException(nameof(model));
                }
                , (Notifier viewModel) =>
                {
                    if (viewModel is Node_ReplicaVM replicaViewModel) return new Node_ReplicaEditorVM(replicaViewModel);
                    if (viewModel is Node_RandomVM randomViewModel) return new Node_RandomEditorVM(randomViewModel);
                    if (viewModel is Node_TransitVM transitViewModel) return new Node_TransitEditorVM(transitViewModel);

                    throw new ArgumentOutOfRangeException(nameof(viewModel));
                }
            );
        }

        private ICommand abstractCutCommand;
        public ICommand AbstractCutCommand => abstractCutCommand ?? (abstractCutCommand = new RelayCommand(() =>
        {

        }));

        private ICommand abstractCopyCommand;
        public ICommand AbstractCopyCommand => abstractCopyCommand ?? (abstractCopyCommand = new RelayCommand(() =>
        {
            Clipboard.SetText(ActiveContext.ActiveCopyPaste?.Copy());
        }));

        private ICommand abstractPasteCommand;
        public ICommand AbstractPasteCommand => abstractPasteCommand ?? (abstractPasteCommand = new RelayCommand(() =>
        {
            ActiveContext.ActiveCopyPaste?.Paste(Clipboard.GetText());
        }));

        private ICommand abstractDeleteCommand;
        public ICommand AbstractDeleteCommand => abstractDeleteCommand ?? (abstractDeleteCommand = new RelayCommand(() => { ActiveContext.ActiveCopyPaste?.Delete(); }));

        public override string Id => Model.id;
    }
}