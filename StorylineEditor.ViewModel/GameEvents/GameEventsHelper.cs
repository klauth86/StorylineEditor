/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.GameEvents;
using StorylineEditor.ViewModel.Interface;
using System;

namespace StorylineEditor.ViewModel.GameEvents
{
    public static class GameEventsHelper
    {
        public static IGameEvent CreateGameEventByType(Type type, INode node)
        {
            if (type == typeof(GE_Item_DropM)) return new GE_Item_DropVM(new GE_Item_DropM(0), node);
            if (type == typeof(GE_Item_PickUpM)) return new GE_Item_PickUpVM(new GE_Item_PickUpM(0), node);
            if (type == typeof(GE_Quest_AddM)) return new GE_Quest_AddVM(new GE_Quest_AddM(0), node);
            if (type == typeof(GE_Quest_Node_AddM)) return new GE_Quest_Node_AddVM(new GE_Quest_Node_AddM(0), node);
            if (type == typeof(GE_Quest_Node_PassM)) return new GE_Quest_Node_PassVM(new GE_Quest_Node_PassM(0), node);
            if (type == typeof(GE_Relation_ChangeM)) return new GE_Relation_ChangeVM(new GE_Relation_ChangeM(0), node);

            throw new ArgumentOutOfRangeException(nameof(type));
        }

        public static IGameEvent CreateGameEventByModel(GE_BaseM model, INode node)
        {
            if (model.GetType() == typeof(GE_Item_DropM)) return new GE_Item_DropVM((GE_Item_DropM)model, node);
            if (model.GetType() == typeof(GE_Item_PickUpM)) return new GE_Item_PickUpVM((GE_Item_PickUpM)model, node);
            if (model.GetType() == typeof(GE_Quest_AddM)) return new GE_Quest_AddVM((GE_Quest_AddM)model, node);
            if (model.GetType() == typeof(GE_Quest_Node_AddM)) return new GE_Quest_Node_AddVM((GE_Quest_Node_AddM)model, node);
            if (model.GetType() == typeof(GE_Quest_Node_PassM)) return new GE_Quest_Node_PassVM((GE_Quest_Node_PassM)model, node);
            if (model.GetType() == typeof(GE_Relation_ChangeM)) return new GE_Relation_ChangeVM((GE_Relation_ChangeM)model, node);

            throw new ArgumentOutOfRangeException(nameof(model));
        }
    }
}