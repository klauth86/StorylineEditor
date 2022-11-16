using System;
using System.IO;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace StorylineEditor.ViewModel.Helpers
{
    public static class FlowDocumentHelper
    {
        public static string GetTextFromFlowDoc(FlowDocument document) { return document != null ? new TextRange(document.ContentStart, document.ContentEnd).Text : null; }

        public static string ConvertTo(FlowDocument document) { return XamlWriter.Save(document); }

        public static FlowDocument ConvertBack(string value)
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