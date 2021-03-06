/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Common;
using StorylineEditor.ViewModels.Nodes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Начать Реплику")]
    [XmlRoot]
    public class GE_StartReplicaVm : GE_BaseVm
    {
        public GE_StartReplicaVm(Node_BaseVm inParent) : base(inParent) {
            ReplicaId = null;
        }

        public GE_StartReplicaVm() : this(null) { }

        public override bool IsValid => base.IsValid && Replica != null;

        public string ReplicaId { get; set; }

        [XmlIgnore]
        public FolderedVm Replica
        {
            get => Parent.Parent.Parent.Parent.Replicas.FirstOrDefault(item => item?.Id == ReplicaId);
            set
            {
                if (ReplicaId != value?.Id)
                {
                    ReplicaId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();

            Replica = null;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            var casted = destObj as GE_StartReplicaVm;
            if (casted != null)
            {
                casted.ReplicaId = ReplicaId;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_StartReplica>({0});", outerName, eventName) + Environment.NewLine;
            resultCode += string.Format("{0}->Replica.Index = {1};", eventName, Parent.Parent.Parent.Parent.ReplicasTab.Items.IndexOf(Replica)) + Environment.NewLine;
            return resultCode;
        }
    }
}