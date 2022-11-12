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