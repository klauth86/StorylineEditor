﻿/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.Model;
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModels.Nodes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Диалог: имеет вершину в прошлых сессиях")]
    [XmlRoot]
    public class P_HasDialogNodeVm : P_BaseVm
    {
        public P_HasDialogNodeVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            DialogId = null;
            DialogNodeId = null;
        }

        public P_HasDialogNodeVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var newP = new P_Dialog_Node_Has_PrevSessionsM(ticks);
            model = newP;

            newP.name = Name;
            newP.description = Description;
            newP.isInversed = IsInversed;
            newP.dialogId = Dialog?.GetModel(ticks, idReplacer)?.id;
            newP.nodeId = DialogNode?.GetModel(ticks, idReplacer)?.id;

            var times = id.Replace(GetType().Name + "_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            return model;
        }

        public override bool IsValid => base.IsValid && Dialog != null && DialogNode != null;

        public override bool IsConditionMet => !IsValid ||
            !isInversed && Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.Any((treePath) => treePath?.Tree?.Id == DialogId && treePath.PassedNodes.Contains(DialogNode) && !treePath.IsActive) ||
            isInversed && !Parent.Parent.Parent.Parent.TreePlayerHistory.PassedDialogsAndReplicas.Any((treePath) => treePath?.Tree?.Id == DialogId && treePath.PassedNodes.Contains(DialogNode) && !treePath.IsActive);

        public string DialogId { get; set; }

        [XmlIgnore]
        public FolderedVm Dialog
        {
            get => Parent?.Parent?.Parent?.Parent?.DialogsAndReplicas.FirstOrDefault(item => item?.Id == DialogId);
            set
            {
                if (DialogId != value?.Id)
                {
                    DialogId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public string DialogNodeId { get; set; }
        
        [XmlIgnore]
        public Node_BaseVm DialogNode
        {
            get => (Dialog as TreeVm)?.Nodes.FirstOrDefault(item => item?.Id == DialogNodeId);
            set
            {
                if (DialogNodeId != value?.Id)
                {
                    DialogNodeId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            Dialog = null;
            DialogNode = null;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasDialogNodeVm casted)
            {
                casted.DialogId = DialogId;
                casted.DialogNodeId = DialogNodeId;
            }
        }
    }
}