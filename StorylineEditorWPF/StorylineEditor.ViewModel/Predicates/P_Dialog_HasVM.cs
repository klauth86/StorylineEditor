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
using StorylineEditor.Model.Predicates;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Dialog_HasVM : P_BaseVM<P_Dialog_HasM, object>
    {
        public CollectionViewSource DialogsAndReplicasCVS { get; }

        public P_Dialog_HasVM(P_Dialog_HasM model, object parent) : base(model, parent)
        {
            DialogsAndReplicasCVS = new CollectionViewSource() { Source = ActiveContext.DialogsAndReplicas };
            
            if (DialogsAndReplicasCVS.View != null)
            {
                DialogsAndReplicasCVS.View.Filter = OnFilter;
                DialogsAndReplicasCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.id), ListSortDirection.Ascending));
                DialogsAndReplicasCVS.View.MoveCurrentTo(DialogOrReplica);
            }
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(dialogsAndReplicasFilter) || model.PassFilter(dialogsAndReplicasFilter);
            }
            return false;
        }

        protected string dialogsAndReplicasFilter;
        public string DialogsAndReplicasFilter
        {
            set
            {
                if (value != dialogsAndReplicasFilter)
                {
                    dialogsAndReplicasFilter = value;
                    DialogsAndReplicasCVS.View?.Refresh();
                }
            }
        }
        public BaseM DialogOrReplica
        {
            get => ActiveContext.GetDialogOrReplica(Model.dialogId);
            set
            {
                if (value?.id != Model.dialogId)
                {
                    Model.dialogId = value?.id;
                    Notify(nameof(DialogOrReplica));
                }
            }
        }
    }
}