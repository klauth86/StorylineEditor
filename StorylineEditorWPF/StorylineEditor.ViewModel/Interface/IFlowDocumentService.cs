﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.RichText;
using System;
using System.Windows.Documents;

namespace StorylineEditor.ViewModel.Interface
{
    public interface IFlowDocumentService
    {
        string GetTextFromFlowDoc(FlowDocument document);

        string ConvertTo(FlowDocument document, ISerializationService serializationService);

        FlowDocument ConvertBack(string value, ISerializationService serializationService);

        string MaskXml(string xml);

        string UnmaskXml(string maskedXml);

        void IterateThroughTextRangeM(TextRangeM rootTextRangeModel, Action<string, TextRangeM> firstRangeCallback, Action<string, TextRangeM> nextRangeCallback);
    }
}