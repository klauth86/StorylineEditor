/*
StorylineEditor
Copyright (C) 2023 Pentangle Studio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
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

        public override bool PassFilter(string filter)
        {
            return
                !nodes.TrueForAll((node)=>!node.PassFilter(filter)) ||
                base.PassFilter(filter);
        }

        public List<Node_BaseM> nodes { get; set; }
        public List<LinkM> links { get; set; }
    }
}