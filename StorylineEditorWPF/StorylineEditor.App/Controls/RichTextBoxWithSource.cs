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
using StorylineEditor.ViewModel.Interface;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Controls
{
    public class RichTextBoxWithSource : RichTextBox
    {
        public static readonly DependencyProperty RtVersionProperty = DependencyProperty.Register
            (
            "RtVersion",
            typeof(int),
            typeof(RichTextBoxWithSource),
            new FrameworkPropertyMetadata(int.MinValue, FrameworkPropertyMetadataOptions.None, RtVersionPropertyChanged)
            );

        public int RtVersion
        {
            get => (int)GetValue(RtVersionProperty);
            set => SetValue(RtVersionProperty, value);
        }

        private static void RtVersionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBoxWithSource richTextBoxWithSource)
            {
                richTextBoxWithSource.RefreshDocument();
            }
        }

        private void RefreshDocument()
        {
            TextRangeM textRangeModel = IsLoaded && (DataContext is IRichTextSource richTextSource) ? richTextSource.GetRichText(Name) : TextRangeM.EmptyTextRange;

            IsUnderDescriptionPropertyChangedScope = true;
            {
                Document.Blocks.Clear();

                Paragraph paragraph = new Paragraph();
                Document.Blocks.Add(paragraph);

                foreach (var subTextRangeModel in textRangeModel.subRanges)
                {
                    if (subTextRangeModel.isNewLine)
                    {
                        paragraph = new Paragraph();
                        Document.Blocks.Add(paragraph);
                    }

                    if (!string.IsNullOrEmpty(subTextRangeModel.content))
                    {
                        Run run = new Run(subTextRangeModel.content);

                        if (subTextRangeModel.isBold) run.FontWeight = FontWeights.Bold;
                        if (subTextRangeModel.isItalic) run.FontStyle = FontStyles.Italic;
                        if (subTextRangeModel.isUnderline) run.TextDecorations = TextDecorations.Underline;

                        paragraph.Inlines.Add(run);
                    }
                }
            }
            IsUnderDescriptionPropertyChangedScope = false;
        }

        private bool IsUnderDescriptionPropertyChangedScope = false;

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!IsUnderDescriptionPropertyChangedScope)
            {
                if (DataContext is IRichTextSource richTextSource)
                {
                    TextRangeM textRangeModel = new TextRangeM(0);

                    foreach (Block block in Document.Blocks)
                    {
                        if (block is Paragraph paragraph)
                        {
                            if (paragraph.Inlines.Count > 0)
                            {
                                foreach (Inline inline in paragraph.Inlines)
                                {
                                    if (inline is Run run)
                                    {
                                        bool isNewLine = run == paragraph.Inlines.FirstInline && paragraph != Document.Blocks.FirstBlock;
                                        bool isBold = run.FontWeight == FontWeights.Bold;
                                        bool isItalic = run.FontStyle == FontStyles.Italic;
                                        bool isUnderline = run.TextDecorations == TextDecorations.Underline;

                                        textRangeModel.AddSubRange(isNewLine, isBold, isItalic, isUnderline, run.Text);
                                    }
                                    else
                                    {
                                        throw new ArgumentOutOfRangeException(nameof(inline));
                                    }
                                }
                            }
                            else
                            {
                                textRangeModel.AddSubRange(true, false, false, false, string.Empty);
                            }
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException(nameof(block));
                        }
                    }

                    richTextSource.SetRichText(Name, ref textRangeModel);
                }
            }
        }

        public RichTextBoxWithSource()
        {
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RefreshDocument();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshDocument();
        }
    }
}