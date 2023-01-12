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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

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

        public abstract void AddToSelection(
            Notifier viewModel
            , bool resetSelection
            );

        public abstract void GetSelection(
            IList outSelection
            );

        public abstract bool HasSelection();

        public abstract bool SelectionCanBeDeleted();

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
            if (SelectionEditor != null)
            {
                if (SelectionEditor is IPartiallyStored partiallyStoredEd)
                {
                    partiallyStoredEd.OnEnter();
                }
            }
        }

        public void OnLeave()
        {
            if (SelectionEditor != null)
            {
                if (SelectionEditor is IPartiallyStored partiallyStoredEd)
                {
                    partiallyStoredEd.OnLeave();
                }
            }
        }
    }
}