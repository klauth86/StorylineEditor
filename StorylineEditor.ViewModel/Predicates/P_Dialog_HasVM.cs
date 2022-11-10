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
using StorylineEditor.ViewModel.Common;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Predicates
{
    public class P_Dialog_HasVM : P_BaseVM<P_Dialog_HasM>
    {
        public CollectionViewSource DialogsAndReplicasCVS { get; }

        public P_Dialog_HasVM(P_Dialog_HasM model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            DialogsAndReplicasCVS = new CollectionViewSource() { Source = ActiveContextService.DialogsAndReplicas };
            
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
            get => ActiveContextService.GetDialogOrReplica(Model.dialogId);
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