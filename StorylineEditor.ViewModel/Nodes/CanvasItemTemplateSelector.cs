using StorylineEditor.ViewModel.Interface;
using System.Windows;
using System.Windows.Controls;

namespace StorylineEditor.ViewModel.Nodes
{
    public class CanvasDataTemplateSelector: DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return (item is INode)
                ? Application.Current.FindResource(string.Format("{0}_ItemTemplate", item.GetType().Name)) as DataTemplate
                : base.SelectTemplate(item, container);
        }
    }
}
