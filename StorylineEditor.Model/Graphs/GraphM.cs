/*
Этот файл — часть StorylineEditor.

StorylineEditor — свободная программа: вы можете перераспространять ее и/или изменять ее на условиях Стандартной общественной лицензии GNU в том виде, 
в каком она была опубликована Фондом свободного программного обеспечения; либо версии 3 лицензии, либо (по вашему выбору) любой более поздней версии.

StorylineEditor распространяется в надежде, что она будет полезной, но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА или ПРИГОДНОСТИ ДЛЯ 
ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Стандартной общественной лицензии GNU.

Вы должны были получить копию Стандартной общественной лицензии GNU вместе с этой программой. Если это не так, см. <https://www.gnu.org/licenses/>.
*/

using StorylineEditor.Model.Nodes;
using System.Collections.Generic;

namespace StorylineEditor.Model.Graphs
{
    public abstract class GraphM : BaseM
    {
        public GraphM(long additionalTicks) : base(additionalTicks)
        {
            nodes = new List<Node_BaseM>();
            links = new List<LinkM>();
        }

        public GraphM() : this(0) { }

        protected override void CloneInternal(BaseM targetObject)
        {
            base.CloneInternal(targetObject);

            if (targetObject is GraphM casted)
            {
                Dictionary<string, string> mapping = new Dictionary<string, string>();

                for (int i = 0; i < nodes.Count; i++)
                {
                    casted.nodes.Add(nodes[i].CloneAs<Node_BaseM>(i));
                    mapping.Add(nodes[i].id, casted.nodes[i].id);
                }

                for (int i = 0; i < links.Count; i++)
                {
                    casted.links.Add(links[i].CloneAs<LinkM>(i));
                }

                foreach (var linkModel in casted.links)
                {
                    linkModel.fromNodeId = mapping[linkModel.fromNodeId];
                    linkModel.toNodeId = mapping[linkModel.toNodeId];
                }
            }
        }

        public List<Node_BaseM> nodes { get; set; }
        public List<LinkM> links { get; set; }
    }
}