/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Helpers;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;

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

                FlowDocument document = FlowDocumentHelper.ConvertBack(e.NewValue?.ToString());

                int i = 0;

                foreach (Block block in document.Blocks)
                {
                    if (block is Paragraph paragraph)
                    {
                        foreach (var inline in paragraph.Inlines)
                        {
                            string text = XamlWriter.Save(inline);
                            using (Stream ms = new MemoryStream(ASCIIEncoding.Default.GetBytes(XamlWriter.Save(inline))))
                            {
                                textBlock.Inlines.Add(XamlReader.Load(ms) as Inline);
                            }
                        }

                        if (i < document.Blocks.Count - 1) textBlock.Inlines.Add(new LineBreak());

                        i++;
                    }
                }
            }
        }

        public static void SetDocument(DependencyObject dp, string value) { dp.SetValue(DocumentProperty, value); }
        public static string GetDocument(DependencyObject dp) { return dp.GetValue(DocumentProperty)?.ToString(); }
    }
}