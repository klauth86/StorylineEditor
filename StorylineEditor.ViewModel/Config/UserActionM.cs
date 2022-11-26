using System.Windows.Input;

namespace StorylineEditor.ViewModel.Config
{
    public class UserActionM
    {
        public byte UserActionType { get; set; }
        public MouseButton MouseButton { get; set; }
        public ModifierKeys ModifierKeys { get; set; }
    }
}