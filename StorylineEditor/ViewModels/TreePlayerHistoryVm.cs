/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Collections.ObjectModel;
using System.Windows.Input;
using StorylineEditor.Common;

namespace StorylineEditor.ViewModels
{
    public class TreePlayerHistoryVm : BaseVm<FullContextVm>
    {
        public TreePlayerHistoryVm(FullContextVm parent, long additionalTicks) : base(parent, additionalTicks)
        {
            Inventory = new ObservableCollection<ItemVm>();
        }

        public TreePlayerHistoryVm() : this(null, 0) { }

        public ObservableCollection<ItemVm> Inventory { get; set; }

        protected ICommand removeItemCommand;
        public ICommand RemoveItemCommand =>
            removeItemCommand ?? (removeItemCommand = new RelayCommand<ItemVm>((item) => { Inventory.Remove(item); }, (item) => item != null));

        protected ICommand addItemCommand;
        public ICommand AddItemCommand =>
            addItemCommand ?? (addItemCommand = new RelayCommand<ItemVm>((item) => { Inventory.Add(item); }, (item) => item != null));

        public bool HasItem(ItemVm item) => Inventory.Contains(item);
    }
}