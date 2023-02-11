/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Interface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Controls
{
    public class RichTextBoxWithTarget : RichTextBox
    {
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register
            (
            "Description",
            typeof(string),
            typeof(RichTextBoxWithTarget),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, DescriptionPropertyChanged)
            );

        private static void DescriptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBoxWithTarget richTextBoxWithTarget)
            {
                string xml = ActiveContext.FlowDocumentService.UnmaskXml(e.NewValue?.ToString());

                richTextBoxWithTarget.RefreshDocument(xml);
            }
        }

        private void RefreshDocument(string xml)
        {
            IsUnderDescriptionPropertyChangedScope = true;
            {
                Document.Blocks.Clear();

                Paragraph paragraph = new Paragraph();
                Document.Blocks.Add(paragraph);

                if (!string.IsNullOrEmpty(xml))
                {
                    TextRangeM rootTextRangeModel = ActiveContext.SerializationService.Deserialize<TextRangeM>(xml);

                    ActiveContext.FlowDocumentService.IterateThroughTextRangeM(rootTextRangeModel
                        , (textSegment, textRangeModel) => AddRunForTextRange(textSegment, textRangeModel, paragraph)
                        , (textSegment, textRangeModel) =>
                        {
                            paragraph = new Paragraph();
                            Document.Blocks.Add(paragraph);

                            AddRunForTextRange(textSegment, textRangeModel, paragraph);
                        });
                }
            }
            IsUnderDescriptionPropertyChangedScope = false;
        }

        private void AddRunForTextRange(string textSegment, TextRangeM textRangeModel, Paragraph paragraph)
        {
            Run run = new Run(textSegment);

            if (textRangeModel.isBold) run.FontWeight = FontWeights.Bold;
            if (textRangeModel.isItalic) run.FontStyle = FontStyles.Italic;
            if (textRangeModel.isUnderline) run.TextDecorations = TextDecorations.Underline;

            paragraph.Inlines.Add(run);
        }

        private bool IsUnderDescriptionPropertyChangedScope = false;

        public string Description
        {
            get => GetValue(DescriptionProperty)?.ToString();
            set => SetValue(DescriptionProperty, value);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!IsUnderDescriptionPropertyChangedScope)
            {
                if (DataContext is IRichTextSource richTextSource)
                {
                    string richTextModelString = ActiveContext.FlowDocumentService.ConvertTo(Document, ActiveContext.SerializationService);
                    string textString = ActiveContext.FlowDocumentService.GetTextFromFlowDoc(Document);
                    richTextSource.OnRichTextChanged(Tag?.ToString(), richTextModelString, textString);
                }
            }
        }

        public RichTextBoxWithTarget()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string xml = ActiveContext.FlowDocumentService.UnmaskXml((e.NewValue as IRichTextSource)?.Description);

            RefreshDocument(xml);
        }
    }
}