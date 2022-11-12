/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModels.Nodes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Квест: добавить альтернативу")]
    [XmlRoot]
    public class GE_AddJournalRecordNodeVm : GE_BaseVm
    {
        public GE_AddJournalRecordNodeVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks)
        {
            JournalRecordId = null;
            JournalRecordNodeId = null;
        }

        public GE_AddJournalRecordNodeVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var newGE = new GE_Quest_Node_AddM(ticks);            
            model = newGE;

            var times = id.Replace(GetType().Name + "_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            idReplacer.Add(id, model.id);

            newGE.name = Name;
            newGE.description = Description;
            newGE.executionMode = ExecuteWhenLeaveDialogNode ? EXECUTION_MODE.ON_LEAVE : EXECUTION_MODE.ON_ENTER;
            newGE.questId = JournalRecord?.GetModel(ticks, idReplacer)?.id;
            newGE.nodeId = JournalRecordNode?.GetModel(ticks, idReplacer)?.id;

            return model;
        }

        public override bool IsValid => base.IsValid && JournalRecord != null && JournalRecordNode != null;

        public override void Execute()
        {
            if (IsValid)
            {
                JournalEntryVm journalEntry = Parent.Parent.Parent.Parent.TreePlayerHistory.AddJournalTree(JournalRecord as TreeVm);
                journalEntry.AddKnownNode(JournalRecordNode);
            }
        }

        public string JournalRecordId { get; set; }

        [XmlIgnore]
        public FolderedVm JournalRecord
        {
            get => Parent?.Parent?.Parent?.Parent?.JournalRecords.FirstOrDefault(item => item?.Id == JournalRecordId);
            set
            {
                if (JournalRecordId != value?.Id)
                {
                    JournalRecordId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public string JournalRecordNodeId { get; set; }

        [XmlIgnore]
        public Node_BaseVm JournalRecordNode
        {
            get => (JournalRecord as TreeVm)?.Nodes.FirstOrDefault(item => item?.Id == JournalRecordNodeId);
            set
            {
                if (JournalRecordNodeId != value?.Id)
                {
                    JournalRecordNodeId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            JournalRecord = null;
            JournalRecordNode = null;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is GE_AddJournalRecordNodeVm casted)
            {
                casted.JournalRecordId = JournalRecordId;
                casted.JournalRecordNodeId = JournalRecordNodeId;
            }
        }
    }
}