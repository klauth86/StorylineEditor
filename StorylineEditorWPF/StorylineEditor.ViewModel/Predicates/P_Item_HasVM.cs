/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
            ItemsCVS = new CollectionViewSource() { Source = ActiveContext.Items };

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

        public override bool IsTrue()
        {
            if (Item != null)
            {
                bool result = ActiveContext.History.Inventory.Contains(Item);

                if (IsInversed) result = !result;
                return result;
            }

            return true;
        }
    }
}