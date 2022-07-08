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
using System.Windows;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public class ReplicasTabVm : BaseTreesTabVm
    {
        public IEnumerable<Type> NodeTypes
        {
            get
            {
                yield return typeof(DNode_CharacterVm);
                yield return typeof(DNode_RandomVm);
                yield return typeof(DNode_TransitVm);
                yield return typeof(DNode_VirtualVm);
            }
        }

        public ReplicasTabVm(FullContextVm Parent) : base(Parent)
        {
            selectedNodeType = typeof(DNode_CharacterVm);
        }

        public ReplicasTabVm() : this(null) { }

        protected override string GetElementTitle(bool isCreate) => isCreate ? "Создать реплику" : "Редактировать реплику";

        public override void GenerateCode(string folderPath)
        {
            // REPLICAS
            string resultCode = string.Empty;
            Dictionary<string, string> nodesPassed = new Dictionary<string, string>();
            Dictionary<TreeVm, List<FolderedVm>> replicasOwners = new Dictionary<TreeVm, List<FolderedVm>>();
            foreach (var item in Items)
            {
                if (item is TreeVm replica)
                {
                    if (replica.RootNodeIds.Count != 1)
                    {
                        MessageBox.Show(string.Format("{0}: Отсутствует корневая вершина ({1})!", Name, replica.Name),
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!replicasOwners.ContainsKey(replica)) replicasOwners.Add(replica, new List<FolderedVm>());

                    var dialogCode = "";
                    int j = items.IndexOf(replica);

                    int i = 1;
                    foreach (var node in replica.NodesTraversal())
                    {
                        if (node is IOwnered ownered && ownered.Owner != null && !replicasOwners[replica].Contains(ownered.Owner)) replicasOwners[replica].Add(ownered.Owner);

                        var nodeName = string.Format("d{0}_node{1}", j, i);
                        dialogCode += node.GenerateCode(nodeName, false);
                        dialogCode += Environment.NewLine;

                        nodesPassed.Add(node?.Id, nodeName);
                        i++;
                    }

                    foreach (var link in replica.Links)
                    {
                        var fromName = nodesPassed[link.FromId];
                        var toName = nodesPassed[link.ToId];
                        dialogCode += string.Format("if ({0} && {1}) {0}->AddChild({1});", fromName, toName) + Environment.NewLine;
                    }

                    var rootNodeName = string.Format("d{0}_node1", items.IndexOf(replica));

                    dialogCode += Environment.NewLine;
                    dialogCode += "if (result) return result;";
                    dialogCode += Environment.NewLine;

                    dialogCode = dialogCode.Replace("{", "{{").Replace("}", "}}");
                    dialogCode = "//=========================================================================================" + Environment.NewLine + "{0}" + Environment.NewLine + dialogCode;
                    dialogCode = dialogCode + "{1}" + Environment.NewLine; // Global condition end

                    var condBegin = string.Format("if (\"{0}\" == dialogId || nodeId != \"\") {{{1}{1}UDialogNode* result = nullptr;{1}", replica.Id, Environment.NewLine);
                    var condEnd = "}";

                    resultCode += string.Format(dialogCode, condBegin, condEnd);
                    resultCode += Environment.NewLine;
                }
            }
            File.WriteAllText(Path.Combine(folderPath, "Dialogs_Replica.gen"), resultCode);



            // DICTIONARY
            resultCode = string.Empty;
            foreach (var item in Items)
            {
                if (item is TreeVm replica)
                {
                    resultCode += string.Format("FReplica::AddReplica(\"{0}\", L\"{1}\");", replica.Id, replica.Name);
                    resultCode += Environment.NewLine;
                }
            }
            File.WriteAllText(Path.Combine(folderPath, "Dialogs_Replica_Dict.gen"), resultCode);
        }

        [XmlIgnore]
        public bool HasManyRoots => false;
    }
}