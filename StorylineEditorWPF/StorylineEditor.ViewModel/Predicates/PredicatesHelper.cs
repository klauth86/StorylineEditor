/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
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

        public static bool IsTrue(P_BaseM predicateModel)
        {
            bool result = false;

            if (predicateModel is P_CompositeM compositePredicateModel)
            {
                if (compositePredicateModel.predicateA != null && compositePredicateModel.predicateB != null)
                {
                    switch (compositePredicateModel.compositionType)
                    {
                        case COMPOSITION_TYPE.AND:
                            result = IsTrue(compositePredicateModel.predicateA) && IsTrue(compositePredicateModel.predicateB);
                            break;
                        case COMPOSITION_TYPE.OR:
                            result = IsTrue(compositePredicateModel.predicateA) || IsTrue(compositePredicateModel.predicateB);
                            break;
                        case COMPOSITION_TYPE.XOR:
                            result = IsTrue(compositePredicateModel.predicateA) ^ IsTrue(compositePredicateModel.predicateB);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(compositePredicateModel.compositionType));
                    }

                    if (predicateModel.isInversed) result = !result;
                }
                else if (compositePredicateModel.predicateA != null)
                {
                    result = IsTrue(compositePredicateModel.predicateA);
                }
                else if (compositePredicateModel.predicateB != null)
                {
                    result |= IsTrue(compositePredicateModel.predicateB);
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Dialog_HasM hasDialogPredicateModel)
            {
                if (hasDialogPredicateModel.dialogId != null)
                {
                    result = ActiveContext.History.DialogEntries.Any((deVm) => deVm.Model.id == hasDialogPredicateModel.dialogId && deVm.Model.id != ActiveContext.History.ActiveDialogEntryId);
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Dialog_Node_Has_ActiveSession_CmpM hasNodeASCMPPredicateModel)
            {
                if (hasNodeASCMPPredicateModel.dialogId != null && hasNodeASCMPPredicateModel.nodeId != null)
                {
                    var dialogEntryVms = ActiveContext.History.DialogEntries.Where((deVm) => deVm.Model.id == hasNodeASCMPPredicateModel.dialogId && deVm.Model.id != ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntryVm in dialogEntryVms)
                    {
                        count += dialogEntryVm.Nodes.Count((node) => node.id == hasNodeASCMPPredicateModel.nodeId);
                    }

                    switch (hasNodeASCMPPredicateModel.compareType)
                    {
                        case COMPARE_TYPE.LESS:
                            result = count < hasNodeASCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.LESS_OR_EQUAL:
                            result = count <= hasNodeASCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL:
                            result = count == hasNodeASCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL_OR_GREATER:
                            result = count >= hasNodeASCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.GREATER:
                            result = count > hasNodeASCMPPredicateModel.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hasNodeASCMPPredicateModel.compareType));
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Dialog_Node_Has_ActiveSessionM hasNodeASPredicateModel)
            {
                if (hasNodeASPredicateModel.dialogId != null && hasNodeASPredicateModel.nodeId != null)
                {
                    var dialogEntryVms = ActiveContext.History.DialogEntries.Where((deVm) => deVm.Model.id == hasNodeASPredicateModel.dialogId && deVm.Model.id != ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntryVm in dialogEntryVms)
                    {
                        count += dialogEntryVm.Nodes.Count((node) => node.id == hasNodeASPredicateModel.nodeId);
                    }

                    result = count > 0;
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Dialog_Node_Has_PrevSessions_CmpM hasNodePSCMPPredicateModel)
            {
                if (hasNodePSCMPPredicateModel.dialogId != null && hasNodePSCMPPredicateModel.nodeId != null)
                {
                    var dialogEntryVms = ActiveContext.History.DialogEntries.Where((deVm) => deVm.Model.id == hasNodePSCMPPredicateModel.dialogId && deVm.Model.id != ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntryVm in dialogEntryVms)
                    {
                        count += dialogEntryVm.Nodes.Count((node) => node.id == hasNodePSCMPPredicateModel.nodeId);
                    }

                    switch (hasNodePSCMPPredicateModel.compareType)
                    {
                        case COMPARE_TYPE.LESS:
                            result = count < hasNodePSCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.LESS_OR_EQUAL:
                            result = count <= hasNodePSCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL:
                            result = count == hasNodePSCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL_OR_GREATER:
                            result = count >= hasNodePSCMPPredicateModel.value;
                            break;
                        case COMPARE_TYPE.GREATER:
                            result = count > hasNodePSCMPPredicateModel.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hasNodePSCMPPredicateModel.compareType));
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Dialog_Node_Has_PrevSessionsM hasNodePSPredicateModel)
            {
                if (hasNodePSPredicateModel.dialogId != null && hasNodePSPredicateModel.nodeId != null)
                {
                    var dialogEntryVms = ActiveContext.History.DialogEntries.Where((deVm) => deVm.Model.id == hasNodePSPredicateModel.dialogId && deVm.Model.id != ActiveContext.History.ActiveDialogEntryId);

                    int count = 0;

                    foreach (var dialogEntryVm in dialogEntryVms)
                    {
                        count += dialogEntryVm.Nodes.Count((node) => node.id == hasNodePSPredicateModel.nodeId);
                    }

                    result = count > 0;
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Item_HasM hasItemPredicateModel)
            {
                if (hasItemPredicateModel.itemId != null)
                {
                    result = ActiveContext.History.Inventory.Any((itemModel) => itemModel.id == hasItemPredicateModel.itemId);
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Quest_AddedM addedQuestPredicateModel)
            {
                if (addedQuestPredicateModel.questId != null)
                {
                    result = ActiveContext.History.QuestEntries.Any((qeVm) => qeVm.Model.id == addedQuestPredicateModel.questId);
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Quest_FinishedM finishedQuestPredicateModel)
            {
                if (finishedQuestPredicateModel.questId != null) ////// TODO SIMPLIFY
                {
                    QuestEntryVM questEntryVm = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == finishedQuestPredicateModel.questId);

                    if (questEntryVm != null)
                    {
                        foreach (var knownNodeEntry in questEntryVm.KnownNodes)
                        {
                            if (knownNodeEntry.IsPassed)
                            {
                                ////// TODO
                                //////if (graph.links.All((linkM) => linkM.fromNodeId != knownNodeEntry.Node.id))
                                //////{
                                //////    result = true;
                                //////    break;
                                //////}
                            }
                        }
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Quest_Node_AddedM addedNodePredicateModel)
            {
                if (addedNodePredicateModel.questId != null && addedNodePredicateModel.nodeId != null)
                {
                    QuestEntryVM questEntryVm = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == addedNodePredicateModel.questId);

                    if (questEntryVm != null)
                    {
                        ////// TODO
                        //////result = questEntryVm.HasKnownNode(Node);
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Quest_Node_PassedM passedNodePredicateModel)
            {
                if (passedNodePredicateModel.questId != null && passedNodePredicateModel.nodeId != null)
                {
                    QuestEntryVM questEntryVm = ActiveContext.History.QuestEntries.FirstOrDefault((qeVm) => qeVm.Model.id == passedNodePredicateModel.questId);

                    if (questEntryVm != null)
                    {
                        ////// TODO
                        //////result = questEntryVm.GetKnownNodeIsPassed(Node);
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel is P_Relation_HasM hasRelationPredicateModel)
            {
                if (hasRelationPredicateModel.npcId != null)
                {
                    ////// TODO
                    //////CharacterM character = (CharacterM)Character;

                    float relation = 0;
                        ////ActiveContext.History.Gender == GENDER.MALE
                        ////? character.initialRelation
                        ////: character.initialRelationFemale;

                    CharacterEntryVM characterEntryVm = ActiveContext.History.CharacterEntries.FirstOrDefault((ceVm) => ceVm.Model.id == hasRelationPredicateModel.npcId);

                    if (characterEntryVm != null)
                    {
                        relation += characterEntryVm.DeltaRelation;
                    }

                    switch (hasRelationPredicateModel.compareType)
                    {
                        case COMPARE_TYPE.LESS:
                            result = relation < hasRelationPredicateModel.value;
                            break;
                        case COMPARE_TYPE.LESS_OR_EQUAL:
                            result = relation <= hasRelationPredicateModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL:
                            result = relation == hasRelationPredicateModel.value;
                            break;
                        case COMPARE_TYPE.EQUAL_OR_GREATER:
                            result = relation >= hasRelationPredicateModel.value;
                            break;
                        case COMPARE_TYPE.GREATER:
                            result = relation > hasRelationPredicateModel.value;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(hasRelationPredicateModel.compareType));
                    }
                }
                else
                {
                    result = true;
                }
            }

            if (predicateModel.isInversed) result = !result;

            return result;
        }
    }
}