/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Helpers;
using System.ComponentModel;
using System.Windows.Data;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_RegularVM<T> : Node_InteractiveVM<T> where T : Node_RegularM
    {
        public CollectionViewSource FilteredCharacterCVS { get; }

        public Node_RegularVM(T model, ICallbackContext callbackContext) : base(model, callbackContext)
        {
            Model.characterId = Model.characterId ?? CharacterM.PLAYER_ID;

            FilteredCharacterCVS = new CollectionViewSource() { Source = ActiveContextService.Characters };

            if (FilteredCharacterCVS.View != null)
            {
                FilteredCharacterCVS.View.Filter = OnFilter;
                FilteredCharacterCVS.View.SortDescriptions.Add(new SortDescription(nameof(BaseM.name), ListSortDirection.Ascending));
                FilteredCharacterCVS.View.MoveCurrentTo(Character);
            }
        }

        private bool OnFilter(object sender)
        {
            if (sender is BaseM model)
            {
                return string.IsNullOrEmpty(characterFilter) || model.PassFilter(characterFilter);
            }
            return false;
        }

        protected string characterFilter;
        public string CharacterFilter
        {
            set
            {
                if (value != characterFilter)
                {
                    characterFilter = value;
                    FilteredCharacterCVS.View?.Refresh();
                }
            }
        }

        public BaseM Character
        {
            get => ActiveContextService.GetCharacter(Model.characterId);
            set
            {
                if (value?.id != Model.characterId)
                {
                    Model.characterId = value?.id;
                    OnModelChanged(Model, nameof(Character));
                    Notify(nameof(Character));

                    RefreshModelName();
                }
            }
        }

        public string OverrideName
        {
            get => Model.overrideName;
            set
            {
                if (Model.overrideName != value)
                {
                    Model.overrideName = value;
                    OnModelChanged(Model, nameof(OverrideName));
                }
            }
        }

        public string FileHttpRef
        {
            get => Model.fileHttpRef;
            set
            {
                if (Model.fileHttpRef != value)
                {
                    Model.fileHttpRef = value;
                    OnModelChanged(Model, nameof(FileHttpRef));
                }
            }
        }

        public string ShortDescription
        {
            get => Model.shortDescription;
            set
            {
                if (Model.shortDescription != value)
                {
                    Model.shortDescription = value;
                    OnModelChanged(Model, nameof(ShortDescription));
                }
            }
        }
    }

    public class Node_ReplicaVM : Node_RegularVM<Node_ReplicaM>
    {
        public Node_ReplicaVM(Node_ReplicaM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        protected override void RefreshModelName() { Name = string.Format("[{0}]: {1}", Character?.name ?? "???", FlowDocumentHelper.GetTextFromFlowDoc(DescriptionFlow)); }
    }

    public class Node_ReplicaEditorVM : Node_ReplicaVM
    {
        public Node_ReplicaEditorVM(Node_ReplicaVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }

    public class Node_DialogVM : Node_RegularVM<Node_DialogM>
    {
        public Node_DialogVM(Node_DialogM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        protected override void RefreshModelName() { Name = string.Format("[{0}]: {1}", Character?.name ?? "???", FlowDocumentHelper.GetTextFromFlowDoc(DescriptionFlow)); }
    }

    public class Node_DialogEditorVM : Node_DialogVM
    {
        public Node_DialogEditorVM(Node_DialogVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }
}