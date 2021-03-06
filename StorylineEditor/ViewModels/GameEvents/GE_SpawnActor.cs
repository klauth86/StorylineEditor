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
    [Description("Создать актор")]
    [XmlRoot]
    public class GE_SpawnActor : GE_BaseVm
    {
        public GE_SpawnActor(Node_BaseVm inParent) : base(inParent)
        {
            ActorToSpawnId = null;
            PointToSpawnId = null;
            num = 1;
            TargetId = null;
            searchTargetByName = false;
        }

        public GE_SpawnActor() : this(null) { }

        public override bool IsValid => base.IsValid && ActorToSpawn != null && PointToSpawn != null && num >= 1;

        public string ActorToSpawnId { get; set; }

        [XmlIgnore]
        public BaseVm ActorToSpawn
        {
            get
            {
                return Parent.Parent.Parent.Parent.AllActors
                  .FirstOrDefault(item => item?.Id == ActorToSpawnId);
            }
            set
            {
                if (ActorToSpawnId != value?.Id)
                {
                    ActorToSpawnId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }


        public string PointToSpawnId { get; set; }

        [XmlIgnore]
        public BaseVm PointToSpawn
        {
            get
            {
                return Parent.Parent.Parent.Parent.AllActors
                  .FirstOrDefault(item => item?.Id == PointToSpawnId);
            }
            set
            {
                if (PointToSpawnId != value?.Id)
                {
                    PointToSpawnId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected int num;
        public int Num {
            get => num;
            set {
                if (num != value) {
                    num = value;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public string TargetId { get; set; }

        [XmlIgnore]
        public BaseVm Target
        {
            get
            {
                return Parent.Parent.Parent.Parent.AllActors
                  .FirstOrDefault(item => item?.Id == TargetId);
            }
            set
            {
                if (TargetId != value?.Id)
                {
                    TargetId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected bool searchTargetByName;
        public bool SearchTargetByName
        {
            get => searchTargetByName;
            set
            {
                if (searchTargetByName != value)
                {
                    searchTargetByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            ActorToSpawn = null;
            PointToSpawn = null;
            Num = 1;
            Target = null;
            SearchTargetByName = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_SpawnActor casted)
            {
                casted.ActorToSpawnId = ActorToSpawnId;
                casted.PointToSpawnId = PointToSpawnId;
                casted.num = num;
                casted.TargetId = TargetId;
                casted.searchTargetByName = searchTargetByName;
            }
        }

        public override string GenerateCode(string eventName, string outerName)
        {
            var resultCode = string.Format("auto {1} = NewObject<UGE_SpawnActor>({0});", outerName, eventName) + Environment.NewLine;
            resultCode += string.Format("{0}->ActorToSpawnClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, ActorToSpawn?.ClassPathName ?? "") + Environment.NewLine;
            resultCode += string.Format("{0}->SpawnPointName = \"{1}\";", eventName, PointToSpawn?.ActorName ?? "") + Environment.NewLine;
            resultCode += string.Format("{0}->Num = {1};", eventName, num) + Environment.NewLine;

            if (searchTargetByName) resultCode += string.Format("{0}->TargetActorName = \"{1}\";", eventName, Target?.ActorName ?? "") + Environment.NewLine;
            if (!searchTargetByName) resultCode += string.Format("{0}->TargetActorClassPtr = FSoftObjectPath(TEXT(\"{1}\"), \"\");", eventName, Target?.ClassPathName ?? null) + Environment.NewLine;
            if (searchTargetByName) resultCode += string.Format("{0}->SearchTargetActorByName = true;", eventName) + Environment.NewLine;
            
            return resultCode;
        }
    }
}
