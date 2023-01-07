/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Predicates;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Item_HasVM : P_BaseVM<P_Item_HasM, object>
    {
        public CollectionViewSource ItemsCVS { get; }

        public P_Item_HasVM(P_Item_HasM model, object parent) : base(model, parent)
        {
            ItemsCVS = new CollectionViewSource() { Source = ActiveContextService.Items };

            if (ItemsCVS.View != null)
            {
                ItemsCVS.View.Filter = OnFilter;
                ItemsCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
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

        public override bool IsTrue()
        {
            if (Item != null)
            {
                bool result = ActiveContextService.History.Inventory.Contains(Item);

                if (IsInversed) result = !result;
                return result;
            }

            return true;
        }
    }
}