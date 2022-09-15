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
using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModels
{
    public class TreePlayerContext_ChoiceVm : BaseVm<TreePlayerVm>
    {
        public TreePlayerContext_ChoiceVm(TreePlayerVm parent, List<Node_BaseVm> nodesToSelect) : base(parent, 0)
        {
            NodesToSelect = new List<Node_BaseVm>();
            NodesToSelect.AddRange(nodesToSelect);
        }
        public List<Node_BaseVm> NodesToSelect { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<Node_BaseVm>((node) => { Parent?.StartTransition(node); }, (node) => node != null && NodesToSelect.Contains(node)));
    }
}
