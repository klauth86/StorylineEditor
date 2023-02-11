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

using StorylineEditor.ViewModel;
using StorylineEditor.ViewModel.Interface;
using System.Windows.Controls;

namespace StorylineEditor.App.Controls
{
    public class RichTextBoxWithTarget : RichTextBox
    {
        public RichTextBoxWithTarget()
        {

        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (DataContext is IRichTextSource richTextSource)
            {
                string richTextModelString = ActiveContext.FlowDocumentService.ConvertTo(Document, ActiveContext.SerializationService);
                string textString = ActiveContext.FlowDocumentService.GetTextFromFlowDoc(Document);
                richTextSource.OnRichTextChanged(richTextModelString, textString);
            }
        }
    }
}