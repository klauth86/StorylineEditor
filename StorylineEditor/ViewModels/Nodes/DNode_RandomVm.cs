/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Случайная вершина")]
    [XmlRoot]
    public class DNode_RandomVm : Node_InteractiveVm
    {
        public DNode_RandomVm(TreeVm Parent) : base(Parent) { }

        public DNode_RandomVm() : this(null) { }

        public override bool IsValid => !string.IsNullOrEmpty(id) &&
            GameEvents.All(gameEvent => gameEvent?.IsValid ?? false) &&
            Predicates.All(predicate => predicate?.IsValid ?? false);

        public override string GenerateCode(string nodeName, bool isPlayerDialog)
        {
            bool hasPredicates = Predicates.Count > 0;

            var resultCode = string.Format("UDialogNode* {0} = nullptr;", nodeName) + Environment.NewLine;

            if (hasPredicates)
            {
                resultCode += string.Format("if (nodeId == \"{0}\" || ", Id) + Environment.NewLine;
                resultCode += string.Join(string.Format("&& {0}", Environment.NewLine), Predicates.Select(predicate => predicate.GenerateCode(nodeName)));
                resultCode += ") {" + Environment.NewLine;
            }

            resultCode += string.Format("{0} = NewObject<UDialogNode>(outer);", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->DialogNodeType = EDialogNodeType::RANDOM;", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->DialogId = \"{1}\";", nodeName, Parent.Id) + Environment.NewLine;
            resultCode += string.Format("{0}->NodeId = \"{1}\";", nodeName, Id) + Environment.NewLine;
            if (isPlayerDialog) resultCode += string.Format("{0}->IsInteractive = true;", nodeName) + Environment.NewLine;

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