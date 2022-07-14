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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public class ParticipantStateVm : BaseVm<DNode_CharacterVm>
    {
        public ParticipantStateVm(DNode_CharacterVm Parent, FolderedVm inCharacter) : base(Parent)
        {
            character = inCharacter;
            CharacterId = character?.Id ?? null;

            SetupParenthood();
        }

        public ParticipantStateVm() : this(null, null) { }

        public string CharacterId { get; set; }

        protected FolderedVm character;
        public FolderedVm Character => character;

        protected bool isLocked;
        public bool IsLocked
        {
            get => isLocked;
            set
            {
                if (value != isLocked)
                {
                    isLocked = value;
                    NotifyWithCallerPropName();

                    if (isLocked)
                    {
                        Parent?.AddLockedCharacter(CharacterId);
                    }
                    else
                    {
                        Parent?.RemoveLockedCharacter(CharacterId);
                    }

                    Notify(nameof(IsSet));
                }
            }
        }

        protected bool isUnlocked;
        public bool IsUnlocked
        {
            get => isUnlocked;
            set
            {
                if (value != isUnlocked)
                {
                    isUnlocked = value;
                    NotifyWithCallerPropName();

                    if (isUnlocked)
                    {
                        Parent?.AddUnlockedCharacter(CharacterId);
                    }
                    else
                    {
                        Parent?.RemoveUnlockedCharacter(CharacterId);
                    }

                    Notify(nameof(IsSet));
                }
            }
        }

        public string FocusActorId { get; set; }

        [XmlIgnore]
        public BaseVm FocusActor {
            get => Parent?.Parent.Parent.Parent.AllActors.FirstOrDefault(item=>item.Id == FocusActorId);
            set {
                if (value != FocusActor) {
                    if (FocusActorId != null) Parent?.RemoveFocusActor(CharacterId);

                    FocusActorId = value?.Id;
                    NotifyWithCallerPropName();

                    if (FocusActorId != null) Parent?.AddFocusActor(CharacterId, FocusActorId);
                }
            }
        }

        protected bool searchByName;
        public bool SearchByName
        {
            get => searchByName;
            set
            {
                if (value != searchByName)
                {
                    searchByName = value;
                    NotifyWithCallerPropName();

                    if (searchByName)
                    {
                        Parent?.AddFocusByName(CharacterId);
                    }
                    else
                    {
                        Parent?.RemoveFocusByName(CharacterId);
                    }
                }
            }
        }

        public bool IsSet => IsLocked || IsUnlocked;

        public void Reset()
        {
            IsLocked = false;
            IsUnlocked = false;
            FocusActor = null;
            SearchByName = false;
        }

        public override void SetupParenthood()
        {
            if (Parent != null)
            {
                character = Parent.Parent.Parent.Parent.CharactersTab.Items.FirstOrDefault((charac) => charac.Id == CharacterId);

                isLocked = Parent.LockedCharacters.Contains(character);
                isUnlocked = Parent.UnlockedCharacters.Contains(character);
                FocusActorId = Parent.FocusActors.Contains(CharacterId) ? Parent.FocusTargets[Parent.FocusActors.IndexOf(CharacterId)] : null;
                searchByName = Parent.FocusesByName.Contains(CharacterId);
            }
            else {
                isLocked = false;
                isUnlocked = false;
                FocusActorId = null;
                searchByName = false;
            }
        }
    }

    [Description("Регулярная вершина")]
    [XmlRoot]
    public class DNode_CharacterVm : Node_InteractiveVm, IOwnered
    {
        public DNode_CharacterVm(TreeVm Parent) : base(Parent)
        {
            OwnerId = CharacterVm.PlayerId;

            LockedCharacterIds = new List<string>();
            UnlockedCharacterIds = new List<string>();
            FocusActors = new List<string>();
            FocusTargets = new List<string>();
            FocusesByName = new List<string>();

            ParticipantStates = new ObservableCollection<ParticipantStateVm>();
        }

        public DNode_CharacterVm() : this(null) { }

        public string OwnerId { get; set; }

        [XmlIgnore]
        public FolderedVm Owner
        {
            get
            {
                return Parent?.Parent.Parent.CharactersTab?.Items
                    .FirstOrDefault(item => item?.Id == OwnerId);
            }
            set
            {
                if (OwnerId != value?.Id)
                {
                    Parent?.RemoveParticipant(Owner);
                    OwnerId = value?.Id;
                    Parent?.AddParticipant(Owner);

                    NotifyWithCallerPropName();
                    Notify(nameof(OwnerId));
                    NotifyIsValidChanged();
                }
            }
        }

        protected string attachedFile;
        public string AttachedFile
        {
            get => string.IsNullOrEmpty(attachedFile) ? null : attachedFile;
            set
            {
                if (value != attachedFile)
                {
                    attachedFile = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        public override bool IsValid => base.IsValid && Owner != null;

        [XmlArray]
        public ObservableCollection<ParticipantStateVm> ParticipantStates { get; }
        public void RemoveParticipantState(FolderedVm character)
        {
            List<ParticipantStateVm> statesToRemove = new List<ParticipantStateVm>();
            foreach (var participantState in ParticipantStates)
            {
                if (participantState.Character == character) {
                    statesToRemove.Add(participantState);
                }
            }
            foreach (var stateToRemove in statesToRemove)
            {
                stateToRemove.Reset();
                ParticipantStates.Remove(stateToRemove);
            }
        }



        public List<string> LockedCharacterIds { get; set; }
        [XmlIgnore]
        public List<FolderedVm> LockedCharacters {
            get {
                if (Parent != null) {
                    var characters = Parent.Parent.Parent.CharactersTab.Items;
                    return characters.Where((character) => LockedCharacterIds.Contains(character.Id)).ToList();
                }
                return null;
            }
        }
        public void AddLockedCharacter(string characterId) { if (!LockedCharacterIds.Contains(characterId)) { LockedCharacterIds.Add(characterId); Notify(nameof(LockedCharacters)); } }
        public void RemoveLockedCharacter(string characterId) { if (LockedCharacterIds.Contains(characterId)) { LockedCharacterIds.Remove(characterId); Notify(nameof(LockedCharacters)); } }



        public List<string> UnlockedCharacterIds { get; set; }
        [XmlIgnore]
        public List<FolderedVm> UnlockedCharacters {
            get
            {
                if (Parent != null)
                {
                    var characters = Parent.Parent.Parent.CharactersTab.Items;
                    return characters.Where((character) => UnlockedCharacterIds.Contains(character.Id)).ToList();
                }
                return null;
            }
        }
        public void AddUnlockedCharacter(string characterId) { if (!UnlockedCharacterIds.Contains(characterId)) { UnlockedCharacterIds.Add(characterId); Notify(nameof(UnlockedCharacters)); } }
        public void RemoveUnlockedCharacter(string characterId) { if (UnlockedCharacterIds.Contains(characterId)) { UnlockedCharacterIds.Remove(characterId); Notify(nameof(UnlockedCharacters)); } }



        public List<string> FocusActors { get; set; }
        public List<string> FocusTargets { get; set; }
        public void AddFocusActor(string characterId, string actorId) {
            if (!FocusActors.Contains(characterId))
            {
                FocusActors.Add(characterId);
                FocusTargets.Add(actorId);
            }
            else {
                FocusTargets[FocusActors.IndexOf(characterId)] = actorId;
            }
        }
        public void RemoveFocusActor(string characterId) {
            if (!FocusActors.Contains(characterId))
            {
                FocusTargets.RemoveAt(FocusActors.IndexOf(characterId));
                FocusActors.Remove(characterId);
            }
        }



        public List<string> FocusesByName { get; set; }
        public void AddFocusByName(string characterId) { if (!FocusesByName.Contains(characterId)) FocusesByName.Add(characterId); }
        public void RemoveFocusByName(string characterId) { FocusesByName.Remove(characterId); }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is DNode_CharacterVm casted)
            {
                casted.OwnerId = OwnerId;
                
                casted.attachedFile = attachedFile;

                casted.LockedCharacterIds = LockedCharacterIds;

                foreach (var lockedCharacter in LockedCharacterIds)
                {
                    casted.LockedCharacterIds.Add(lockedCharacter);
                }

                foreach (var unlockedCharacter in UnlockedCharacterIds)
                {
                    casted.UnlockedCharacterIds.Add(unlockedCharacter);
                }

                for (int i = 0; i < FocusActors.Count; i++)
                {
                    casted.FocusActors.Add(FocusActors[i]);
                    casted.FocusTargets.Add(FocusTargets[i]);
                }

                foreach (var focusByName in FocusesByName)
                {
                    casted.FocusesByName.Add(focusByName);
                }

                foreach (var participantState in ParticipantStates)
                {
                    casted.ParticipantStates.Add(new ParticipantStateVm(casted, participantState.Character));
                }
            }
        }

        public override string GenerateCode(string nodeName, bool isInteractive)
        {
            var resultCode = string.Format("UDialogNode* {0} = nullptr;", nodeName) + Environment.NewLine;

            {
                resultCode += string.Format("if (nodeId == \"{0}\"", Id);
                if (Gender != UNISEX || Predicates.Count > 0) resultCode += " || " + Environment.NewLine;
                if (Gender != UNISEX)
                {
                    resultCode += string.Format("gender == {0}", GetGenderEnum()); ////// TODO
                    if (Predicates.Count > 0) resultCode += "&& ";
                }
                resultCode += string.Join(string.Format("&& {0}", Environment.NewLine), Predicates.Select(predicate => predicate.GenerateCode(nodeName)));
                resultCode += ") {" + Environment.NewLine;
            }

            resultCode += string.Format("{0} = NewObject<UDialogNode>(outer);", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->DialogNodeType = EDialogNodeType::REGULAR;", nodeName) + Environment.NewLine;
            resultCode += string.Format("{0}->DialogId = \"{1}\";", nodeName, Parent.Id) + Environment.NewLine;
            resultCode += string.Format("{0}->NodeId = \"{1}\";", nodeName, Id) + Environment.NewLine;
            resultCode += string.Format("{0}->OwnerClassPath = \"{1}\";", nodeName, Owner?.ClassPathName ?? "") + Environment.NewLine;
            resultCode += string.Format("{0}->Content = LOCTEXT(\"{1}\", \"{2}\");", nodeName, Id, GetSafeString(RTBHelper.GetFlowDocumentContent(Name))) + Environment.NewLine;
            resultCode += string.Format("{0}->Description = LOCTEXT(\"{1}\", \"{2}\");", nodeName, Id, GetSafeString(Description)) + Environment.NewLine;

            var allActors = Parent.Parent.Parent.AllActors.ToList();

            //////foreach (var characterId in LockedCharacterIds)
            //////{
            //////    resultCode += string.Format("{0}->LockedParticipants.Add(\"{1}\");", nodeName, 
            //////        allActors.FirstOrDefault(actor=>actor.Id == characterId)?.ClassPathName ?? "") + Environment.NewLine;
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

            if (isInteractive) resultCode += string.Format("{0}->IsInteractive = true;", nodeName) + Environment.NewLine;

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

        public override void SetupParenthood()
        {
            base.SetupParenthood();

            foreach (var participantState in ParticipantStates)
            {
                participantState.Parent = this;
                participantState.SetupParenthood();
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Owner != null && Owner.PassFilter(filter);
    }
}