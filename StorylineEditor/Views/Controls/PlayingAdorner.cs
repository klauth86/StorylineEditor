using StorylineEditor.ViewModels.Nodes;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace StorylineEditor.Views.Controls
{
    public class PlayingAdorner: ContentControl
    {
        public PlayingAdorner()
        {
            var ellipse = new System.Windows.Shapes.Ellipse();

            ellipse.Fill = Brushes.Gold;

            Content = ellipse;
        }

        public void ToTransitionForm()
        {
            (Content as System.Windows.Shapes.Ellipse).Width = 64;
            (Content as System.Windows.Shapes.Ellipse).Height = 64;
        }

        public void ToPlayForm(Node_BaseVm activeNode)
        {

        }
    }
}
