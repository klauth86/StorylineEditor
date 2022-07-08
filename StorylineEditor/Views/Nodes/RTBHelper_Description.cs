/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels.Nodes;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace StorylineEditor.Views.Nodes
{
    public static class RTBHelper_Description
    {
        public static bool GetIsEditing(DependencyObject obj) => (bool)obj.GetValue(IsEditingProperty);
        public static void SetIsEditing(DependencyObject obj, bool value) => obj.SetValue(IsEditingProperty, value);

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.RegisterAttached(
                "IsEditing",
                typeof(bool),
                typeof(RTBHelper_Description),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsEditingChanged));

        private static void IsEditingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                if (!(obj is RichTextBox rtb)) return;

                rtb.DataContextChanged += DataContextChanged;
                rtb.TextChanged += OnTextChanged;

                var tabControl = (((rtb.Parent as GroupBox).Parent as Grid).Parent as TabItem).Parent as TabControl;

                var button = ((rtb.Parent as GroupBox)?.Header as StackPanel).Children[1] as Button;
                button.Click += OnClick;

                tabControl.Tag = new Tuple<RichTextBox, Button>(rtb, button);
                tabControl.SelectionChanged += OnSelectionChanged_Parent;
                tabControl.Unloaded += OnUnloaded_Parent;

                if (!(rtb.DataContext is Node_BaseVm node)) return;

                if (!string.IsNullOrEmpty(node.Description))
                {
                    using (var stringReader = new StringReader(node.Description))
                    {
                        using (var xmlTextReader = new XmlTextReader(stringReader))
                        {
                            rtb.Document = (FlowDocument)XamlReader.Load(xmlTextReader);
                        }
                    }
                }
            }
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            if (!((((sender as FrameworkElement).Parent as FrameworkElement)?.Parent as GroupBox)?.Content is RichTextBox rtb)) return;

            RefreshInlines(rtb);
        }

        private static void RefreshInlines(RichTextBox rtb)
        {
            if (rtb.Selection.IsEmpty) return;

            var selectionText = rtb.Selection.Text;
            if (selectionText.All(c => c == '\n' || c == '\r')) return;

            var pSelectionStart = rtb.Selection.Start;
            var selectionStartRun = pSelectionStart.Parent as Run;
            var selectionStartParagraph = selectionStartRun.Parent as Paragraph;
            var sT = selectionStartRun?.Text;

            var pSelectionEnd = rtb.Selection.End;
            var selectionEndRun = pSelectionEnd.Parent is FlowDocument ? ((rtb.Document.Blocks.LastBlock as Paragraph).Inlines.LastInline as Run) : pSelectionEnd.Parent as Run;
            var selectionEndParagraph = selectionEndRun.Parent as Paragraph;
            var eT = selectionEndRun?.Text;

            var fontWeight = selectionStartRun.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;

            if (selectionStartRun == selectionEndRun)
            {
                RefreshInlines(selectionText, pSelectionStart, pSelectionEnd, selectionStartRun, selectionStartParagraph, fontWeight);
            }
            else
            {
                bool paragraphStarted = false;
                bool runStarted = false;
                bool runEnded = false;
                bool paragraphEnded = false;

                foreach (Paragraph paragraph in rtb.Document.Blocks)
                {
                    if (paragraph == selectionStartParagraph) paragraphStarted = true;

                    if (paragraphStarted && !paragraphEnded)
                    {
                        foreach (Run inline in paragraph.Inlines)
                        {
                            if (inline == selectionEndRun) runEnded = true; // First and last will be processed alone

                            if (runStarted && !runEnded) inline.FontWeight = fontWeight;

                            if (inline == selectionStartRun) runStarted = true; // First and last will be processed alone
                        }
                    }

                    if (paragraph == selectionEndParagraph) paragraphEnded = true;
                }

                var selectionTextStart = selectionText.Substring(0, -selectionStartRun.ContentEnd.GetOffsetToPosition(pSelectionStart));
                RefreshInlines(selectionTextStart, pSelectionStart, selectionStartRun.ContentEnd, selectionStartRun, selectionStartParagraph, fontWeight);

                var selectionTextEnd = selectionText.Substring(selectionText.Length + pSelectionEnd.GetOffsetToPosition(selectionEndRun.ContentStart));
                RefreshInlines(selectionTextEnd, selectionEndRun.ContentStart, pSelectionEnd, selectionEndRun, selectionEndParagraph, fontWeight);
            }

            if (!(rtb.DataContext is Node_BaseVm node)) return;

            string newDescription = XamlWriter.Save(rtb.Document);

            node.Description = newDescription;
        }

        private static void RefreshInlines(string selectionText, TextPointer pSelectionStart, TextPointer pSelectionEnd, Run selectionStartRun, Paragraph selectionStartParagraph, FontWeight fontWeight)
        {
            var runText = selectionStartRun.Text;

            if (selectionText.Contains(runText))
            {
                selectionStartRun.FontWeight = fontWeight;
            }
            else
            {
                var substringStart = -pSelectionStart.GetOffsetToPosition(selectionStartRun.ContentStart);

                var substringEnd = Math.Min(-pSelectionEnd.GetOffsetToPosition(selectionStartRun.ContentStart), runText.Length);

                var changedText = selectionStartRun.Text.Substring(substringStart, substringEnd - substringStart);

                if (substringStart == 0)
                {
                    var unchangedText = selectionStartRun.Text.Substring(substringEnd);

                    selectionStartParagraph.Inlines.InsertBefore(selectionStartRun, new Run() { Text = changedText, FontWeight = fontWeight });
                    selectionStartRun.Text = unchangedText;
                }
                else if (substringEnd == runText.Length)
                {
                    var unchangedText = selectionStartRun.Text.Substring(0, substringStart);

                    selectionStartRun.Text = unchangedText;
                    selectionStartParagraph.Inlines.InsertAfter(selectionStartRun, new Run() { Text = changedText, FontWeight = fontWeight });
                }
                else
                {
                    var firstText = selectionStartRun.Text.Substring(0, substringStart);
                    if (!string.IsNullOrEmpty(firstText)) selectionStartParagraph.Inlines.InsertBefore(selectionStartRun, new Run() { Text = firstText, FontWeight = selectionStartRun.FontWeight });

                    var thirdText = selectionStartRun.Text.Substring(substringEnd, runText.Length - substringEnd);
                    if (!string.IsNullOrEmpty(thirdText)) selectionStartParagraph.Inlines.InsertAfter(selectionStartRun, new Run() { Text = thirdText, FontWeight = selectionStartRun.FontWeight });

                    selectionStartRun.Text = changedText;
                    selectionStartRun.FontWeight = fontWeight;
                }
            }
        }

        private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is RichTextBox rtb)) return;

            if (e.NewValue == null) return;

            if (!(e.NewValue is Node_BaseVm node)) return;

            if (!string.IsNullOrEmpty(node.Description))
            {
                using (var stringReader = new StringReader(node.Description))
                {
                    using (var xmlTextReader = new XmlTextReader(stringReader))
                    {
                        rtb.Document = (FlowDocument)XamlReader.Load(xmlTextReader);
                    }
                }
            }
            else
            {
                var document = new FlowDocument();
                document.Blocks.Add(new Paragraph());
                rtb.Document = document;
            }
        }

        private static void OnUnloaded_Parent(object sender, RoutedEventArgs e)
        {
            if (!(sender is TabControl tabControl)) return;

            if (tabControl.Tag is Tuple<RichTextBox, Button> tuple)
            {
                if (tuple.Item1 != null)
                {
                    tuple.Item1.DataContextChanged -= DataContextChanged;
                    tuple.Item1.TextChanged -= OnTextChanged;
                }
                if (tuple.Item2 != null)
                {
                    tuple.Item2.Click -= OnClick;
                }
            }

            tabControl.SelectionChanged -= OnSelectionChanged_Parent;
            tabControl.Unloaded -= OnUnloaded_Parent;
        }

        private static void OnSelectionChanged_Parent(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is TabControl tc)) return;

            bool isEdititng = tc.SelectedIndex == 0;

            if ((((tc.Items[0] as TabItem).Content as Grid).Children[1] as GroupBox).Content is RichTextBox rtb) SetIsEditing(rtb, isEdititng);
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is RichTextBox rtb)) return;

            if (!(rtb.DataContext is Node_BaseVm node)) return;

            string newDescription = XamlWriter.Save(rtb.Document);

            node.Description = newDescription;
        }

        public static string GetDocument(DependencyObject obj) => (string)obj.GetValue(DocumentProperty);
        public static void SetDocument(DependencyObject obj, string value) => obj.SetValue(DocumentProperty, value);

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached(
                "Document",
                typeof(string),
                typeof(RTBHelper_Description),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, DocumentChanged));

        private static void DocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is TextBlock tb)) return;

            tb.Text = null;

            if (!string.IsNullOrEmpty(e.NewValue?.ToString()))
            {
                using (var stringReader = new StringReader(e.NewValue?.ToString()))
                {
                    using (var xmlTextReader = new XmlTextReader(stringReader))
                    {
                        var document = (FlowDocument)XamlReader.Load(xmlTextReader);

                        if (document.Blocks.Count > 0)
                        {
                            foreach (Paragraph paragraph in document.Blocks)
                            {
                                foreach (var inline in paragraph.Inlines)
                                {
                                    if (!(inline is Run documentRun)) continue;

                                    var run = new Run
                                    {
                                        Text = documentRun.Text,
                                        Background = documentRun.Background,
                                        FontWeight = documentRun.FontWeight
                                    };

                                    tb.Inlines.Add(run);
                                }

                                if (paragraph != document.Blocks.LastBlock) tb.Inlines.Add(new LineBreak());
                            }
                        }
                        else
                        {
                            document.Blocks.Add(new Paragraph());
                        }
                    }
                }
            }
        }
    }
}