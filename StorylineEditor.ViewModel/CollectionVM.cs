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
using System.Windows.Input;

namespace StorylineEditor.ViewModel
{
    public class CollectionVM : BaseVM<ICollection<BaseM>>
    {
        private readonly Func<BaseM> _itemCreator;

        public CollectionVM(ICollection<BaseM> model, Func<BaseM> itemCreator) : base(model)
        {
            context = model;
            _itemCreator = itemCreator ?? throw new ArgumentNullException(nameof(itemCreator));
        }

        private ICommand addCommand;
        public ICommand AddCommand => addCommand ?? (addCommand = new RelayCommand<bool>((isFolder) => { }));

        private ICommand removeCommand;
        public ICommand RemoveCommand => removeCommand ?? (removeCommand = new RelayCommand<BaseM>((item) => { }, (item) => item != null));

        private ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<BaseM>((item) => { }, (item) => item != null));

        private object context;
        public object Context
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
    }
}