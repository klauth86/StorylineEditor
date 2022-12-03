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
using System.Collections.Generic;
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

        public override string Stats
        {
            get
            {
                string result = "";

                Dictionary<string, Dictionary<string, int>> countByCharacter = new Dictionary<string, Dictionary<string, int>>();
                Dictionary<string, int> countByTypeDescription = new Dictionary<string, int>();

                int characterNameMaxLength = 0;

                foreach (var node in Model.nodes)
                {
                    string characterName = "N/A";

                    string gender = " ";

                    if (node.gender == GENDER.MALE) gender = "👨";
                    if (node.gender == GENDER.FEMALE) gender = "👩";

                    if (!countByCharacter.ContainsKey(characterName))
                    {
                        countByCharacter.Add(characterName, new Dictionary<string, int>() { { " ", 0 }, { "👨", 0 }, { "👩", 0 } });
                    }

                    countByCharacter[characterName][gender]++;

                    var typeDescription = node.GetType().Name;
                    if (!countByTypeDescription.ContainsKey(typeDescription)) countByTypeDescription.Add(typeDescription, 0);
                    countByTypeDescription[typeDescription]++;
                }

                if (countByCharacter.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ПЕРСОНАЖАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByCharacter.OrderBy(pair => pair.Key))
                    {
                        result += string.Format("{0, -" + (characterNameMaxLength + 6) + "}{1}", "- " + entry.Key + ":", string.Join("\t", entry.Value.Select(pair => string.Format("{0}{1, -6}", pair.Key, pair.Value)))) + Environment.NewLine;
                    }
                }

                if (countByTypeDescription.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ТИПАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByTypeDescription.OrderBy(pair => pair.Key)) result += "- " + entry.Key + ": " + entry.Value + Environment.NewLine;
                }

                return result;
            }
        }
    }

    public class QuestEditorVM : Graph_BaseVM<QuestM>
    {
        public QuestEditorVM(QuestVM viewModel, ICallbackContext callbackContext, Func<Type, Point, BaseM> modelCreator, Func<BaseM, ICallbackContext, Notifier> viewModelCreator,
           Func<Notifier, ICallbackContext, Notifier> editorCreator, Type defaultNodeType) : base(viewModel.Model, callbackContext,
                modelCreator, viewModelCreator, editorCreator, defaultNodeType)
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