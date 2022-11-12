/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model;
using StorylineEditor.Model.GameEvents;
using StorylineEditor.Model.Nodes;
using StorylineEditor.Model.Predicates;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace StorylineEditor.ViewModels.Nodes
{
    [Description("Случайная вершина")]
    [XmlRoot]
    public class DNode_RandomVm : Node_InteractiveVm
    {
        public DNode_RandomVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public DNode_RandomVm() : this(null, 0) { }

        public override bool IsValid => !string.IsNullOrEmpty(id) &&
            GameEvents.All(gameEvent => gameEvent?.IsValid ?? false) &&
            Predicates.All(predicate => predicate?.IsValid ?? false);

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var newNode = new Node_RandomM(ticks)
            {
                gender = (byte)Gender,
                positionX = PositionX,
                positionY = PositionY,
            };

            model = newNode;

            var times = id.Replace("DNode_RandomVm_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            idReplacer.Add(id, model.id);

            int k = 0;
            foreach (var gameEvent in GameEvents)
            {
                if (gameEvent != null)
                {
                    var gameEventModel = gameEvent.GetModel(k, idReplacer);
                    newNode.gameEvents.Add((GE_BaseM)gameEventModel);
                }
                k++;
            }

            k = 0;
            foreach (var predicate in Predicates)
            {
                if (predicate != null)
                {
                    var predicateModel = predicate.GetModel(k, idReplacer);
                    newNode.predicates.Add((P_BaseM)predicateModel);
                }
                k++;
            }

            return model;
        }
    }
}