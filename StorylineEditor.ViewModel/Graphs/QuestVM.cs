/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Nodes;
using System;
using System.Linq;
using System.Windows;

namespace StorylineEditor.ViewModel.Graphs
{
    public class QuestVM : BaseVM<QuestM>
    {
        public QuestVM(QuestM model) : base(model, null) { }
    }

    public class QuestEditorVM : Graph_BaseVM<QuestM>
    {
        public QuestEditorVM(QuestVM viewModel, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
            Func<Notifier, Notifier> editorCreator, Func<Notifier, BaseM> modelExtractor, Type defaultNodeType, Func<Type, string> typeDescriptor) : base(viewModel.Model, null,
                modelCreator, viewModelCreator, editorCreator, modelExtractor, defaultNodeType, typeDescriptor)
        { }

        protected override string CanLinkNodes(INodeVM from, INodeVM to)
        {
            if (from is Node_Journal_AlternativeVM && to is Node_Journal_AlternativeVM) return nameof(NotImplementedException);

            foreach (var linkId in FromNodesLinks[from.Id])
            {
                if (LinksVMs.ContainsKey(linkId))
                {
                    if (LinksVMs[linkId].ToNodeId == to.Id) return nameof(NotImplementedException);
                }
            }

            return string.Empty;
        }

        protected override void PreLinkNodes(INodeVM from, INodeVM to)
        {
            if (from is Node_Journal_AlternativeVM) // Remove all if linking Alternative to smth
            {
                foreach (var fromlinkId in FromNodesLinks[from.Id].ToList()) RemoveLink(fromlinkId);
            }
            else
            {
                if (to is Node_Journal_AlternativeVM) // Remove all Steps if linking Step to Alternative
                {
                    foreach (var fromlinkId in FromNodesLinks[from.Id].ToList())
                    {
                        if (LinksVMs.ContainsKey(fromlinkId))
                        {
                            if (NodesVMs.ContainsKey(LinksVMs[fromlinkId].ToNodeId))
                            {
                                if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_StepVM) RemoveLink(fromlinkId);
                            }
                        }
                    }
                }
                else // Remove all if linking Step to Step
                {
                    foreach (var fromlinkId in FromNodesLinks[from.Id].ToList()) RemoveLink(fromlinkId);
                }
            }
        }
    }
}