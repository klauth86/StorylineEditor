/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.Views.Controls;
using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModels
{
    public class TreePlayerContext_ChoiceVm : BaseVm
    {
        public readonly TreeCanvas TreeCanvas;

        public TreePlayerContext_ChoiceVm(TreeCanvas treeCanvas, Dictionary<Node_BaseVm, List<Node_BaseVm>> nodesPaths) : base(0)
        {
            TreeCanvas = treeCanvas;
            NodesPaths = nodesPaths;
        }
        public Dictionary<Node_BaseVm, List<Node_BaseVm>> NodesPaths { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<Node_BaseVm>((node) => { TreeCanvas?.PrepareAndStartTransition(node, NodesPaths[node]); }, (node) => node != null && NodesPaths.ContainsKey(node)));
    }
}
