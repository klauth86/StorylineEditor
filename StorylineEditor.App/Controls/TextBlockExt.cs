/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StorylineEditor.App.Controls
{
    public class TextBlockExt: TextBlock
    {
        private static readonly DependencyProperty DocumentProperty = DependencyProperty.Register
            (
            "Document",
            typeof(string),
            typeof(TextBlockExt),
            new PropertyMetadata(null, OnDocumentPropertyChanged)
            );

        private static void OnDocumentPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp is TextBlock textBlock)
            {
                textBlock.Inlines.Clear();

                FlowDocument document = ViewModel.ActiveContext.FlowDocumentService.ConvertBack(e.NewValue?.ToString()); // TODO

                int i = 0;

                foreach (Block block in document.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        foreach (var inline in paragraph.Inlines)
                        {
                            if (inline is Run run)
                            {
                                textBlock.Inlines.Add(new Run(run.Text) { FontWeight = run.FontWeight, FontStyle = run.FontStyle, TextDecorations = run.TextDecorations });
                            }
                        }

                        if (i < document.Blocks.Count - 1) textBlock.Inlines.Add(new LineBreak());

                        i++;
                    }
                }
            }
        }

        public string Document
        {
            get => GetValue(DocumentProperty)?.ToString();
            set => SetValue(DocumentProperty, value);
        }
    }
}