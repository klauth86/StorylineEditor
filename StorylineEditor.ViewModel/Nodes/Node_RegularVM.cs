/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModel.Common;

namespace StorylineEditor.ViewModel.Nodes
{
    public abstract class Node_RegularVM<T> : Node_BaseVM<T> where T : Node_RegularM
    {
        public Node_RegularVM(T model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_ReplicaVM : Node_RegularVM<Node_ReplicaM>
    {
        public Node_ReplicaVM(Node_ReplicaM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_ReplicaEditorVM : Node_ReplicaVM
    {
        public Node_ReplicaEditorVM(Node_ReplicaVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }

    public class Node_DialogVM : Node_RegularVM<Node_DialogM>
    {
        public Node_DialogVM(Node_DialogM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_DialogEditorVM : Node_DialogVM
    {
        public Node_DialogEditorVM(Node_DialogVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }
}