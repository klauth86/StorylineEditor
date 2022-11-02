/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace StorylineEditor.App.Controls
{
    public class RichTextBoxExt : RichTextBox
    {
        protected bool setDocumentTextIsBlocked = false;

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (!setDocumentTextIsBlocked) SetDocumentText(this, XamlWriter.Save(Document));
        }

        private TextRange textRange;

        private static readonly DependencyProperty DocumentTextProperty = DependencyProperty.Register
            (
            "DocumentText",
            typeof(string),
            typeof(RichTextBoxExt),
            new PropertyMetadata(DocumentTextPropertyChanged)
            );

        public static void SetDocumentText(DependencyObject dp, string value) { dp.SetValue(DocumentTextProperty, value); }
        public static string GetDocumentText(DependencyObject dp) { return dp.GetValue(DocumentTextProperty)?.ToString(); }

        private static void DocumentTextPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue != DependencyProperty.UnsetValue && !string.IsNullOrEmpty(args.NewValue?.ToString()))
            {
                if (dp is RichTextBoxExt rtbExt)
                {
                    FlowDocument newDocument = ConvertToFlowDocument(args.NewValue.ToString());

                    if (newDocument != null)
                    {
                        rtbExt.setDocumentTextIsBlocked = true;

                        ////TextRange textRange = new TextRange(rtbExt.Document.ContentStart, rtbExt.Document.ContentEnd);

                        ////TextRange newTextRange = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);

                        //////using (MemoryStream ms = new MemoryStream())
                        //////{
                        //////    newTextRange.Save(ms, DataFormats.Rtf);

                        //////    ms.Seek(0, SeekOrigin.Begin);

                        //////    textRange.Load(ms, DataFormats.Rtf);
                        //////}

                        rtbExt.setDocumentTextIsBlocked = false;
                    }
                }
            }
        }

        private static FlowDocument ConvertToFlowDocument(string newValue)
        {
            using (var stringReader = new StringReader(newValue))
            {
                try
                {
                    using (var xmlTextReader = new XmlTextReader(stringReader))
                    {
                        return (FlowDocument)XamlReader.Load(xmlTextReader);
                    }
                }
                catch (Exception) { }
            }

            return null;
        }
    }
}