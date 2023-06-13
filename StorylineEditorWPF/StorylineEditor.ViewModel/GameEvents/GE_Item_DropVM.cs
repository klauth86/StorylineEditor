/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.GameEvents
{
    public class GE_Item_DropVM : GE_BaseVM<GE_Item_DropM, object>
    {
        public CollectionViewSource ItemsCVS { get; }

        public GE_Item_DropVM(GE_Item_DropM model, object parent) : base(model, parent)
        {
            ItemsCVS = new CollectionViewSource() { Source = ActiveContext.Items };

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
            get => ActiveContext.GetItem(Model.itemId);
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