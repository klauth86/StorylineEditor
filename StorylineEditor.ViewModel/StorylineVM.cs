/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.ViewModel.Common;
using System;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class StorylineVM : BaseVM<StorylineM>
    {
        public StorylineVM(StorylineM model) : base(model) { }



        private ICommand charactersTabCommand;
        public ICommand CharactersTabCommand => charactersTabCommand ?? (charactersTabCommand = new RelayCommand(() =>
        {
            Selection = new CollectionVM(Model.characters,
                (bool isFolder) => { if (isFolder) return new FolderM() { name = "Новая папка" }; else return new CharacterM() { name = "Новый персонаж" }; },
                (BaseM model) => { if (model is FolderM folderM) return new FolderVM(folderM); else return new CharacterVM((CharacterM)model); },
                (Notifier oldSel, Notifier newSel) => HandleSelection<CharacterVM>(oldSel, newSel, (Notifier sel) => new CharacterEditorVM(((CharacterVM)sel).Model)));
        }));

        private ICommand itemsTabCommand;
        public ICommand ItemsTabCommand => itemsTabCommand ?? (itemsTabCommand = new RelayCommand(() =>
        {
        }));

        private ICommand actorsTabCommand;
        public ICommand ActorsTabCommand => actorsTabCommand ?? (actorsTabCommand = new RelayCommand(() =>
        {
        }));

        private ICommand journalTabCommand;
        public ICommand JournalTabCommand => journalTabCommand ?? (journalTabCommand = new RelayCommand(() =>
        {

        }));

        private ICommand dialogsTabCommand;
        public ICommand DialogsTabCommand => dialogsTabCommand ?? (dialogsTabCommand = new RelayCommand(() =>
        {

        }));

        private ICommand replicasTabCommand;
        public ICommand ReplicasTabCommand => replicasTabCommand ?? (replicasTabCommand = new RelayCommand(() =>
        {

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



        private Notifier HandleSelection<T>(Notifier oldSelection, Notifier newSelection, Func<Notifier, Notifier> editorCreator) where T : class
        {
            if (oldSelection == null && newSelection == null) return null;

            if (oldSelection == null && newSelection != null ||
                oldSelection != null && newSelection == null ||
                oldSelection.GetType() != newSelection.GetType())
            {
                if (newSelection is FolderVM newFolderVM) return new FolderEditorVM(newFolderVM.Model);

                return editorCreator(newSelection);
            }

            if (oldSelection is FolderVM oldFolderVM)
            {
                FolderM newFolderM = ((FolderVM)newSelection).Model;

                if (oldFolderVM.Model == newFolderM) return oldSelection;

                return new FolderEditorVM(newFolderM);
            }

            if (((BaseVM<T>)oldSelection).Model == ((BaseVM<T>)newSelection).Model) return oldSelection;

            return editorCreator(newSelection);
        }
    }
}