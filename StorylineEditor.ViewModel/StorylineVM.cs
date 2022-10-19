﻿/*
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
        public StorylineVM(StorylineM model) : base(model, null) { }



        private ICommand charactersTabCommand;
        public ICommand CharactersTabCommand => charactersTabCommand ?? (charactersTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.characters,
                (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new CharacterM() { name = "Новый персонаж" }; },
                (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new CharacterVM((CharacterM)model); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new CharacterEditorVM(((CharacterVM)viewModel)); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((CharacterVM)viewModel).Model; },
                (Notifier viewModel) => { });
        }));

        private ICommand itemsTabCommand;
        public ICommand ItemsTabCommand => itemsTabCommand ?? (itemsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.items,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ItemM() { name = "Новый предмет" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ItemVM((ItemM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new ItemEditorVM(((ItemVM)viewModel)); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ItemVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private ICommand actorsTabCommand;
        public ICommand ActorsTabCommand => actorsTabCommand ?? (actorsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.actors,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ActorM() { name = "Новый актор" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ActorVM((ActorM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new ActorEditorVM(((ActorVM)viewModel)); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ActorVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private ICommand journalTabCommand;
        public ICommand JournalTabCommand => journalTabCommand ?? (journalTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.journal,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new QuestM() { name = "Новый квест" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new QuestVM((QuestM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return CreateQuestEditorVM(((QuestVM)viewModel)); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((QuestVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private Notifier CreateQuestEditorVM(QuestVM inViewModel)
        {
            return new QuestEditorVM(inViewModel,
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
            (Notifier viewModel) =>
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
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new DialogVM((DialogM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new DialogEditorVM(((DialogVM)viewModel)); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((DialogVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private ICommand replicasTabCommand;
        public ICommand ReplicasTabCommand => replicasTabCommand ?? (replicasTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.replicas,
            (Type type, object param) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ReplicaM() { name = "Новая реплика" }; },
            (BaseM model, ICallbackContext callbackContext) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ReplicaVM((ReplicaM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM); else return new ReplicaEditorVM(((ReplicaVM)viewModel)); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ReplicaVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));



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
    }
}