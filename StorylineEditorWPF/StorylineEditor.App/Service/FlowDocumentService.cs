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
using System.Windows;
using System.Windows.Documents;

namespace StorylineEditor.App.Service
{
    public class FlowDocumentService : IFlowDocumentService
    {
        const string StartTagBracket = "<";
        const string StartTagBracketMasked = "&lt;";
        const string EndTagBracket = ">";
        const string EndTagBracketMasked = "&gt;";

        static string newLineString = new string('\n', 1);
        static char[] Separator = { '\n' };

        public string GetTextFromFlowDoc(FlowDocument document) { return document != null ? new TextRange(document.ContentStart, document.ContentEnd).Text : null; }

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

            return serializationService.Serialize(rootTextRangeModel).Replace(StartTagBracket, StartTagBracketMasked).Replace(EndTagBracket, EndTagBracketMasked);
        }



        public FlowDocument ConvertBack(string value, ISerializationService serializationService)
        {
            FlowDocument flowDocument = new FlowDocument();

            Paragraph paragraph = new Paragraph();
            flowDocument.Blocks.Add(paragraph);

            if (!string.IsNullOrEmpty(value))
            {
                TextRangeM rootTextRangeModel = serializationService.Deserialize<TextRangeM>(value.Replace(StartTagBracketMasked, StartTagBracket).Replace(EndTagBracketMasked, EndTagBracket));

                foreach (var textRangeModel in rootTextRangeModel.subRanges)
                {
                    if (!string.IsNullOrEmpty(textRangeModel.content))
                    {
                        string[] textLines = textRangeModel.content.Replace(Environment.NewLine, newLineString).Split(Separator);

                        if (textLines.Length > 0)
                        {
                            AddRunForTextLine(textLines[0], textRangeModel.isBold, textRangeModel.isItalic, textRangeModel.isUnderline, paragraph);

                            for (int i = 1; i < textLines.Length; i++)
                            {
                                paragraph = new Paragraph();
                                flowDocument.Blocks.Add(paragraph);

                                AddRunForTextLine(textLines[i], textRangeModel.isBold, textRangeModel.isItalic, textRangeModel.isUnderline, paragraph);
                            }
                        }
                    }
                }
            }

            return flowDocument;
        }

        private void AddRunForTextLine(string textLine, bool isBold, bool isItalic, bool isUnderline, Paragraph paragraph)
        {
            Run run = new Run(textLine);

            if (isBold) run.FontWeight = FontWeights.Bold;
            if (isItalic) run.FontStyle = FontStyles.Italic;
            if (isUnderline) run.TextDecorations = TextDecorations.Underline;

            paragraph.Inlines.Add(run);
        }
    }
}