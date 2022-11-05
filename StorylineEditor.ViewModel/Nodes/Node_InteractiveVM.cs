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
    public abstract class Node_InteractiveVM<T> : Node_BaseVM<T> where T : Node_InteractiveM
    {
        public Node_InteractiveVM(T model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_RandomVM : Node_InteractiveVM<Node_RandomM>
    {
        public Node_RandomVM(Node_RandomM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_RandomEditorVM : Node_RandomVM
    {
        public Node_RandomEditorVM(Node_RandomVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }

    public class Node_TransitVM : Node_InteractiveVM<Node_TransitM>
    {
        public Node_TransitVM(Node_TransitM model, ICallbackContext callbackContext) : base(model, callbackContext) { }
    }

    public class Node_TransitEditorVM : Node_TransitVM
    {
        public Node_TransitEditorVM(Node_TransitVM viewModel) : base(viewModel.Model, viewModel.CallbackContext) { }
    }
}