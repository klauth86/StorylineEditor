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
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Tabs
{
    [XmlRoot]
    public class JournalRecordsTabVm : BaseTreesTabVm
    {
        public IEnumerable<Type> NodeTypes
        {
            get
            {
                yield return typeof(JNode_StepVm);
                yield return typeof(JNode_AlternativeVm);
            }
        }

        public JournalRecordsTabVm(FullContextVm Parent) : base(Parent) {
            selectedNodeType = typeof(JNode_StepVm);
        }

        public JournalRecordsTabVm() : this(null) { }

        protected override string GetElementTitle(bool isCreate) => isCreate ? "Создать квест" : "Редактировать квест";

        public override void GenerateCode(string folderPath)
        {
            // JOURNAL
            string resultCode = string.Empty;

            foreach (var item in Items)
            {
                if (item is TreeVm journal)
                {
                    var j = 1;
                    foreach (var rootNodeId in journal.RootNodeIds)
                    {
                        var rootNode = journal.Nodes.FirstOrDefault((node) => node.Id == rootNodeId);

                        Dictionary<string, string> nodesPassed = new Dictionary<string, string>();

                        var journalCode = "";

                        int i = 1;
                        foreach (var node in rootNode.ChildNodes)
                        {
                            var nodeName = string.Format("d{0}_node{1}", j, i);
                            journalCode += node.GenerateCode(nodeName, false);
                            journalCode += Environment.NewLine;

                            nodesPassed.Add(node?.Id, nodeName);
                            i++;
                        }

                        foreach (var link in journal.Links)
                        {
                            if (nodesPassed.ContainsKey(link.FromId))
                            {
                                var fromName = nodesPassed[link.FromId];
                                var toName = nodesPassed[link.ToId];
                                journalCode += string.Format("if ({0} && {1}) {0}->AddChild({1});", fromName, toName) + Environment.NewLine;
                            }
                        }

                        var rootNodeName = string.Format("d{0}_node1", j);

                        journalCode += Environment.NewLine;
                        journalCode += "if (result) return result;";
                        journalCode += Environment.NewLine;

                        journalCode = journalCode.Replace("{", "{{").Replace("}", "}}");
                        journalCode = "//=========================================================================================" + Environment.NewLine + "{0}" + Environment.NewLine + journalCode;
                        journalCode = journalCode + "{1}" + Environment.NewLine; // Global condition end

                        var condBegin = string.Format("if (\"{0}\" == rootNodeId || nodeId != \"\") {{{1}{1}UJournalNode_Base* result = nullptr;{1}", rootNode.Id, Environment.NewLine);
                        var condEnd = "}";

                        resultCode += string.Format(journalCode, condBegin, condEnd);
                        resultCode += Environment.NewLine;

                        j++;
                    }
                }
            }
            File.WriteAllText(Path.Combine(folderPath, "Journal.gen"), resultCode);

            // DICTIONARY
            resultCode = string.Empty;
            foreach (var item in Items)
            {
                if (item is TreeVm journal)
                {
                    foreach (var rootNodeId in journal.RootNodeIds)
                    {
                        var rootNode = journal.Nodes.FirstOrDefault((node) => node.Id == rootNodeId);

                        foreach (var node in journal.NodesTraversal(rootNode, false))
                        {

                            resultCode += string.Format("UpdateJournalNodes(\"{0}\", \"{1}\");", rootNode.Id, node.Id);
                            resultCode += Environment.NewLine;
                        }
                    }
                }
            }
            File.WriteAllText(Path.Combine(folderPath, "Journal_Dict.gen"), resultCode);
        }

        [XmlIgnore]
        public bool HasManyRoots => true;
    }
}
