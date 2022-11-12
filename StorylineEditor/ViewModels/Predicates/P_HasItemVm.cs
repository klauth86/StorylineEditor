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
using StorylineEditor.Model.Predicates;
using StorylineEditor.ViewModels.Nodes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Predicates
{
    [Description("Предмет: имеет")]
    public class P_HasItemVm : P_BaseVm
    {
        public P_HasItemVm(Node_BaseVm inParent, long additionalTicks) : base(inParent, additionalTicks) {
            ItemId = null;
            searchByName = false;
        }

        public P_HasItemVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var newP = new P_Item_HasM(ticks);
            model = newP;

            newP.name = Name;
            newP.description = Description;
            newP.isInversed = IsInversed;
            newP.itemId = Item?.GetModel(ticks, idReplacer)?.id;

            var times = id.Replace(GetType().Name + "_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            return model;
        }

        public override bool IsValid => base.IsValid && Item != null;

        public override bool IsConditionMet => !IsValid ||
            !isInversed && Parent.Parent.Parent.Parent.TreePlayerHistory.Inventory.Contains(Item) ||
            isInversed && !Parent.Parent.Parent.Parent.TreePlayerHistory.Inventory.Contains(Item);
        
        public string ItemId { get; set; }
        
        [XmlIgnore]
        public FolderedVm Item
        {
            get
            {
                return Parent.Parent.Parent.Parent.ItemsTab.Items
                  .FirstOrDefault(item => item?.Id == ItemId);
            }
            set
            {
                if (ItemId != value?.Id)
                {
                    ItemId = value?.Id;
                    NotifyWithCallerPropName();
                    NotifyIsValidChanged();
                }
            }
        }

        public bool searchByName;
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

            Item = null;
            SearchByName = false;
        }

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is P_HasItemVm casted)
            {
                casted.ItemId = ItemId;
                casted.searchByName = searchByName;
            }
        }
    }
}