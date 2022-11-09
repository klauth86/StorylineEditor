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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public abstract class Collection_BaseVM<T, U> : SimpleVM<T> where T : class
    {
        public Collection_BaseVM(T model, ICallbackContext callbackContext, Func<Type, U, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
            Func<Notifier, ICallbackContext, Notifier> editorCreator) : base(model, callbackContext)
        {
            _modelCreator = modelCreator ?? throw new ArgumentNullException(nameof(modelCreator));
            _viewModelCreator = viewModelCreator ?? throw new ArgumentNullException(nameof(viewModelCreator));
            _editorCreator = editorCreator ?? throw new ArgumentNullException(nameof(editorCreator));

            CutVMs = new ObservableCollection<CutEntryVM>();

            ItemsVMs = new ObservableCollection<Notifier>();
        }

        public abstract ICommand AddCommand { get; }

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand(() =>
        {
            List<Notifier> prevSelection = new List<Notifier>();            
            GetSelection(prevSelection);
            
            AddToSelection(null, true);

            foreach (var viewModel in prevSelection)
            {
                BaseM model = (viewModel as IWithModel)?.GetModel<BaseM>();
                Remove(viewModel, model, GetContext(model));
            }

            CommandManager.InvalidateRequerySuggested();

        }, () => HasSelection()));

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
        public abstract ICommand SelectCommand{ get; }



        protected void Add(BaseM model, Notifier viewModel) // pass null to one of params if want to add only model/only viewModel
        {
            if (model != null) GetContext(model).Add(model);

            if (viewModel != null) ItemsVMs.Add(viewModel);
        }
        protected void Remove(Notifier viewModel, BaseM model, IList context) // pass null to one of params if want to remove only model/only viewModel
        {
            if (viewModel != null) ItemsVMs.Remove(viewModel);

            if (model != null && context != null) context.Remove(model);
        }



        protected readonly Func<Type, U, BaseM> _modelCreator;
        protected readonly Func<BaseM, ICallbackContext, Notifier> _viewModelCreator;
        protected readonly Func<Notifier, ICallbackContext, Notifier> _editorCreator;



        public ObservableCollection<CutEntryVM> CutVMs { get; }
        public ObservableCollection<Notifier> ItemsVMs { get; }



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
        public abstract IList GetContext(BaseM model);



        public abstract void AddToSelection(Notifier viewModel, bool resetSelection);
        public abstract void GetSelection(IList outSelection);
        public abstract bool HasSelection();
    }
}