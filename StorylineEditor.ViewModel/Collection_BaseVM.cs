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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class CutEntryVM : Notifier
    {
        public BaseM Model { get; set; }
        public Notifier ViewModel { get; set; }
        public IList Context { get; set; }
    }

    public abstract class Collection_BaseVM<T> : SimpleVM<T> where T : class
    {
        public Collection_BaseVM(T model, Func<bool, BaseM> modelCreator, Func<BaseM, Notifier> viewModelCreator,
            Func<Notifier, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor) : base(model)
        {
            _modelCreator = modelCreator ?? throw new ArgumentNullException(nameof(modelCreator));
            _viewModelCreator = viewModelCreator ?? throw new ArgumentNullException(nameof(viewModelCreator));
            _editorCreator = editorCreator ?? throw new ArgumentNullException(nameof(editorCreator));

            _modelExtractor = modelExtractor ?? throw new ArgumentNullException(nameof(modelExtractor));

            CutVMs = new ObservableCollection<CutEntryVM>();

            ItemsVMs = new ObservableCollection<Notifier>();
        }

        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM model = _modelCreator(isFolder);
            Notifier viewModel = _viewModelCreator(model);

            Add(model, viewModel);

            Selection = viewModel;

            CommandManager.InvalidateRequerySuggested();

        }));

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand(() =>
        {
            Notifier prevSelection = Selection;
            Selection = null;

            BaseM model = _modelExtractor(prevSelection);

            Remove(prevSelection, model, GetContext(model));

            CommandManager.InvalidateRequerySuggested();

        }, () => Selection != null));

        private ICommand cutCommand;
        public ICommand CutCommand => cutCommand ?? (cutCommand = new RelayCommand<Notifier>((viewModel) =>
        {
            BaseM model = _modelExtractor(viewModel);

            CutVMs.Add(new CutEntryVM() { Model = model, ViewModel = viewModel, Context = GetContext(model) });
            viewModel.IsCut = true;

            CommandManager.InvalidateRequerySuggested();

        }, (viewModel) => viewModel != null && !viewModel.IsCut));

        private ICommand pasteCommand;
        public ICommand PasteCommand => pasteCommand ?? (pasteCommand = new RelayCommand(() =>
        {
            foreach (var cutEntryVM in CutVMs)
            {
                Remove(cutEntryVM.ViewModel, cutEntryVM.Model, cutEntryVM.Context);
                Add(cutEntryVM.Model, cutEntryVM.ViewModel);

                cutEntryVM.ViewModel.IsCut = false;
            }

            Selection = CutVMs.Last().ViewModel;

            CutVMs.Clear();

            CommandManager.InvalidateRequerySuggested();

        }, () => CutVMs.Count > 0));

        private ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((viewModel) =>
        {
            Selection = viewModel;

            CommandManager.InvalidateRequerySuggested();

        }, (viewModel) => viewModel != null));



        protected void Add(BaseM model, Notifier viewModel) // pass null to one of params if want to add only model/only viewModel
        {
            if (model != null) GetContext(model).Add(model);

            if (viewModel != null) ItemsVMs.Add(viewModel);
        }
        protected void Remove(Notifier viewModel, BaseM model, IList context) // pass null to one of params if want to remove only model/only viewModel
        {
            if (viewModel != null) ItemsVMs.Remove(viewModel);

            if (model != null) context.Remove(model);
        }



        protected readonly Func<bool, BaseM> _modelCreator;
        protected readonly Func<BaseM, Notifier> _viewModelCreator;
        protected readonly Func<Notifier, Notifier> _editorCreator;
        protected readonly Func<Notifier, BaseM> _modelExtractor;



        public ObservableCollection<CutEntryVM> CutVMs { get; }
        public ObservableCollection<Notifier> ItemsVMs { get; }



        private Notifier selection;
        public Notifier Selection
        {
            get => selection;
            set
            {
                if (selection != value)
                {
                    if (selection != null) selection.IsSelected = false;

                    selection = value;

                    if (selection != null) selection.IsSelected = true;

                    Notify(nameof(Selection));
                    Notify(nameof(SelectionEditor));

                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Notifier SelectionEditor => selection != null ? _editorCreator(selection) : null;

        public abstract IList GetContext(BaseM model);
    }
}