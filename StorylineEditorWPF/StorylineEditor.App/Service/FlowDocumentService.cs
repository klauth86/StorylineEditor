/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModel.Interface;
using System;
using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace StorylineEditor.App.Service
{
    public class FlowDocumentService : IFlowDocumentService
    {
        public string GetTextFromFlowDoc(FlowDocument document) { return document != null ? new TextRange(document.ContentStart, document.ContentEnd).Text : null; }

        public string ConvertTo(FlowDocument document) { return XamlWriter.Save(document); }

        public FlowDocument ConvertBack(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                using (var stringReader = new StringReader(value))
                {
                    try
                    {
                        using (var xmlTextReader = new XmlTextReader(stringReader))
                        {
                            return (FlowDocument)XamlReader.Load(xmlTextReader);
                        }
                    }
                    catch (Exception) { } ////// TODO
                }
            }

            return new FlowDocument();
        }
    }
}
