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

using System;
using System.Collections.Generic;
using System.Text;

namespace StorylineEditor.Model.RichText
{
    public struct TextRangeM
    {
        public const string NEW_LINE_DECORATOR = "...";

        public const int CYCLE = 10000;

        public static TextRangeM EmptyTextRange = new TextRangeM(0);

        public TextRangeM(int dummy)
        {
            isNewLine = false;
            isBold = false;
            isItalic = false;
            isUnderline = false;
            content = string.Empty;
            subRanges = new List<TextRangeM>() { new TextRangeM(false, false, false, false, string.Empty) };
        }

        private TextRangeM(bool inIsNewLine, bool inIsBold, bool inIsItalic, bool inIsUnderline, string inContent)
        {
            isNewLine = inIsNewLine;
            isBold = inIsBold;
            isItalic = inIsItalic;
            isUnderline = inIsUnderline;
            content = inContent;
            subRanges = null;
        }

        public bool isNewLine { get; set; }
        public bool isBold { get; set; }
        public bool isItalic { get; set; }
        public bool isUnderline { get; set; }

        public string content { get; set; }
        public List<TextRangeM> subRanges { get; set; }

        public void AddSubRange(bool isNewLine, bool isBold, bool isItalic, bool isUnderline, string content)
        {
            subRanges.Add(new TextRangeM(isNewLine, isBold, isItalic, isUnderline, content));
        }

        public bool PassFilter(string filter)
        {
            return !subRanges.TrueForAll((textRangeModel) => (textRangeModel.content?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) ?? -1) < 0);
        }

        public override string ToString() { return ToStringWithReplacement(NEW_LINE_DECORATOR); }

        public string ToStringWithReplacement(string replacement)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var textRangeModel in subRanges)
            {
                if (!string.IsNullOrEmpty(textRangeModel.content))
                {
                    if (textRangeModel.isNewLine)
                    {
                        stringBuilder.Append(replacement);
                    }

                    stringBuilder.Append(textRangeModel.content);
                }
            }

            return stringBuilder.ToString();
        }
    }
}