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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Behaviors
{
    public static class RichTextMBehavior
    {
        public static readonly DependencyProperty RtVersionProperty = DependencyProperty.RegisterAttached
            (
            "RtVersion",
            typeof(int),
            typeof(RichTextMBehavior),
            new PropertyMetadata(-1, RtVersionPropertyChanged)
            );

        public static int GetRtVersion(this UIElement inUIElement) { return (int)inUIElement.GetValue(RtVersionProperty); }
        public static void SetRtVersion(this UIElement inUIElement, int value) { inUIElement.SetValue(RtVersionProperty, value); }

        private static void RtVersionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                RefreshDocument(textBlock);
            }
        }

        private static void RefreshDocument(TextBlock textBlock)
        {
            TextRangeM textRangeModel = (textBlock.DataContext as IRichTextSource)?.GetRichText(textBlock.Name) ?? TextRangeM.EmptyTextRange;

            textBlock.Inlines.Clear();

            foreach (var subTextRangeModel in textRangeModel.subRanges)
            {
                if (subTextRangeModel.isNewLine)
                {
                    textBlock.Inlines.Add(new LineBreak());
                }

                if (!string.IsNullOrEmpty(subTextRangeModel.content))
                {
                    Run run = new Run(subTextRangeModel.content);

                    if (subTextRangeModel.isBold) run.FontWeight = FontWeights.Bold;
                    if (subTextRangeModel.isItalic) run.FontStyle = FontStyles.Italic;
                    if (subTextRangeModel.isUnderline) run.TextDecorations = TextDecorations.Underline;

                    textBlock.Inlines.Add(run);
                }
            }
        }
    }
}