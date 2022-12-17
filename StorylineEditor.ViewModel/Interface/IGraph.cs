using System;

namespace StorylineEditor.ViewModel.Interface
{
    public interface INode : IPositioned
    {
        // Absoulute
        double Width { get; set; }
        double Height { get; set; }

        // Local
        double Left { get; set; }
        double Top { get; set; }

        string Id { get; }
        string Name { get; set; }
        byte Gender { get; }
        bool IsSelected { get; }
        bool IsRoot { get; set; }
    }

    public interface IGraph
    {
        INode SelectionNode { get; }
        void MoveTo(INode node, Action<INode> callbackAction);
    }
}