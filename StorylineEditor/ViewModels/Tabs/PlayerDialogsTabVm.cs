/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public class PlayerDialogsTabVm : BaseTreesTabVm
    {
        public IEnumerable<Type> NodeTypes
        {
            get
            {
                yield return typeof(DNode_DialogVm);
                yield return typeof(DNode_RandomVm);
                yield return typeof(DNode_TransitVm);
                yield return typeof(DNode_VirtualVm);
            }
        }

        public PlayerDialogsTabVm(FullContextVm Parent) : base(Parent)
        {
            selectedNodeType = typeof(DNode_DialogVm);
        }

        public PlayerDialogsTabVm() : this(null) { }

        protected override string GetElementTitle(bool isCreate) => isCreate ? "Создать диалог" : "Редактировать диалог";

        public override void GenerateCode(string folderPath)
        {
            string resultCode = string.Empty;
            Dictionary<string, string> nodesPassed = new Dictionary<string, string>();
            List<FolderedVm> participants = new List<FolderedVm>();

            int j = 1;
            foreach (FolderedVm foldered in Items)
            {
                foreach (var item in foldered.FoldersTraversal())
                {
                    if (item is TreeVm playerDialog)
                    {
                        if (playerDialog.RootNodeIds.Count != 1)
                        {
                            MessageBox.Show(string.Format("{0}: Отсутствует корневая вершина ({1})!", Name, playerDialog.Name),
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (playerDialog.Participants.All(participant => participant == CharacterVm.PlayerId))
                        {
                            MessageBox.Show(string.Format("{0}: в диалоге с игроком должно быть не менее одного участника Npc!", Name, playerDialog.Name),
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        participants.Clear();

                        var dialogCode = "";

                        int i = 1;
                        foreach (var node in playerDialog.NodesTraversal())
                        {
                            if (node is IOwnered ownered && ownered.Owner != null && !participants.Contains(ownered.Owner)) participants.Add(ownered.Owner);

                            var nodeName = string.Format("d{0}_node{1}", j, i);
                            dialogCode += node.GenerateCode(nodeName, true);
                            dialogCode += Environment.NewLine;

                            nodesPassed.Add(node?.Id, nodeName);
                            i++;
                        }

                        foreach (var link in playerDialog.Links)
                        {
                            var fromName = nodesPassed[link.FromId];
                            var toName = nodesPassed[link.ToId];
                            dialogCode += string.Format("if ({0} && {1}) {0}->AddChild({1});", fromName, toName) + Environment.NewLine;
                        }

                        var rootNodeName = string.Format("d{0}_node1", items.IndexOf(playerDialog));

                        dialogCode += Environment.NewLine;
                        dialogCode += "if (result) return result;";
                        dialogCode += Environment.NewLine;

                        dialogCode = dialogCode.Replace("{", "{{").Replace("}", "}}");
                        dialogCode = "//=========================================================================================" + Environment.NewLine + "{0}" + Environment.NewLine + dialogCode;
                        dialogCode = dialogCode + "{1}" + Environment.NewLine; // Global condition end

                        var npcParticipant = participants.FirstOrDefault(participant => participant.Id != CharacterVm.PlayerId);

                        var condBegin = string.Format("if (characterBClass->GetPathName() == \"{0}\" || nodeId != \"\") {{{1}{1}UDialogNode* result = nullptr;{1}", npcParticipant.ClassPathName, Environment.NewLine);
                        string condEnd = "}";

                        resultCode += string.Format(dialogCode, condBegin, condEnd);
                        resultCode += Environment.NewLine;

                        j++;
                    }
                }
            }

            File.WriteAllText(Path.Combine(folderPath, "Dialogs_Player.gen"), resultCode);
        }

        [XmlIgnore]
        public bool HasManyRoots => false;
    }
}