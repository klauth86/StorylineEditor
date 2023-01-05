﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModel.Common;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.Predicates
{
    public static class PredicatesHelper
    {
        public static IWithModel CreatePredicateByType(Type type, ICallbackContext callbackContext)
        {
            if (type == typeof(P_CompositeM)) return new P_CompositeVM(new P_CompositeM(0), callbackContext);
            else if (type == typeof(P_Dialog_HasM)) return new P_Dialog_HasVM(new P_Dialog_HasM(0), null);
            else if (type == typeof(P_Dialog_Node_Has_ActiveSession_CmpM)) return new P_Dialog_Node_Has_ActiveSession_CmpVM(new P_Dialog_Node_Has_ActiveSession_CmpM(0), callbackContext);
            else if (type == typeof(P_Dialog_Node_Has_ActiveSessionM)) return new P_Dialog_Node_Has_ActiveSessionVM(new P_Dialog_Node_Has_ActiveSessionM(0), callbackContext);
            else if (type == typeof(P_Dialog_Node_Has_PrevSessions_CmpM)) return new P_Dialog_Node_Has_PrevSessions_CmpVM(new P_Dialog_Node_Has_PrevSessions_CmpM(0), null);
            else if (type == typeof(P_Dialog_Node_Has_PrevSessionsM)) return new P_Dialog_Node_Has_PrevSessionsVM(new P_Dialog_Node_Has_PrevSessionsM(0), null);
            else if (type == typeof(P_Item_HasM)) return new P_Item_HasVM(new P_Item_HasM(0), null);
            else if (type == typeof(P_Quest_AddedM)) return new P_Quest_AddedVM(new P_Quest_AddedM(0), null);
            else if (type == typeof(P_Quest_FinishedM)) return new P_Quest_FinishedVM(new P_Quest_FinishedM(0), null);
            else if (type == typeof(P_Quest_Node_AddedM)) return new P_Quest_Node_AddedVM(new P_Quest_Node_AddedM(0), null);
            else if (type == typeof(P_Quest_Node_PassedM)) return new P_Quest_Node_PassedVM(new P_Quest_Node_PassedM(0), null);
            else if (type == typeof(P_Relation_HasM)) return new P_Relation_HasVM(new P_Relation_HasM(0), null);

            return null;
        }

        public static IWithModel CreatePredicateByModel(P_BaseM model, ICallbackContext callbackContext)
        {
            if (model.GetType() == typeof(P_CompositeM)) return new P_CompositeVM((P_CompositeM)model, callbackContext);
            else if (model.GetType() == typeof(P_Dialog_HasM)) return new P_Dialog_HasVM((P_Dialog_HasM)model, null);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_ActiveSession_CmpM)) return new P_Dialog_Node_Has_ActiveSession_CmpVM((P_Dialog_Node_Has_ActiveSession_CmpM)model, callbackContext);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_ActiveSessionM)) return new P_Dialog_Node_Has_ActiveSessionVM((P_Dialog_Node_Has_ActiveSessionM)model, callbackContext);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_PrevSessions_CmpM)) return new P_Dialog_Node_Has_PrevSessions_CmpVM((P_Dialog_Node_Has_PrevSessions_CmpM)model, null);
            else if (model.GetType() == typeof(P_Dialog_Node_Has_PrevSessionsM)) return new P_Dialog_Node_Has_PrevSessionsVM((P_Dialog_Node_Has_PrevSessionsM)model, null);
            else if (model.GetType() == typeof(P_Item_HasM)) return new P_Item_HasVM((P_Item_HasM)model, null);
            else if (model.GetType() == typeof(P_Quest_AddedM)) return new P_Quest_AddedVM((P_Quest_AddedM)model, null);
            else if (model.GetType() == typeof(P_Quest_FinishedM)) return new P_Quest_FinishedVM((P_Quest_FinishedM)model, null);
            else if (model.GetType() == typeof(P_Quest_Node_AddedM)) return new P_Quest_Node_AddedVM((P_Quest_Node_AddedM)model, null);
            else if (model.GetType() == typeof(P_Quest_Node_PassedM)) return new P_Quest_Node_PassedVM((P_Quest_Node_PassedM)model, null);
            else if (model.GetType() == typeof(P_Relation_HasM)) return new P_Relation_HasVM((P_Relation_HasM)model, null);

            return null;
        }
    }
}