/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Views.Nodes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Журнальная вершина")]
    [XmlRoot]
    public class JNode_BaseVm : JNode_AlternativeVm
    {
        public JNode_BaseVm(TreeVm Parent) : base(Parent) { }

        public JNode_BaseVm() : this(null) { }

        public override bool IsValid => !string.IsNullOrEmpty(id) &&
            GameEvents.All(gameEvent => gameEvent?.IsValid ?? false) &&
            Predicates.All(predicate => predicate?.IsValid ?? false);

        public override bool AllowsManyChildren => false;

        public override string GenerateCode(string nodeName, bool isPlayerDialog)
        {
            bool hasPredicates = Predicates.Count > 0;

            var resultCode = string.Format("UJournalNode_Base* {0} = nullptr;", nodeName) + Environment.NewLine;

            if (hasPredicates)
            {
                resultCode += string.Format("if (nodeId == \"{0}\" || ", Id) + Environment.NewLine;
                resultCode += string.Join(string.Format("&& {0}", Environment.NewLine), Predicates.Select(predicate => predicate.GenerateCode(nodeName)));
                resultCode += ") {" + Environment.NewLine;
            }

            var rootNodes = Parent.GetRootNodes(this);
            if (rootNodes.Count > 1 || rootNodes.Count <= 0) throw new ArgumentOutOfRangeException();

            resultCode += string.Format("{0} = NewObject<UJournalNode_Base>(outer);", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->NodeType = EDialogNodeType::REGULAR;", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->RootNodeId = \"{1}\";", nodeName, rootNodes[0].Id) + Environment.NewLine;
            resultCode += string.Format("{0}->NodeId = \"{1}\";", nodeName, Id) + Environment.NewLine;
            resultCode += string.Format("{0}->X = {1};", nodeName, (int)PositionX) + Environment.NewLine;
            resultCode += string.Format("{0}->Y = {1};", nodeName, (int)PositionY) + Environment.NewLine;
            
            var preContent = IsRoot ? Label : RTBHelper.GetFlowDocumentContent(Name);
            if (!string.IsNullOrEmpty(preContent))
                resultCode += string.Format("{0}->PreContent = LOCTEXT(\"{1}_Pre\", \"{2}\");", nodeName, Id, preContent?.Replace("\"", "\\\"")) + Environment.NewLine;
            else
                resultCode += string.Format("{0}->PreContent = FText::GetEmpty();", nodeName) + Environment.NewLine;
            
            if (!string.IsNullOrEmpty(Description)) 
                resultCode += string.Format("{0}->PostContent = LOCTEXT(\"{1}_Post\", \"{2}\");", nodeName, Id, Description) + Environment.NewLine;
            else
                resultCode += string.Format("{0}->PostContent = FText::GetEmpty();", nodeName) + Environment.NewLine;
            
            if (IsRoot) resultCode += string.Format("{0}->IsRootNode = true;", nodeName) + Environment.NewLine;
            if (IsLeaf) resultCode += string.Format("{0}->IsLeafNode = true;", nodeName) + Environment.NewLine;

            ////// TODO
            //////if (GameEvents.Count > 0)
            //////{
            //////    resultCode += Environment.NewLine + "{" + Environment.NewLine;
            //////    resultCode += string.Join(
            //////        Environment.NewLine, GameEvents.Select(gameEvent =>
            //////        gameEvent.GenerateCode(string.Format("{0}_event{1}", nodeName, GameEvents.IndexOf(gameEvent)), nodeName) +
            //////        string.Format("{0}->{1}.Add({2});", nodeName, gameEvent.ExecuteWhenLeaveDialogNode ? "LeaveGameEvents" : "EnterGameEvents", string.Format("{0}_event{1}", nodeName, GameEvents.IndexOf(gameEvent)), nodeName) +
            //////        Environment.NewLine));
            //////    resultCode += "}" + Environment.NewLine;
            //////}

            if (IsRoot)
                resultCode += string.Format("if (nodeId == \"{0}\" || nodeId == \"\") result = {1};", id, nodeName) + Environment.NewLine;
            else
                resultCode += string.Format("if (nodeId == \"{0}\") result = {1};", id, nodeName) + Environment.NewLine;

            if (hasPredicates) resultCode += "}" + Environment.NewLine;

            return resultCode;
        }
    }
}