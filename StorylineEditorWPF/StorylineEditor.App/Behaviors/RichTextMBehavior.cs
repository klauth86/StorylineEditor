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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Behaviors
{
    public static class RichTextMBehavior
    {
        private static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached
            (
            "Description",
            typeof(string),
            typeof(RichTextMBehavior),
            new PropertyMetadata(null, DescriptionPropertyChanged)
            );

        public static string GetDescription(this UIElement inUIElement) { return inUIElement.GetValue(DescriptionProperty)?.ToString(); }

        public static void SetDescription(this UIElement inUIElement, string value) { inUIElement.SetValue(DescriptionProperty, value); }

        private static void DescriptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                textBlock.Inlines.Clear();

                string maskedXml = e.NewValue?.ToString();

                if (!string.IsNullOrEmpty(maskedXml))
                {
                    TextRangeM rootTextRangeModel = ActiveContext.SerializationService.Deserialize<TextRangeM>(ActiveContext.FlowDocumentService.UnmaskXml(maskedXml));

                    ActiveContext.FlowDocumentService.IterateThroughTextRangeM(rootTextRangeModel
                        , (textSegment, textRangeModel) => AddRunForTextRange(textSegment, textRangeModel, textBlock)
                        , (textSegment, textRangeModel) =>
                        {
                            textBlock.Inlines.Add(new LineBreak());

                            AddRunForTextRange(textSegment, textRangeModel, textBlock);
                        });
                }
            }
        }

        private static void AddRunForTextRange(string textSegment, TextRangeM textRangeModel, TextBlock textBlock)
        {
            Run run = new Run(textSegment);

            if (textRangeModel.isBold) run.FontWeight = FontWeights.Bold;
            if (textRangeModel.isItalic) run.FontStyle = FontStyles.Italic;
            if (textRangeModel.isUnderline) run.TextDecorations = TextDecorations.Underline;

            textBlock.Inlines.Add(run);
        }
    }
}