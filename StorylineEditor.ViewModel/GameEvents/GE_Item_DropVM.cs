/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Common;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Item_DropVM : GE_BaseVM<GE_Item_DropM>
    {
        public CollectionViewSource ItemsCVS { get; }

        public GE_Item_DropVM(GE_Item_DropM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            ItemsCVS = new CollectionViewSource() { Source = ActiveContextService.Items };

            if (ItemsCVS.View != null)
            {
                ItemsCVS.View.Filter = OnFilter;
                ItemsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                ItemsCVS.View.MoveCurrentTo(Item);
            }
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(itemsFilter) || model.PassFilter(itemsFilter);
            }
            return false;
        }

        protected string itemsFilter;
        public string ItemsFilter
        {
            set
            {
                if (value != itemsFilter)
                {
                    itemsFilter = value;
                    ItemsCVS.View?.Refresh();
                }
            }
        }

        public BaseM Item
        {
            get => ActiveContextService.GetItem(Model.itemId);
            set
            {
                if (value?.id != Model.itemId)
                {
                    Model.itemId = value?.id;
                    Notify(nameof(Item));
                }
            }
        }
    }
}