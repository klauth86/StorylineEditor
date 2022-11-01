using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Controls
{
    public class RichTextBoxWithDP : RichTextBox
    {
        private static readonly DependencyProperty DocumentDPProperty = DependencyProperty.Register
            (
            "DocumentDP",
            typeof(FlowDocument),
            typeof(RichTextBoxWithDP),
            new PropertyMetadata(DocumentDPPropertyChanged)
            );

        public static void SetDocumentDP(DependencyObject dp, FlowDocument value) { dp.SetValue(DocumentDPProperty, value); }
        public static FlowDocument GetDocumentDP(DependencyObject dp) { return (FlowDocument)dp.GetValue(DocumentDPProperty); }

        private static void DocumentDPPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (dp is RichTextBox richTextBox)
            {
                richTextBox.Document =
                    args.NewValue != DependencyProperty.UnsetValue && args.NewValue is FlowDocument flowDocument
                    ? flowDocument
                    : null;
            }
        }
    }
}