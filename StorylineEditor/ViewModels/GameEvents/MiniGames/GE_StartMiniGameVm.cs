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
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.GameEvents
{
    [Description("Начать Мини Игру")]
    [XmlRoot]
    public class GE_StartMiniGameVm : GE_BaseVm
    {
        public GE_StartMiniGameVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            MiniGameId = null;
            searchByName = false;
        }

        public GE_StartMiniGameVm() : this(null, 0) { }

        public override bool IsValid => base.IsValid && MiniGame != null;

        public string MiniGameId { get; set; }

        [XmlIgnore]
        public FolderedVm MiniGame
        {
            get
            {
                return Parent.Parent.Parent.Parent.LocationObjectsTab.Items
                  .FirstOrDefault(item => item?.Id == MiniGameId);
            }
            set
            {
                if (MiniGameId != value?.Id)
                {
                    MiniGameId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        protected bool searchByName;
        public bool SearchByName
        {
            get => searchByName;
            set
            {
                if (searchByName != value)
                {
                    searchByName = value;
                    NotifyWithCallerPropName();
                }
            }
        }

        protected override void ResetInternalData()
        {
            base.ResetInternalData();
            MiniGame = null;
            SearchByName = false;
        }

        protected override void CloneInternalData(BaseVm destObj)
        {
            base.CloneInternalData(destObj);

            if (destObj is GE_StartMiniGameVm casted)
            {
                casted.MiniGameId = MiniGameId;
                casted.searchByName = searchByName;
            }
        }
    }
}