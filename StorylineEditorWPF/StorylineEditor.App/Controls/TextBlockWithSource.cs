/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel.Interface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Controls
{
    public class TextBlockWithSource : TextBlock
    {
        public static readonly DependencyProperty RtVersionProperty = DependencyProperty.Register
            (
            "RtVersion",
            typeof(int),
            typeof(TextBlockWithSource),
            new FrameworkPropertyMetadata(int.MinValue, FrameworkPropertyMetadataOptions.None, RtVersionPropertyChanged)
            );

        public int RtVersion
        {
            get => (int)GetValue(RtVersionProperty);
            set => SetValue(RtVersionProperty, value);
        }

        private static void RtVersionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlockWithSource textBlockWithSource)
            {
                textBlockWithSource.RefreshInlines();
            }
        }

        private void RefreshInlines()
        {
            TextRangeM textRangeModel = IsLoaded && (DataContext is IRichTextSource richTextSource) ? richTextSource.GetRichText(Name) : TextRangeM.EmptyTextRange;

            Inlines.Clear();

            foreach (var subTextRangeModel in textRangeModel.subRanges)
            {
                if (subTextRangeModel.isNewLine)
                {
                    Inlines.Add(new LineBreak());
                }

                if (!string.IsNullOrEmpty(subTextRangeModel.content))
                {
                    Run run = new Run(subTextRangeModel.content);

                    if (subTextRangeModel.isBold) run.FontWeight = FontWeights.Bold;
                    if (subTextRangeModel.isItalic) run.FontStyle = FontStyles.Italic;
                    if (subTextRangeModel.isUnderline) run.TextDecorations = System.Windows.TextDecorations.Underline;

                    Inlines.Add(run);
                }
            }
        }

        public TextBlockWithSource()
        {
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RefreshInlines();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshInlines();
        }
    }
}