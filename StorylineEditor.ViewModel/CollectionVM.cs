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
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class CollectionVM : Collection_BaseVM<List<BaseM>, object>, ICallbackContext
    {
        public CollectionVM(List<BaseM> inModel, ICallbackContext callbackContext, Func<Type, object, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
            Func<Notifier, ICallbackContext, Notifier> editorCreator) : base(inModel, callbackContext, modelCreator, viewModelCreator,
                editorCreator)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsVMs);

            if (view != null)
            {
                view.SortDescriptions.Add(new SortDescription(nameof(IsFolder), ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Ascending));
            }

            Context = new ObservableCollection<FolderM>();
            Context.Add(new FolderM() { name = "root", content = inModel });

            foreach (var model in Model) { Add(null, _viewModelCreator(model, this)); }
        }

        private ICommand addCommand;
        public override ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM model = _modelCreator(isFolder ? typeof(FolderM) : null, null);
            Notifier viewModel = _viewModelCreator(model, this);

            Add(model, viewModel);

            AddToSelection(viewModel, true);
        }));

        private ICommand selectCommand;
        public override ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((viewModel) => AddToSelection(viewModel, true), (viewModel) => viewModel != null));

        private ICommand setContextCommand;
        public ICommand SetContextCommand => setContextCommand ?? (setContextCommand = new RelayCommand<FolderM>((folderM) =>
        {
            if (Context.Count > 0 && Context[Context.Count - 1] == folderM) return;

            int cutIndex = -1;

            foreach (var contextEntry in Context)
            {
                cutIndex++;
                if (contextEntry == folderM) break;
            }

            if (cutIndex >= 0 && cutIndex + 1 < Context.Count)
            {
                for (int i = Context.Count - 1; i > cutIndex; i--) Context.RemoveAt(i);
            }
            else
            {
                Context.Add(folderM);
            }

            RefreshItemsVMs();

        }, (folderM) => folderM != null));

        protected void RefreshItemsVMs()
        {
            AddToSelection(null, true);

            ItemsVMs.Clear();

            HashSet<string> cutViewModels = new HashSet<string>();
            foreach (var cutEntryVM in CutVMs) cutViewModels.Add(cutEntryVM.ViewModel.Id);

            foreach (var model in Context.Last().content)
            {
                Notifier viewModel = _viewModelCreator(model, this);
                viewModel.IsCut = cutViewModels.Contains(viewModel.Id);
                Add(null, viewModel);
            }

            CommandManager.InvalidateRequerySuggested();
        }



        public override string Id => null;
        public ObservableCollection<FolderM> Context { get; }
        public override IList GetContext(BaseM model) { return Context.Last().content; }



        private Notifier selection;
        public override void AddToSelection(Notifier viewModel, bool resetSelection)
        {
            if (selection != null)
            {
                ActiveContextService.ActiveGraph = null;
                SelectionEditor = null;
                selection.IsSelected = false;
            }

            selection = viewModel;

            if (selection != null)
            {
                selection.IsSelected = true;
                SelectionEditor = _editorCreator(selection, this);
                ActiveContextService.ActiveGraph = SelectionEditor as IGraph;
            }

            CommandManager.InvalidateRequerySuggested();
        }
        public override void GetSelection(IList outSelection) { if (selection != null) outSelection.Add(selection); }
        public override bool HasSelection() => selection != null;
        public override bool SelectionCanBeDeleted() { return selection.Id != CharacterM.PLAYER_ID; }

        public void Callback(object viewModelObj, string propName)
        {
            if (viewModelObj != null && propName == nameof(BaseVM<BaseM>.Name))
            {
                CollectionViewSource.GetDefaultView(ItemsVMs)?.Refresh();
            }
        }

        public override string Title => null;
        public override string Stats => null;
    }
}