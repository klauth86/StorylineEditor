/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Interface;
using System;
using System.Linq;

namespace StorylineEditor.ViewModel.Predicates
{
    public static class PredicatesHelper
    {
        public static IPredicate CreatePredicateByType(Type type, object node)
        {
            if (type == typeof(P_CompositeM)) return new P_CompositeVM(new P_CompositeM(0), node);
            else if (type == typeof(P_Dialog_HasM)) return new P_Dialog_HasVM(new P_Dialog_HasM(0), node);
            else if (type == typeof(P_Dialog_Node_Has_ActiveSession_CmpM)) return new P_Dialog_Node_Has_ActiveSession_CmpVM(new P_Dialog_Node_Has_ActiveSession_CmpM(0), node);
            else if (type == typeof(P_Dialog_Node_Has_ActiveSessionM)) return new P_Dialog_Node_Has_ActiveSessionVM(new P_Dialog_Node_Has_ActiveSessionM(0), node);
            else if (type == typeof(P_Dialog_Node_Has_PrevSessions_CmpM)) return new P_Dialog_Node_Has_PrevSessions_CmpVM(new P_Dialog_Node_Has_PrevSessions_CmpM(0), node);
            else if (type == typeof(P_Dialog_Node_Has_PrevSessionsM)) return new P_Dialog_Node_Has_PrevSessionsVM(new P_Dialog_Node_Has_PrevSessionsM(0), node);
            else if (type == typeof(P_Item_HasM)) return new P_Item_HasVM(new P_Item_HasM(0), node);
            else if (type == typeof(P_Quest_AddedM)) return new P_Quest_AddedVM(new P_Quest_AddedM(0), node);
            else if (type == typeof(P_Quest_FinishedM)) return new P_Quest_FinishedVM(new P_Quest_FinishedM(0), node);
            else if (type == typeof(P_Quest_Node_AddedM)) return new P_Quest_Node_AddedVM(new P_Quest_Node_AddedM(0), node);
            else if (type == typeof(P_Quest_Node_PassedM)) return new P_Quest_Node_PassedVM(new P_Quest_Node_PassedM(0), node);
            else if (type == typeof(P_Relation_HasM)) return new P_Relation_HasVM(new P_Relation_HasM(0), node);

            throw new ArgumentOutOfRangeException(nameof(type));
        }

