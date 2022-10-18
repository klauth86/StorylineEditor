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
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Graphs;
using System;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class StorylineVM : SimpleVM<StorylineM>
    {
        public StorylineVM(StorylineM model) : base(model) { }



        private ICommand charactersTabCommand;
        public ICommand CharactersTabCommand => charactersTabCommand ?? (charactersTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.characters,
                (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new CharacterM() { name = "Новый персонаж" }; },
                (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new CharacterVM((CharacterM)model); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return new CharacterEditorVM(((CharacterVM)viewModel).Model); },
                (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((CharacterVM)viewModel).Model; },
                (Notifier viewModel) => { });
        }));

        private ICommand itemsTabCommand;
        public ICommand ItemsTabCommand => itemsTabCommand ?? (itemsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.items,
            (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ItemM() { name = "Новый предмет" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ItemVM((ItemM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return new ItemEditorVM(((ItemVM)viewModel).Model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ItemVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private ICommand actorsTabCommand;
        public ICommand ActorsTabCommand => actorsTabCommand ?? (actorsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.actors,
            (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ActorM() { name = "Новый актор" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ActorVM((ActorM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return new ActorEditorVM(((ActorVM)viewModel).Model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ActorVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private ICommand journalTabCommand;
        public ICommand JournalTabCommand => journalTabCommand ?? (journalTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.journal,
            (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new QuestM() { name = "Новый квест" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new QuestVM((QuestM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return CreateQuestEditorVM(((QuestVM)viewModel).Model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((QuestVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private Notifier CreateQuestEditorVM(QuestM inModel)
        {
            return new QuestEditorVM(inModel,
            (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ActorM() { name = "Новый актор" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ActorVM((ActorM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return new ActorEditorVM(((ActorVM)viewModel).Model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((ActorVM)viewModel).Model; },
            (Type type) => { return ""; }
            );
        }

        private ICommand dialogsTabCommand;
        public ICommand DialogsTabCommand => dialogsTabCommand ?? (dialogsTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.dialogs,
            (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new DialogM() { name = "Новый диалог" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new DialogVM((DialogM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return new DialogEditorVM(((DialogVM)viewModel).Model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return folderVM.Model; else return ((DialogVM)viewModel).Model; },
            (Notifier viewModel) => { });
        }));

        private ICommand replicasTabCommand;
        public ICommand ReplicasTabCommand => replicasTabCommand ?? (replicasTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.replicas,
            (Type type) => { if (type == typeof(FolderM)) return new FolderM() { name = "Новая папка" }; else return new ReplicaM() { name = "Новая реплика" }; },
            (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new ReplicaVM((ReplicaM)model); },
            (Notifier viewModel) => { if (viewModel is FolderVM folderVM) return new FolderEditorVM(folderVM.Model); else return new ReplicaEditorVM(((ReplicaVM)viewModel).Model); },
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