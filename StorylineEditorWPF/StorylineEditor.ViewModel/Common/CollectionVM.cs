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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public abstract class CollectionVM<T, U, V>
        : SimpleVM<T, U>
        , ICollection_Base
        , IPartiallyStored
        where T : class
        where U : class
    {
        protected readonly Func<Type, V, BaseM> _mCreator;
        protected readonly Func<BaseM, Notifier> _vmCreator;
        protected readonly Func<Notifier, Notifier> _evmCreator;

        public ObservableCollection<Notifier> ItemVms { get; }

        protected Notifier selectionEditor;
        public Notifier SelectionEditor
        {
            get => selectionEditor;
            set
            {
                if (value != selectionEditor)
                {
                    selectionEditor = value;
                    Notify(nameof(SelectionEditor));
                }
            }
        }

        public CollectionVM(
            T model
            , U parent
            , Func<Type, V, BaseM> mCreator
            , Func<BaseM, Notifier> vmCreator
            , Func<Notifier, Notifier> evmCreator)
            : base(model, parent)
        {
            _mCreator = mCreator ?? throw new ArgumentNullException(nameof(mCreator));
            _vmCreator = vmCreator ?? throw new ArgumentNullException(nameof(vmCreator));
            _evmCreator = evmCreator ?? throw new ArgumentNullException(nameof(evmCreator));

            ItemVms = new ObservableCollection<Notifier>();
        }

        // pass null to one of params if want to add only model/only viewModel
        protected void Add(
            BaseM model
            , Notifier viewModel
            )
        {
            if (model != null) GetContext(model).Add(model);

            if (viewModel != null) ItemVms.Add(viewModel);
        }

        // pass null to one of params if want to remove only model/only viewModel
        protected void Remove(
            Notifier viewModel
            , BaseM model
            , IList context
            )
        {
            if (viewModel != null) ItemVms.Remove(viewModel);

            if (model != null && context != null) context.Remove(model);
        }

        public abstract IList GetContext(
            BaseM model
            );

        private Notifier selection;
        public virtual void AddToSelection(Notifier viewModel, bool resetSelection)
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

        public virtual void GetSelection(IList outSelection) { if (selection != null) outSelection.Add(selection); }
        public virtual bool HasSelection() => selection != null;
        public virtual bool SelectionCanBeDeleted() { return selection.Id != CharacterM.PLAYER_ID; }

        public bool AddToSelectionById(
            string id
            , bool resetSelection
            )
        {
            Notifier viewModel = ItemVms.FirstOrDefault(itemVm => itemVm.Id == id);

            if (viewModel != null)
            {
                AddToSelection(
                    viewModel
                    , resetSelection
                    );
                return true;
            }

            return false;
        }

        public void Refresh() { CollectionViewSource.GetDefaultView(ItemVms)?.Refresh(); }

        public void OnEnter()
        {
            ActiveContext.ActiveGraph = SelectionEditor as IGraph;
        }

        public void OnLeave()
        {
            ActiveContext.ActiveGraph = null;
        }
    }
}