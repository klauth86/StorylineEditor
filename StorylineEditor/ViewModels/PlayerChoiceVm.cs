using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using System.Collections.Generic;
using System.Windows.Input;

namespace StorylineEditor.ViewModels
{
    public class PlayerChoiceVm : BaseVm<PlayerVm>
    {
        public PlayerChoiceVm(PlayerVm parent, List<Node_BaseVm> nodesToSelect) : base(parent, 0)
        {
            NodesToSelect = new List<Node_BaseVm>();
            NodesToSelect.AddRange(nodesToSelect);
        }
        public List<Node_BaseVm> NodesToSelect { get; set; }

        protected ICommand selectNodeCommand;
        public ICommand SelectNodeCommand => selectNodeCommand ?? (selectNodeCommand = new RelayCommand<Node_BaseVm>((node) => { Parent?.StartTransition(node); }, (node) => node != null && NodesToSelect.Contains(node)));
    }
}
