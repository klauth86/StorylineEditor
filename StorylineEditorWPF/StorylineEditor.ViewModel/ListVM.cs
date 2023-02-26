/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

    public class ListVM : CollectionVM<List<BaseM>, object ,object>
    {
        public ObservableCollection<CutEntryVM> CutVMs { get; }

        public ListVM(
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

            foreach (var model in Model) { Add(null, null, _vmCreator(model)); }
        }

        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM model = _mCreator(isFolder ? typeof(FolderM) : null, null);
            Notifier viewModel = _vmCreator(model);

            Add(GetContext(model), model, viewModel);

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
                Add(null, null, viewModel);
            }

            CommandManager.InvalidateRequerySuggested();

        }, (folderM) => folderM != null));

        private ICommand dropCommand;
        public ICommand DropCommand => dropCommand ?? (dropCommand = new RelayCommand<Tuple<object, object>>((tuple) =>
        {
            if (tuple != null)
            {
                if (tuple.Item1 is FolderVM folderViewModel)
                {
                    if (tuple.Item2 is Notifier itemViewModel)
                    {
                        List<Notifier> prevSelection = new List<Notifier>();
                        GetSelection(prevSelection);

                        if (prevSelection.Contains(itemViewModel))
                        {
                            AddToSelection(null, true);
                        }

                        FolderM folderModel = ((IWithModel)folderViewModel).GetModel<FolderM>();
                        BaseM itemModel = ((IWithModel)itemViewModel).GetModel<BaseM>();

                        Remove(itemViewModel, itemModel, GetContext(itemModel));
                        Add(folderModel.content, itemModel, null);
                    }
                }
            }
        }));

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
                Add(GetContext(cutEntryVM.Model), cutEntryVM.Model, cutEntryVM.ViewModel);

                cutEntryVM.ViewModel.IsCut = false;
            }

            AddToSelection(CutVMs.Last().ViewModel, true);

            CutVMs.Clear();

            CommandManager.InvalidateRequerySuggested();

        }, () => CutVMs.Count > 0));

        public override string Id => null;
        public ObservableCollection<FolderM> Context { get; }
        public override IList GetContext(BaseM model) { return Context.Last().content; }
    }
}