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

using StorylineEditor.App.Helpers;
using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Text;
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
                string xml = RichTextMHelper.UnmaskXml(e.NewValue?.ToString());

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

                    RichTextMHelper.IterateThroughTextRangeM(rootTextRangeModel
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
                    string richTextModelString = GetRichTextModelString(Document, ActiveContext.SerializationService);
                    string textString = GetTextString(Document);
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
            string xml = RichTextMHelper.UnmaskXml((e.NewValue as IRichTextSource)?.Description);

            RefreshDocument(xml);
        }

        private static char[] newLineDecorString = "...".ToCharArray();

        public string GetRichTextModelString(FlowDocument document, ISerializationService serializationService)
        {
            TextRangeM rootTextRangeModel = new TextRangeM();

            int paragraphIndex = 0;
            int oldStyle = -1;

            foreach (Block block in document.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    if (paragraphIndex > 0)
                    {
                        if (rootTextRangeModel.subRanges.Count == 0)
                        {
                            rootTextRangeModel.subRanges.Add(new TextRangeM());
                            oldStyle = 0;
                        }
                        rootTextRangeModel.subRanges[rootTextRangeModel.subRanges.Count - 1].content += Environment.NewLine;
                    }

                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            bool isBold = run.FontWeight == FontWeights.Bold;
                            bool isItalic = run.FontStyle == FontStyles.Italic;
                            bool isUnderline = run.TextDecorations == TextDecorations.Underline;

                            int newStyle = (isBold ? 1 : 0) + (isItalic ? 2 : 0) + (isUnderline ? 4 : 0);

                            if (newStyle != oldStyle)
                            {
                                rootTextRangeModel.subRanges.Add(new TextRangeM() { isBold = isBold, isItalic = isItalic, isUnderline = isUnderline });
                                oldStyle = newStyle;
                            }

                            rootTextRangeModel.subRanges[rootTextRangeModel.subRanges.Count - 1].content += run.Text;
                        }
                        else if (inline is LineBreak)
                        {
                            continue;
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(nameof(inline));
                        }
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(block));
                }

                paragraphIndex++;
            }

            return RichTextMHelper.MaskXml(serializationService.Serialize(rootTextRangeModel));
        }

        public string GetTextString(FlowDocument document)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (document != null)
            {
                foreach (var block in document.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        string paragraphText = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text;
                        if (!string.IsNullOrEmpty(paragraphText))
                        {
                            if (block != document.Blocks.FirstBlock)
                            {
                                stringBuilder.Append(newLineDecorString);
                            }
                            stringBuilder.Append(paragraphText);
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}