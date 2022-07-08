/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.ViewModels;
using StorylineEditor.ViewModels.GameEvents;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Predicates;
using StorylineEditor.ViewModels.Tabs;
using System;

namespace StorylineEditor.Common
{
    public class CustomByteConverter
    {
        public static BaseVm CreateByName(string name, BaseVm Parent)
        {
            if (name == typeof(AbilityVm).Name) return new AbilityVm(Parent as AbilitiesTabVm);
            if (name == typeof(CharacterVm).Name) return new CharacterVm(Parent as CharactersTabVm);
            if (name == typeof(ItemVm).Name) return new ItemVm(Parent as ItemsTabVm);
            if (name == typeof(LocationObjectVm).Name) return new LocationObjectVm(Parent as LocationObjectsTabVm);

            if (name == typeof(JournalTagVm).Name) return new JournalTagVm(Parent as GlobalTagsTabVm);

            if (name == typeof(TreeVm).Name) return new TreeVm(Parent as BaseTreesTabVm);

            if (name == typeof(GE_DropItemVm).Name) return new GE_DropItemVm(Parent as Node_BaseVm);
            if (name == typeof(GE_PickUpItemVm).Name) return new GE_PickUpItemVm(Parent as Node_BaseVm);
            if (name == typeof(GE_SetIsActiveVm).Name) return new GE_SetIsActiveVm(Parent as Node_BaseVm);
            if (name == typeof(GE_DestroyActorVm).Name) return new GE_DestroyActorVm(Parent as Node_BaseVm);
            if (name == typeof(GE_SpawnActor).Name) return new GE_SpawnActor(Parent as Node_BaseVm);
            if (name == typeof(GE_Storyline).Name) return new GE_Storyline(Parent as Node_BaseVm);
            if (name == typeof(GE_MoveToVm).Name) return new GE_MoveToVm(Parent as Node_BaseVm);
            if (name == typeof(GE_SetTargetActorVm).Name) return new GE_SetTargetActorVm(Parent as Node_BaseVm);
            if (name == typeof(GE_StartMiniGameVm).Name) return new GE_StartMiniGameVm(Parent as Node_BaseVm);
            if (name == typeof(GE_StartDialogVm).Name) return new GE_StartDialogVm(Parent as Node_BaseVm);
            if (name == typeof(GE_StartReplicaVm).Name) return new GE_StartReplicaVm(Parent as Node_BaseVm);

            if (name == typeof(P_HasDialogNodeMoreLessVm).Name) return new P_HasDialogNodeMoreLessVm(Parent as Node_BaseVm);
            if (name == typeof(P_HasDialogNodeVm).Name) return new P_HasDialogNodeVm(Parent as Node_BaseVm);
            if (name == typeof(P_HasDialogVm).Name) return new P_HasDialogVm(Parent as Node_BaseVm);
            if (name == typeof(P_HasItemVm).Name) return new P_HasItemVm(Parent as Node_BaseVm);
            if (name == typeof(P_HasJournalTagsVm).Name) return new P_HasJournalTagsVm(Parent as Node_BaseVm);
            if (name == typeof(P_HasJournalRecordVm).Name) return new P_HasJournalRecordVm(Parent as Node_BaseVm);
            if (name == typeof(P_HasJournalRecordNodeVm).Name) return new P_HasJournalRecordNodeVm(Parent as Node_BaseVm);
            if (name == typeof(P_CompositeVm).Name) return new P_CompositeVm(Parent as Node_BaseVm);

            if (name == typeof(JNode_AlternativeVm).Name) return new JNode_AlternativeVm(Parent as TreeVm);
            if (name == typeof(JNode_StepVm).Name) return new JNode_StepVm(Parent as TreeVm);
            if (name == typeof(JNode_BaseVm).Name) return new JNode_BaseVm(Parent as TreeVm);
            if (name == typeof(DNode_CharacterVm).Name) return new DNode_CharacterVm(Parent as TreeVm);
            if (name == typeof(DNode_RandomVm).Name) return new DNode_RandomVm(Parent as TreeVm);
            if (name == typeof(DNode_TransitVm).Name) return new DNode_TransitVm(Parent as TreeVm);
            if (name == typeof(DNode_DialogVm).Name) return new DNode_DialogVm(Parent as TreeVm);
            if (name == typeof(DNode_VirtualVm).Name) return new DNode_VirtualVm(Parent as TreeVm);

            if (name == typeof(AbilitiesTabVm).Name) return new AbilitiesTabVm(Parent as FullContextVm);
            if (name == typeof(CharactersTabVm).Name) return new CharactersTabVm(Parent as FullContextVm);
            if (name == typeof(ItemsTabVm).Name) return new ItemsTabVm(Parent as FullContextVm);
            if (name == typeof(LocationObjectsTabVm).Name) return new LocationObjectsTabVm(Parent as FullContextVm);

            if (name == typeof(GlobalTagsTabVm).Name) return new GlobalTagsTabVm(Parent as FullContextVm);

            if (name == typeof(PlayerDialogsTabVm).Name) return new PlayerDialogsTabVm(Parent as FullContextVm);
            if (name == typeof(ReplicasTabVm).Name) return new ReplicasTabVm(Parent as FullContextVm);
            if (name == typeof(JournalRecordsTabVm).Name) return new JournalRecordsTabVm(Parent as FullContextVm);

            if (name == typeof(FullContextVm).Name) return new FullContextVm();

            throw new ArgumentOutOfRangeException();
        }
    }
}
