using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Tabs;
using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModels
{
    public class PlayerChoiceVm : BaseVm<PlayerVm>
    {
        public PlayerChoiceVm(PlayerVm parent, Node_BaseVm activeNode, List<Node_BaseVm> nodesToSelect) : base(parent, 0)
        {
            ActiveNode = activeNode;
            NodesToSelect = new List<Node_BaseVm>();
            NodesToSelect.AddRange(nodesToSelect);
        }

        public Node_BaseVm ActiveNode { get; set; }
        public List<Node_BaseVm> NodesToSelect { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<Node_BaseVm>((node) => { Parent?.StartTransition(node); }, (node) => node != null && NodesToSelect.Contains(node)));
    }
}
