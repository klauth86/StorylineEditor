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
        public CollectionVM(ICollection<BaseM> model, Func<bool, BaseM> itemMCreator, Func<BaseM, Notifier> itemVMCreator,
            Func<Notifier, Notifier, Notifier> selectionVMCreator, Action<Notifier> itemMRemover, Action<Notifier> itemVMInformer) : base(model)
        {
            _itemMCreator = itemMCreator ?? throw new ArgumentNullException(nameof(itemMCreator));
            _itemVMCreator = itemVMCreator ?? throw new ArgumentNullException(nameof(itemVMCreator));
            _selectionVMCreator = selectionVMCreator ?? throw new ArgumentNullException(nameof(selectionVMCreator));
            _itemMRemover = itemMRemover ?? throw new ArgumentNullException(nameof(itemMRemover));
            _itemVMInformer = itemVMInformer ?? throw new ArgumentNullException(nameof(itemVMInformer));

            ItemsVMs = new ObservableCollection<Notifier>();

            foreach (var itemM in Model) { ItemsVMs.Add(_itemVMCreator(itemM)); }
        }



        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) =>
        {
            BaseM itemM = _itemMCreator(isFolder);
            selectionSource = _itemVMCreator(itemM);

            (Context ?? Model).Add(itemM);
            ItemsVMs.Add(selectionSource);

            Selection = _selectionVMCreator(Selection, selectionSource);
        }));

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            Selection = null;

            if (ItemsVMs.Remove(selectionSource))
            {
                _itemMRemover(selectionSource);

                selectionSource = null;
            }
        }, (itemVM) => itemVM != null));

        private ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((item) => _itemVMInformer(item), (item) => item != null));

        private ICommand setContextCommand;
        public ICommand SetContextCommand => setContextCommand ?? (setContextCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            if (itemVM is FolderVM folderVM)
            {
                ItemsVMs.Clear();
                foreach (var itemM in folderVM.Model.content) { ItemsVMs.Add(_itemVMCreator(itemM)); }
            }
        }, (itemVM) => itemVM != null));

        private ICommand selectCommand;
        public ICommand SelectCommand => selectCommand ?? (selectCommand = new RelayCommand<Notifier>((itemVM) =>
        {
            selectionSource = itemVM;
            Selection = _selectionVMCreator(Selection, selectionSource);
            
            CommandManager.InvalidateRequerySuggested();
        }, (itemVM) => itemVM != null));



        private readonly Func<bool, BaseM> _itemMCreator;
        private readonly Func<BaseM, Notifier> _itemVMCreator;
        private readonly Func<Notifier, Notifier, Notifier> _selectionVMCreator;
        private readonly Action<Notifier> _itemMRemover;
        private readonly Action<Notifier> _itemVMInformer;


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

        private Notifier selectionSource;

        private Notifier selection;
        public Notifier Selection
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