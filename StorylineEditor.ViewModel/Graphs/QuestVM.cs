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
using System.Windows.Input;

namespace StorylineEditor.ViewModel.Graphs
{
    public class QuestVM : BaseVM<QuestM>
    {
        public QuestVM(QuestM model, ICallbackContext callbackContext) : base(model, callbackContext) { }

        protected ICommand infoCommand;
        public ICommand InfoCommand => infoCommand ?? (infoCommand = new RelayCommand<Notifier>((viewModel) =>
        {
            CallbackContext?.Callback(this, nameof(ICallbackContext));
        }));

        public override string Stats => Graph_BaseVM<QuestM>.GetStats(Model);
    }

    public class QuestEditorVM : Graph_BaseVM<QuestM>
    {
        public QuestEditorVM(QuestVM viewModel, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
           Func<Notifier, ICallbackContext, Notifier> editorCreator, Type defaultNodeType) : base(viewModel.Model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, defaultNodeType)
        { }

        protected override string CanLinkNodes(INodeVM from, INodeVM to)
        {
            if (from == to) return nameof(ArgumentException);

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
            if (from is Node_Journal_AlternativeVM) // Remove all if linking Alternative to smth (with respect to Gender)
            {
                foreach (var fromlinkId in FromNodesLinks[from.Id].ToList())
                {
                    if (LinksVMs.ContainsKey(fromlinkId))
                    {
                        if (NodesVMs.ContainsKey(LinksVMs[fromlinkId].ToNodeId))
                        {
                            if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_StepVM existingTo)
                            {
                                if (existingTo.Gender * to.Gender == 0 || existingTo.Gender == to.Gender) RemoveLink(fromlinkId);
                            }
                        }
                    }
                }
            }
            else if (to is Node_Journal_AlternativeVM) // Remove all Steps if linking Step to Alternative
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
            else // Remove all if linking Step to Step (with respect to Gender)
            {
                foreach (var fromlinkId in FromNodesLinks[from.Id].ToList())
                {
                    if (LinksVMs.ContainsKey(fromlinkId))
                    {
                        if (NodesVMs.ContainsKey(LinksVMs[fromlinkId].ToNodeId))
                        {
                            if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_StepVM existingTo)
                            {
                                if (existingTo.Gender * to.Gender == 0 || existingTo.Gender == to.Gender) RemoveLink(fromlinkId);
                            }
                            else if (NodesVMs[LinksVMs[fromlinkId].ToNodeId] is Node_Journal_AlternativeVM existingAlternativeTo)
                            {
                                RemoveLink(fromlinkId);
                            }
                        }
                    }
                }
            }
        }

        public override string Title => null;
        public override string Stats => null;
    }
}