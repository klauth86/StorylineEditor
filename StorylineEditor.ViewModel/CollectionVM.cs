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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class CollectionVM : BaseVM<ICollection<BaseM>>
    {
        public CollectionVM(ICollection<BaseM> model, Func<bool, BaseM> modelCreator, Func<BaseM, Notifier> viewModelCreator,
            Func<Notifier, Notifier> editorCreator, Action<Notifier> modelRemover, Action<Notifier> viewModelInformer) : base(model)
        {
            _modelCreator = modelCreator ?? throw new ArgumentNullException(nameof(modelCreator));
            _viewModelCreator = viewModelCreator ?? throw new ArgumentNullException(nameof(viewModelCreator));
            _editorCreator = editorCreator ?? throw new ArgumentNullException(nameof(editorCreator));
            
            _modelRemover = modelRemover ?? throw new ArgumentNullException(nameof(modelRemover));
            
            _viewModelInformer = viewModelInformer ?? throw new ArgumentNullException(nameof(viewModelInformer));

            CutVMs = new ObservableCollection<Notifier>();
            ItemsVMs = new ObservableCollection<Notifier>();

            foreach (var itemM in Model) { ItemsVMs.Add(_viewModelCreator(itemM)); }
        }


        private ICommand upCommand;
        public ICommand UpCommand => upCommand ?? (upCommand = new RelayCommand(() => { }, () => Context != null));

        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM model = _modelCreator(isFolder);
            (Context ?? Model).Add(model);

            Notifier viewModel = _viewModelCreator(model);
            ItemsVMs.Add(viewModel);

            Selection = viewModel;
        }));

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand(() =>
        {
            Notifier prevSelection = Selection;
            Selection = null;

            if (ItemsVMs.Remove(prevSelection))
            {
                _modelRemover(prevSelection);
            }
        }, () => Selection != null));

        private ICommand cutCommand;
        public ICommand CutCommand => cutCommand ?? (cutCommand = new RelayCommand<Notifier>((itemVM) => CutVMs.Add(itemVM), (itemVM) => itemVM != null && !CutVMs.Contains(itemVM)));

        private ICommand pasteCommand;
        public ICommand PasteCommand => pasteCommand ?? (pasteCommand = new RelayCommand(() =>
        {
            foreach (var cutVM in CutVMs)
            {
                RemoveCommand.Execute(cutVM);

            }
        }, () => CutVMs.Count > 0));

        private ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((item) => _viewModelInformer(item), (item) => item != null));

        private ICommand setContextCommand;
        public ICommand SetContextCommand => setContextCommand ?? (setContextCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            if (itemVM is FolderVM folderVM)
            {
                ItemsVMs.Clear();
                foreach (var itemM in folderVM.Model.content) { ItemsVMs.Add(_viewModelCreator(itemM)); }
            }
        }, (itemVM) => itemVM != null));

        private ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            Selection = itemVM;
            CommandManager.InvalidateRequerySuggested();
        }, (itemVM) => itemVM != null));



        private readonly Func<bool, BaseM> _modelCreator;
        private readonly Func<BaseM, Notifier> _viewModelCreator;
        private readonly Func<Notifier, Notifier> _editorCreator;
        private readonly Action<Notifier> _modelRemover;
        private readonly Action<Notifier> _viewModelInformer;



        public ObservableCollection<Notifier> CutVMs { get; }
        public ObservableCollection<Notifier> ItemsVMs { get; }

        private ICollection<BaseM> context;
        public ICollection<BaseM> Context
        {
            get => context;
            set
            {
                if (value != context)
                {
                    context = value;
                    Notify(nameof(Context));
                }
            }
        }

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
    }
}