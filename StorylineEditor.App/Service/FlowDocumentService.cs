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
