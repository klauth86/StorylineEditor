/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.RichText;
using StorylineEditor.ViewModel;
using System;
using System.Text;

namespace StorylineEditor.App.Helpers
{
    public static class RichTextMHelper
    {
        private const string StartTagBracket = "<";
        private const string StartTagBracketMasked = "&lt;";
        private const string EndTagBracket = ">";
        private const string EndTagBracketMasked = "&gt;";

        public static string MaskXml(string xml) { return xml?.Replace(StartTagBracket, StartTagBracketMasked)?.Replace(EndTagBracket, EndTagBracketMasked); }

        public static string UnmaskXml(string maskedXml) { return maskedXml?.Replace(StartTagBracketMasked, StartTagBracket)?.Replace(EndTagBracketMasked, EndTagBracket); }

        private static string newLineString = new string('\n', 1);
        private static char[] Separator = { '\n' };

        public static void IterateThroughTextRangeM(TextRangeM rootTextRangeModel, Action<string, TextRangeM> firstRangeCallback, Action<string, TextRangeM> nextRangeCallback)
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

        public const string NewLineDecorString = "...";

        public static string GetTextString(string maskedXml)
        {
            string xml = UnmaskXml(maskedXml);

            if (!string.IsNullOrEmpty(xml))
            {
                TextRangeM rootTextRangeModel = ActiveContext.SerializationService.Deserialize<TextRangeM>(xml);
                
                foreach (var textRangeModel in rootTextRangeModel.subRanges)
                {
                    if (!string.IsNullOrEmpty(textRangeModel.content))
                    {
                        string[] textSegments = textRangeModel.content.Replace(Environment.NewLine, newLineString).Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                        return string.Join(NewLineDecorString, textSegments);
                    }
                }
            }

            return null;
        }
    }
}