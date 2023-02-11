/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;

namespace StorylineEditor.App.Service
{
    public class FlowDocumentService : IFlowDocumentService
    {
        private static Dictionary<IRichTextSource, object> richTextViews = new Dictionary<IRichTextSource, object>();

        private static string newLineString = new string('\n', 1);
        private static char[] newLineDecorString = "...".ToCharArray();
        private static char[] Separator = { '\n' };

        public string GetTextFromFlowDoc(FlowDocument document)
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

        public string ConvertTo(FlowDocument document, ISerializationService serializationService)
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

            return MaskXml(serializationService.Serialize(rootTextRangeModel));
        }

        public FlowDocument ConvertBack(string value, ISerializationService serializationService)
        {
            FlowDocument flowDocument = new FlowDocument();

            Paragraph paragraph = new Paragraph();
            flowDocument.Blocks.Add(paragraph);

            if (!string.IsNullOrEmpty(value))
            {
                TextRangeM rootTextRangeModel = serializationService.Deserialize<TextRangeM>(UnmaskXml(value));

                IterateThroughTextRangeM(rootTextRangeModel
                    , (textSegment, textRangeModel) => AddRunForTextRange(textSegment, textRangeModel, paragraph)
                    , (textSegment, textRangeModel) =>
                    {
                        paragraph = new Paragraph();
                        flowDocument.Blocks.Add(paragraph);

                        AddRunForTextRange(textSegment, textRangeModel, paragraph);
                    });
            }

            return flowDocument;
        }

        private void AddRunForTextRange(string textSegment, TextRangeM textRangeModel, Paragraph paragraph)
        {
            Run run = new Run(textSegment);

            if (textRangeModel.isBold) run.FontWeight = FontWeights.Bold;
            if (textRangeModel.isItalic) run.FontStyle = FontStyles.Italic;
            if (textRangeModel.isUnderline) run.TextDecorations = TextDecorations.Underline;

            paragraph.Inlines.Add(run);
        }

        private const string StartTagBracket = "<";
        private const string StartTagBracketMasked = "&lt;";
        private const string EndTagBracket = ">";
        private const string EndTagBracketMasked = "&gt;";

        public string MaskXml(string xml) { return xml?.Replace(StartTagBracket, StartTagBracketMasked)?.Replace(EndTagBracket, EndTagBracketMasked); }

        public string UnmaskXml(string maskedXml) { return maskedXml?.Replace(StartTagBracketMasked, StartTagBracket)?.Replace(EndTagBracketMasked, EndTagBracket); }

        public void IterateThroughTextRangeM(TextRangeM rootTextRangeModel, Action<string, TextRangeM> firstRangeCallback, Action<string, TextRangeM> nextRangeCallback)
        {
            foreach (var textRangeModel in rootTextRangeModel.subRanges)
            {
                if (!string.IsNullOrEmpty(textRangeModel.content))
                {
                    string[] textSegments = textRangeModel.content.Replace(Environment.NewLine, newLineString).Split(Separator);

                    if (textSegments.Length > 0)
                    {
                        firstRangeCallback(textSegments[0], textRangeModel);

                        for (int i = 1; i < textSegments.Length; i++)
                        {
                            nextRangeCallback(textSegments[i], textRangeModel);
                        }
                    }
                }
            }
        }
    }
}