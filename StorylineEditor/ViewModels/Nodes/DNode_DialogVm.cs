/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Views.Nodes;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public class DNode_DialogVm : DNode_CharacterVm
    {
        public DNode_DialogVm(TreeVm Parent) : base(Parent)
        {
            isNondialogNode = false;
        }

        public DNode_DialogVm() : this(null) { }

        public override bool IsValid
        {
            get
            {
                return base.IsValid && (!IsNondialogNode ||
                    Parent.NodesTraversal(this, false).ToList().TrueForAll(childNode => !(childNode is IOwnered ownered) || ownered.Owner.Id != CharacterVm.PlayerId));
            }
        }

        protected bool isNondialogNode;
        public bool IsNondialogNode
        {
            get
            {
                return isNondialogNode;
            }
            set
            {
                if (value != isNondialogNode)
                {
                    isNondialogNode = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is DNode_DialogVm casted)
            {
                casted.isNondialogNode = isNondialogNode;
            }
        }

        public override string GenerateCode(string nodeName, bool isInteractive)
        {
            var resultCode = string.Format("UDialogNode* {0} = nullptr;", nodeName) + Environment.NewLine;

            {
                resultCode += string.Format("if (nodeId == \"{0}\" || ", Id) + Environment.NewLine;
                resultCode += string.Format("gender & {0}", Gender == UNISEX ? 3 : Gender); ////// TODO
                if (Predicates.Count > 0) resultCode += "&& " + string.Join(string.Format("&& {0}", Environment.NewLine), Predicates.Select(predicate => predicate.GenerateCode(nodeName)));
                resultCode += ") {" + Environment.NewLine;
            }

            resultCode += string.Format("{0} = NewObject<UDialogNode>(outer);", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->DialogNodeType = EDialogNodeType::REGULAR;", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->DialogId = \"{1}\";", nodeName, Parent.Id) + Environment.NewLine;
            resultCode += string.Format("{0}->NodeId = \"{1}\";", nodeName, Id) + Environment.NewLine;
            resultCode += string.Format("{0}->OwnerClassPath = \"{1}\";", nodeName, Owner?.ClassPathName ?? "") + Environment.NewLine;
            resultCode += string.Format("{0}->Content = LOCTEXT(\"{1}\", \"{2}\");", nodeName, Id, RTBHelper.GetFlowDocumentContent(Name)) + Environment.NewLine;
            resultCode += string.Format("{0}->Description = LOCTEXT(\"{1}\", \"{2}\");", nodeName, Id, Description) + Environment.NewLine;

            var allActors = Parent.Parent.Parent.AllActors.ToList();

            //////foreach (var characterId in LockedCharacterIds)
            //////{
            //////    resultCode += string.Format("{0}->LockedParticipants.Add(\"{1}\");", nodeName,
            //////        allActors.FirstOrDefault(actor => actor.Id == characterId)?.ClassPathName ?? "") + Environment.NewLine;
            //////}

            //////foreach (var characterId in UnlockedCharacterIds)
            //////{
            //////    resultCode += string.Format("{0}->UnlockedParticipants.Add(\"{1}\");", nodeName,
            //////        allActors.FirstOrDefault(actor => actor.Id == characterId)?.ClassPathName ?? "") + Environment.NewLine;
            //////}

            for (int i = 0; i < FocusActors.Count; i++)
            {
                var focusByName = FocusesByName.Contains(FocusActors[i]);
                var actorToLookAt = allActors.FirstOrDefault(actor => actor.Id == FocusTargets[i]);
                var focusPointString = focusByName ? (actorToLookAt?.ActorName ?? "") : (actorToLookAt?.ClassPathName ?? "");
                resultCode += string.Format("{0}->Focuses.Add(\"{1}\", FParticipantFocus({2}, \"{3}\"));", nodeName,
                    allActors.FirstOrDefault(actor => actor.Id == FocusActors[i])?.ClassPathName ?? "", focusByName ? "true" : "false",
                    focusPointString) + Environment.NewLine;
            }

            if (isInteractive && !isNondialogNode) resultCode += string.Format("{0}->IsInteractive = true;", nodeName) + Environment.NewLine;

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

            resultCode += "}" + Environment.NewLine;

            return resultCode;
        }
    }
}
