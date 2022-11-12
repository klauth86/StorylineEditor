/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using StorylineEditor.Common;
using StorylineEditor.Model;
using StorylineEditor.Model.Nodes;

namespace StorylineEditor.ViewModels.Nodes
{
    [XmlRoot]
    public class NodePairVm : BaseVm<TreeVm>
    {
        public NodePairVm(TreeVm Parent, long additionalTicks) : base(Parent, additionalTicks) { }

        public NodePairVm() : this(null, 0) { }

        protected BaseM model = null;
        public override BaseM GetModel(long ticks, Dictionary<string, string> idReplacer)
        {
            if (model != null) return model;

            var linkModel = new LinkM()
            {
                fromNodeId = From?.GetModel(ticks, idReplacer)?.id,
                toNodeId = To?.GetModel(ticks, idReplacer)?.id,
            };

            model = linkModel;

            var times = id.Replace("NodePairVm_", "").Substring(0, 19).Split('_');
            model.createdAt = new System.DateTime(int.Parse(times[0]), int.Parse(times[1]), int.Parse(times[2]), int.Parse(times[3]), int.Parse(times[4]), int.Parse(times[5]));
            model.id = string.Format("{0}_{1:yyyy_MM_dd_HH_mm_ss}_{2}_{3}", model.GetType().Name, model.createdAt, model.createdAt.Ticks, ticks);

            return model;
        }

        public string FromId { get; set; }

        public string ToId { get; set; }

        [XmlIgnore]
        public Node_BaseVm From => Parent?.Nodes.FirstOrDefault(node => node.Id == FromId);

        [XmlIgnore]
        public Node_BaseVm To => Parent?.Nodes.FirstOrDefault(node => node.Id == ToId);

        public override bool PassFilter(string filter) =>
            Description != null && Description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
            From != null && From.PassFilter(filter) &&
            To != null && To.PassFilter(filter);
    }
}