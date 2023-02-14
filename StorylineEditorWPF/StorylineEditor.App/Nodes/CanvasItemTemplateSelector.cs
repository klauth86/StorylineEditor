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

using StorylineEditor.ViewModel.Interface;
using System.Windows;
using System.Windows.Controls;

namespace StorylineEditor.App.Nodes
{
    public class CanvasDataTemplateSelector: DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return (item is INode)
                ? Application.Current.FindResource(string.Format("{0}_ItemTemplate", item.GetType().Name)) as DataTemplate
                : base.SelectTemplate(item, container);
        }
    }
}
