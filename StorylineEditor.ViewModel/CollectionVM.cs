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
    public class CutEntryVM : Notifier
    {
        public override string Id => null;
        public BaseM Model { get; set; }
        public Notifier ViewModel { get; set; }
        public IList Context { get; set; }
    }

    public class CollectionVM : Collection_BaseVM<List<BaseM>, object ,object>
    {
        public ObservableCollection<CutEntryVM> CutVMs { get; }

        public CollectionVM(
            List<BaseM> inModel
            , object parent
            , Func<Type, object, BaseM> mCreator
            , Func<BaseM, Notifier> vmCreator
            , Func<Notifier, Notifier> evmCreator
            )
            : base(
                  inModel
                  , parent
                  , mCreator
                  , vmCreator
                  , evmCreator
                  )
        {
            CutVMs = new ObservableCollection<CutEntryVM>();

            ICollectionView view = CollectionViewSource.GetDefaultView(ItemVms);

            if (view != null)
            {
                view.SortDescriptions.Add(new SortDescription(nameof(IsFolder), ListSortDirection.Descending));
                view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                view.SortDescriptions.Add(new SortDescription("CreatedAt", ListSortDirection.Ascending));
            }

            Context = new ObservableCollection<FolderM>();
            Context.Add(new FolderM() { name = "root", content = inModel });

            foreach (var model in Model) { Add(null, _vmCreator(model)); }
        }

        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM model = _mCreator(isFolder ? typeof(FolderM) : null, null);
            Notifier viewModel = _vmCreator(model);

            Add(model, viewModel);

            AddToSelection(viewModel, true);
        }));

        private ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((viewModel) => AddToSelection(viewModel, true), (viewModel) => viewModel != null));

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

            AddToSelection(null, true);

            ItemVms.Clear();

            HashSet<string> cutViewModels = new HashSet<string>();
            foreach (var cutEntryVM in CutVMs) cutViewModels.Add(cutEntryVM.ViewModel.Id);

            foreach (var model in Context.Last().content)
            {
                Notifier viewModel = _vmCreator(model);
                viewModel.IsCut = cutViewModels.Contains(viewModel.Id);
                Add(null, viewModel);
            }

            CommandManager.InvalidateRequerySuggested();

        }, (folderM) => folderM != null));

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand(() =>
        {
            List<Notifier> prevSelection = new List<Notifier>();
            GetSelection(prevSelection);

            AddToSelection(null, true);

            foreach (var viewModel in prevSelection)
            {
                if (viewModel.Id == CharacterM.PLAYER_ID) continue;

                BaseM model = (viewModel as IWithModel)?.GetModel<BaseM>();
                Remove(viewModel, model, GetContext(model));
            }

            CommandManager.InvalidateRequerySuggested();

        }, () => HasSelection() && SelectionCanBeDeleted()));

        private ICommand cutCommand;
        public ICommand CutCommand => cutCommand ?? (cutCommand = new RelayCommand(() =>
        {
            foreach (var cutEntryVM in CutVMs) cutEntryVM.ViewModel.IsCut = false;
            CutVMs.Clear();

            List<Notifier> selection = new List<Notifier>();
            GetSelection(selection);

            foreach (var viewModel in selection)
            {
                BaseM model = (viewModel as IWithModel)?.GetModel<BaseM>();
                CutVMs.Add(new CutEntryVM() { Model = model, ViewModel = viewModel, Context = GetContext(model) });
                viewModel.IsCut = true;
            }

            CommandManager.InvalidateRequerySuggested();

        }, () => HasSelection()));

        private ICommand pasteCommand;
        public ICommand PasteCommand => pasteCommand ?? (pasteCommand = new RelayCommand(() =>
        {
            foreach (var cutEntryVM in CutVMs)
            {
                Remove(cutEntryVM.ViewModel, cutEntryVM.Model, cutEntryVM.Context);
                Add(cutEntryVM.Model, cutEntryVM.ViewModel);

                cutEntryVM.ViewModel.IsCut = false;
            }

            AddToSelection(CutVMs.Last().ViewModel, true);

            CutVMs.Clear();

            CommandManager.InvalidateRequerySuggested();

        }, () => CutVMs.Count > 0));

        public override string Id => null;
        public ObservableCollection<FolderM> Context { get; }
        public override IList GetContext(BaseM model) { return Context.Last().content; }



        private Notifier selection;
        public override void AddToSelection(Notifier viewModel, bool resetSelection)
        {
            if (selection != null)
            {
                ActiveContext.ActiveGraph = null;
                SelectionEditor = null;
                selection.IsSelected = false;
            }

            selection = viewModel;

            if (selection != null)
            {
                selection.IsSelected = true;
                SelectionEditor = _evmCreator(selection);
                ActiveContext.ActiveGraph = SelectionEditor as IGraph;
            }

            CommandManager.InvalidateRequerySuggested();
        }
        public override void GetSelection(IList outSelection) { if (selection != null) outSelection.Add(selection); }
        public override bool HasSelection() => selection != null;
        public override bool SelectionCanBeDeleted() { return selection.Id != CharacterM.PLAYER_ID; }
    }
}