        public static IPredicate CreatePredicateByModel(P_BaseM model, object node)
        {
            if (model.GetType() == typeof(P_CompositeM)) return new P_CompositeVM((P_CompositeM)model, node);
            else if (model.GetType() == typeof(P_Dialog_HasM)) return new P_Dialog_HasVM((P_Dialog_HasM)model, node);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_ActiveSession_CmpM)) return new P_Dialog_Node_Has_ActiveSession_CmpVM((P_Dialog_Node_Has_ActiveSession_CmpM)model, node);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_ActiveSessionM)) return new P_Dialog_Node_Has_ActiveSessionVM((P_Dialog_Node_Has_ActiveSessionM)model, node);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_PrevSessions_CmpM)) return new P_Dialog_Node_Has_PrevSessions_CmpVM((P_Dialog_Node_Has_PrevSessions_CmpM)model, node);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_PrevSessionsM)) return new P_Dialog_Node_Has_PrevSessionsVM((P_Dialog_Node_Has_PrevSessionsM)model, node);
            else if (model.GetType() == typeof(P_Item_HasM)) return new P_Item_HasVM((P_Item_HasM)model, node);
            else if (model.GetType() == typeof(P_Quest_AddedM)) return new P_Quest_AddedVM((P_Quest_AddedM)model, node);
            else if (model.GetType() == typeof(P_Quest_FinishedM)) return new P_Quest_FinishedVM((P_Quest_FinishedM)model, node);
            else if (model.GetType() == typeof(P_Quest_Node_AddedM)) return new P_Quest_Node_AddedVM((P_Quest_Node_AddedM)model, node);
            else if (model.GetType() == typeof(P_Quest_Node_PassedM)) return new P_Quest_Node_PassedVM((P_Quest_Node_PassedM)model, node);
            else if (model.GetType() == typeof(P_Relation_HasM)) return new P_Relation_HasVM((P_Relation_HasM)model, node);

            throw new ArgumentOutOfRangeException(nameof(model));
        }

        public static bool IsTrue(P_BaseM pModel)
        {
            bool result = false;

            if (pModel is P_CompositeM compositePModel)
            {
                if (compositePModel.predicateA != null && compositePModel.predicateB != null)
                {
                    switch (compositePModel.compositionType)
                    {
                        case COMPOSITION_TYPE.AND:
                            result = IsTrue(compositePModel.predicateA) && IsTrue(compositePModel.predicateB);
                            break;
                        case COMPOSITION_TYPE.OR:
                            result = IsTrue(compositePModel.predicateA) || IsTrue(compositePModel.predicateB);
                            break;
                        case COMPOSITION_TYPE.XOR:
                            result = IsTrue(compositePModel.predicateA) ^ IsTrue(compositePModel.predicateB);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(compositePModel.compositionType));
                    }

                    if (pModel.isInversed) result = !result;
                }
                else if (compositePModel.predicateA != null)
                {
                    result = IsTrue(compositePModel.predicateA);
                }
                else if (compositePModel.predicateB != null)
                {
                    result = IsTrue(compositePModel.predicateB);
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Dialog_HasM hasDialogPModel)
            {
                if (hasDialogPModel.dialogId != null)
                {
                    result = ActiveContext.History.DialogEntries.Any((dialogEntry) => dialogEntry.Model.id == hasDialogPModel.dialogId && dialogEntry.Model.id != ActiveContext.History.ActiveDialogEntryId);
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Dialog_Node_Has_ActiveSession_CmpM hasNodeASCMPPModel)
            {
                if (hasNodeASCMPPModel.dialogId != null && hasNodeASCMPPModel.nodeId != null)
                {
                    var dialogEntries = ActiveContext.History.DialogEntries.Where((dialogEntry) => dialogEntry.Model.id == hasNodeASCMPPModel.dialogId && dialogEntry.Model.id == ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntry in dialogEntries)
                    {
                        count += dialogEntry.Nodes.Count((node) => node.id == hasNodeASCMPPModel.nodeId);
                    }

                    switch (hasNodeASCMPPModel.compareType)
                    {
                        case COMPARE_TYPE.LESS:
                            result = count < hasNodeASCMPPModel.value;
                            break;
                        case COMPARE_TYPE.LESS_OR_EQUAL:
                            result = count <= hasNodeASCMPPModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL:
                            result = count == hasNodeASCMPPModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL_OR_GREATER:
                            result = count >= hasNodeASCMPPModel.value;
                            break;
                        case COMPARE_TYPE.GREATER:
                            result = count > hasNodeASCMPPModel.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hasNodeASCMPPModel.compareType));
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Dialog_Node_Has_ActiveSessionM hasNodeASPModel)
            {
                if (hasNodeASPModel.dialogId != null && hasNodeASPModel.nodeId != null)
                {
                    var dialogEntries = ActiveContext.History.DialogEntries.Where((dialogEntry) => dialogEntry.Model.id == hasNodeASPModel.dialogId && dialogEntry.Model.id == ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntry in dialogEntries)
                    {
                        count += dialogEntry.Nodes.Count((node) => node.id == hasNodeASPModel.nodeId);
                    }

                    result = count > 0;
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Dialog_Node_Has_PrevSessions_CmpM hasNodePSCMPPModel)
            {
                if (hasNodePSCMPPModel.dialogId != null && hasNodePSCMPPModel.nodeId != null)
                {
                    var dialogEntries = ActiveContext.History.DialogEntries.Where((dialogEntry) => dialogEntry.Model.id == hasNodePSCMPPModel.dialogId && dialogEntry.Model.id != ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntry in dialogEntries)
                    {
                        count += dialogEntry.Nodes.Count((node) => node.id == hasNodePSCMPPModel.nodeId);
                    }

                    switch (hasNodePSCMPPModel.compareType)
                    {
                        case COMPARE_TYPE.LESS:
                            result = count < hasNodePSCMPPModel.value;
                            break;
                        case COMPARE_TYPE.LESS_OR_EQUAL:
                            result = count <= hasNodePSCMPPModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL:
                            result = count == hasNodePSCMPPModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL_OR_GREATER:
                            result = count >= hasNodePSCMPPModel.value;
                            break;
                        case COMPARE_TYPE.GREATER:
                            result = count > hasNodePSCMPPModel.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hasNodePSCMPPModel.compareType));
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Dialog_Node_Has_PrevSessionsM hasNodePSPModel)
            {
                if (hasNodePSPModel.dialogId != null && hasNodePSPModel.nodeId != null)
                {
                    var dialogEntries = ActiveContext.History.DialogEntries.Where((dialogEntry) => dialogEntry.Model.id == hasNodePSPModel.dialogId && dialogEntry.Model.id != ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntry in dialogEntries)
                    {
                        count += dialogEntry.Nodes.Count((node) => node.id == hasNodePSPModel.nodeId);
                    }

                    result = count > 0;
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Item_HasM hasItemPModel)
            {
                if (hasItemPModel.itemId != null)
                {
                    result = ActiveContext.History.Inventory.Any((itemModel) => itemModel.id == hasItemPModel.itemId);
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Quest_AddedM addedQuestPModel)
            {
                if (addedQuestPModel.questId != null)
                {
                    result = ActiveContext.History.QuestEntries.Any((qeVm) => qeVm.Model.id == addedQuestPModel.questId);
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Quest_FinishedM finishedQuestPModel)
            {
                if (finishedQuestPModel.questId != null) ////// TODO SIMPLIFY
                {
                    QuestEntryVM questEntry = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == finishedQuestPModel.questId);

                    if (questEntry != null)
                    {
                        foreach (var knownNodeEntry in questEntry.KnownNodes)
                        {
                            if (knownNodeEntry.IsPassed)
                            {
                                if (questEntry.Model.links.All((linkM) => linkM.fromNodeId != knownNodeEntry.Node.id))
                                {
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Quest_Node_AddedM addedNodeForQuestPModel)
            {
                if (addedNodeForQuestPModel.questId != null && addedNodeForQuestPModel.nodeId != null)
                {
                    QuestEntryVM questEntry = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == addedNodeForQuestPModel.questId);

                    if (questEntry != null)
                    {
                        result = questEntry.HasKnownNode(addedNodeForQuestPModel.nodeId);
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Quest_Node_PassedM passedNodeForQuestPModel)
            {
                if (passedNodeForQuestPModel.questId != null && passedNodeForQuestPModel.nodeId != null)
                {
                    QuestEntryVM questEntry = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == passedNodeForQuestPModel.questId);

                    if (questEntry != null)
                    {
                        result = questEntry.GetKnownNodeIsPassed(passedNodeForQuestPModel.nodeId);
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (pModel is P_Relation_HasM hasRelationPModel)
            {
                if (hasRelationPModel.npcId != null)
                {
                    CharacterM character = (CharacterM)ActiveContext.GetCharacter(hasRelationPModel.npcId);

                    float relation = ActiveContext.History.Gender == GENDER.MALE
                        ? character.initialRelation
                        : character.initialRelationFemale;

                    CharacterEntryVM characterEntry = ActiveContext.History.CharacterEntries.FirstOrDefault((ceVm) => ceVm.Model.id == hasRelationPModel.npcId);

                    if (characterEntry != null)
                    {
                        relation += characterEntry.DeltaRelation;
                    }

                    switch (hasRelationPModel.compareType)
                    {
                        case COMPARE_TYPE.LESS:
                            result = relation < hasRelationPModel.value;
                            break;
                        case COMPARE_TYPE.LESS_OR_EQUAL:
                            result = relation <= hasRelationPModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL:
                            result = relation == hasRelationPModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL_OR_GREATER:
                            result = relation >= hasRelationPModel.value;
                            break;
                        case COMPARE_TYPE.GREATER:
                            result = relation > hasRelationPModel.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hasRelationPModel.compareType));
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (pModel.isInversed) result = !result;

            return result;
        }
    }
}