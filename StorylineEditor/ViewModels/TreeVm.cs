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
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using StorylineEditor.Common;
using StorylineEditor.Model;
using StorylineEditor.Model.Graphs;
using StorylineEditor.Model.Nodes;
using StorylineEditor.ViewModels.Nodes;
using StorylineEditor.ViewModels.Tabs;

namespace StorylineEditor.ViewModels
{
    [XmlRoot]
    public class TreeVm : NonFolderVm
    {
        public string InterlocutorId { get; set; }

        [XmlIgnore]
        public FolderedVm Interlocutor
        {
            get => Parent?.Parent?.NPCharacters.FirstOrDefault(item => item?.Id == InterlocutorId);
            set
            {
                if (InterlocutorId != value?.Id)
                {
                    InterlocutorId = value?.Id;
                    NotifyWithCallerPropName();

                    Notify(nameof(NameAndCharacter));
                }
            }
        }

        protected BaseM model = null;
        public override BaseM GetModel()
        {
            if (model != null) return model;

            if (Parent is JournalRecordsTabVm)
            {
                model = new QuestM()
                {
                    name = Name,
                    description = Description,
                    links = Links.Select((link)=>(LinkM)(link.GetModel())).ToList(),
                    nodes = Nodes.Select((node)=> (Node_BaseM)(node.GetModel())).ToList(), 
                };
            }
            else if (Parent is PlayerDialogsTabVm)
            {
                model = new DialogM()
                {
                    name = Name,
                    description = Description,
                    links = Links.Select((link) => (LinkM)(link.GetModel())).ToList(),
                    nodes = Nodes.Select((node) => (Node_BaseM)(node.GetModel())).ToList(),
                    npcId = Interlocutor?.GetModel()?.id, 
                };
            }
            else if (Parent is ReplicasTabVm)
            {
                model = new ReplicaM()
                {
                    name = Name,
                    description = Description,
                    links = Links.Select((link) => (LinkM)(link.GetModel())).ToList(),
                    nodes = Nodes.Select((node) => (Node_BaseM)(node.GetModel())).ToList(), 
                };
            }

            return model;
        }

        public bool IsPlayerDialog => Parent is PlayerDialogsTabVm;

        public event Action<Node_BaseVm> OnNodeRemoved = delegate { };

        public event Action<NodePairVm> OnLinkRemoved = delegate { };

        public event Action<Node_BaseVm> OnNodePositionChanged = delegate { };
        public void NodePositionChanged(Node_BaseVm node) { OnNodePositionChanged(node); }

        public TreeVm(BaseVm<FullContextVm> Parent, long additionalTicks) : base(Parent, additionalTicks)
        {
            Nodes = new ObservableCollection<Node_BaseVm>();
            Links = new ObservableCollection<NodePairVm>();

            RootNodeIds = new List<string>();
        }

        public TreeVm() : this(null, 0) { }

        public string Stats
        {
            get
            {
                string result = "";
                result += string.Format("ID: {0}", id) + Environment.NewLine;
                result += string.Format("NAME: {0}", Name) + Environment.NewLine;

                Dictionary<string, Dictionary<string, int>> countByCharacter = new Dictionary<string, Dictionary<string, int>>();
                Dictionary<string, int> countByTypeDescription = new Dictionary<string, int>();

                int characterNameMaxLength = 0;

                foreach (var node in Nodes)
                {
                    string characterName = "N/A";

                    if (node is IOwnered owneredNode)
                    {
                        characterName = owneredNode.Owner?.Name;
                        characterNameMaxLength = Math.Max(characterName?.Length ?? 0, characterNameMaxLength);
                    }

                    string gender = " ";
                    if (node.Gender == Node_BaseVm.MALE) gender = "👨";
                    if (node.Gender == Node_BaseVm.FEMALE) gender = "👩";

                    if (!countByCharacter.ContainsKey(characterName))
                    {
                        countByCharacter.Add(characterName, new Dictionary<string, int>() { { " ", 0 }, { "👨", 0 }, { "👩", 0 } });
                    }

                    countByCharacter[characterName][gender]++;

                    var typeDescription = AttributeHelper.GetTypeDescription(node.GetType());
                    if (!countByTypeDescription.ContainsKey(typeDescription)) countByTypeDescription.Add(typeDescription, 0);
                    countByTypeDescription[typeDescription]++;
                }

                if (countByCharacter.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ПЕРСОНАЖАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByCharacter.OrderBy(pair => pair.Key))
                    {
                        result += string.Format("{0, -" + (characterNameMaxLength + 6) + "}{1}", "- " + entry.Key + ":", string.Join("\t", entry.Value.Select(pair => string.Format("{0}{1, -6}", pair.Key, pair.Value)))) + Environment.NewLine;
                    }
                }

                if (countByTypeDescription.Count > 0)
                {
                    // Delimiter
                    result += Environment.NewLine;
                    result += Environment.NewLine;

                    result += "ВЕРШИНЫ ПО ТИПАМ:" + Environment.NewLine;
                    result += Environment.NewLine;

                    foreach (var entry in countByTypeDescription.OrderBy(pair => pair.Key)) result += "- " + entry.Key + ": " + entry.Value + Environment.NewLine;
                }

                // Delimiter
                result += Environment.NewLine;
                result += Environment.NewLine;

                result += "ОШИБКИ:" + Environment.NewLine;
                result += Environment.NewLine;

                if (RootNodeIds.Count == 0)
                {
                    result += string.Format("- ⚠ В дереве отсутствует корневая вершина. У дерева должна быть только ОДНА корневая вершина!") + Environment.NewLine;
                }
                else if (RootNodeIds.Count > 1)
                {
                    result += string.Format("- ⚠ В дереве несколько корневых вершин. У дерева должна быть только ОДНА корневая вершина!") + Environment.NewLine;
                }

                return result;
            }
        }
        public override bool IsValid => base.IsValid && Nodes.All(node => node?.IsValid ?? false) &&
            Links.All(link => link != null && Nodes.Any((node) => node.Id == link.FromId) &&
            Nodes.Any((node) => node.Id == link.ToId) && link.FromId != link.ToId);

        public override void NotifyNameChanged() { base.NotifyNameChanged(); Notify(nameof(NameAndCharacter)); }

        public string NameAndCharacter
        {
            get
            {
                if (IsPlayerDialog) return Interlocutor == null ? string.Format("{0}", Name) : string.Format("{0} [{1}]", Name, Interlocutor.Name);

                List<Node_BaseVm> nodes = NodesTraversal();

                foreach (var node in nodes) if (node is IOwnered owneredNode) return string.Format("{0} [{1}]", Name, owneredNode.Owner.Name);

                return Name;
            }
        }

        public string PrefixedNameAndCharacter => String.Format(IsPlayerDialog ? "Диал. {0}" : "Репл. {0}", NameAndCharacter);

        public ObservableCollection<Node_BaseVm> Nodes { get; set; }

        public ObservableCollection<NodePairVm> Links { get; set; }

        public List<string> RootNodeIds { get; set; }

        public bool NodeFilter(object obj)
        {
            if (obj is DNode_CharacterVm ||
                obj is DNode_DialogVm ||
                obj is JNode_AlternativeVm ||
                obj is JNode_StepVm)
            {
                return true;           
            }

            return false;
        }

        public void AddRootNode(Node_BaseVm node)
        {
            node.IsRoot = true;
            RootNodeIds.Add(node.Id);

            Notify(nameof(Stats));
        }

        public void RemoveRootNode(Node_BaseVm node)
        {
            RootNodeIds.Remove(node.Id);
            node.IsRoot = false;

            Notify(nameof(Stats));
        }

        public void AddNode(Node_BaseVm node)
        {
            if (node != null)
            {
                Nodes.Add(node);

                AddRootNode(node);

                NotifyIsValidChanged();
            }
        }

        public void RemoveNode(Node_BaseVm node)
        {
            List<NodePairVm> brokenLinks = new List<NodePairVm>();
            foreach (var link in Links) { if (link.FromId == node.Id || link.ToId == node.Id) brokenLinks.Add(link); }
            foreach (var link in brokenLinks) { RemoveLink(link); }

            Nodes.Remove(node);

            OnNodeRemoved(node);

            RemoveRootNode(node);

            NotifyIsValidChanged();
        }

        public bool CanLink(Node_BaseVm from, Node_BaseVm to)
        {
            if (from == to) return false;

            if (from is JNode_AlternativeVm && !(to is JNode_StepVm)) return false;

            if (from is JNode_StepVm || from is JNode_AlternativeVm)
            {
                if (to.ChildNodes.Contains(from)) return false;
            }

            var parentNodes = GetPrimaryParentNodes(to);

            if (parentNodes.Contains(from)) return false;

            return true;
        }

        public NodePairVm AddLink(Node_BaseVm from, Node_BaseVm to)
        {
            if (from != null && to != null && Nodes.Contains(from) && Nodes.Contains(to))
            {
                // Break existing links
                if (!from.AllowsManyChildren)
                {
                    List<NodePairVm> brokenLinks = new List<NodePairVm>();
                    foreach (var link in Links) { if (link.FromId == from.Id) brokenLinks.Add(link); }
                    foreach (var link in brokenLinks) { RemoveLink(link); }
                }

                RefreshJournalLinks_From(from, to);

                var newLink = new NodePairVm() { FromId = from?.Id, ToId = to?.Id, Parent = this };
                Links.Add(newLink);

                RemoveRootNode(to);

                NotifyIsValidChanged();

                return newLink;
            }

            return null;
        }

        public void RemoveLink(NodePairVm link)
        {
            if (link != null && Links.Contains(link))
            {
                Links.Remove(link);

                OnLinkRemoved(link);

                if (Links.All(remainingLink => remainingLink.ToId != link.ToId))
                {
                    AddRootNode(Nodes.FirstOrDefault((node) => node.Id == link.ToId));
                }

                NotifyIsValidChanged();
            }
        }

        public void RefreshJournalLinks_To(Node_BaseVm to)
        {
            List<NodePairVm> links = new List<NodePairVm>(Links);
            foreach (var link in links) if (link.ToId == to.Id) RefreshJournalLinks_From(link.From, link.To);
        }

        /// <summary>
        /// Break some links depending on node Type (Step and Alternative) and node Gender
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void RefreshJournalLinks_From(Node_BaseVm from, Node_BaseVm to)
        {
            // If we add new link from Step node to Step node
            // - all existing links to Alternative nodes will be broken (node Type rule)
            // - all existing Step nodes of intersecting gender will be broken (node Gender rule)

            if (to is JNode_StepVm)
            {
                List<NodePairVm> brokenLinks = new List<NodePairVm>();
                foreach (var link in Links)
                {
                    if (link.FromId == from.Id)
                    {
                        // if it is called from AddLink, we have no such link
                        // if it is called from RefreshJournalLinks_To, we prefer last changed node to all others
                        if (link.ToId == to.Id) continue;

                        if (link.To is JNode_AlternativeVm ||
                            to.Gender == link.To.Gender ||
                            to.Gender == 0 ||
                            link.To.Gender == 0)
                            brokenLinks.Add(link);
                    }
                }
                foreach (var link in brokenLinks) { RemoveLink(link); }
            }

            // If we add new link from Step node to Alternative node:
            // - all existing links to Step nodes will be broken (node Type rule)

            if (from is JNode_StepVm && to is JNode_AlternativeVm)
            {
                List<NodePairVm> brokenLinks = new List<NodePairVm>();
                foreach (var link in Links) { if (link.FromId == from.Id && Nodes.First(node => node.Id == link.ToId) is JNode_StepVm) brokenLinks.Add(link); }
                foreach (var link in brokenLinks) { RemoveLink(link); }
            }
        }

        public List<Node_BaseVm> GetPrimaryParentNodes(Node_BaseVm node)
        {
            var nodeIds = Links.Where(link => link.ToId == node.Id).Select(link => link.FromId).ToList();
            return Nodes.Where((otherNode) => nodeIds.Contains(otherNode.Id)).ToList();
        }

        public List<Node_BaseVm> GetPrimaryChildNodes(Node_BaseVm node)
        {
            var nodeIds = Links.Where(link => link.FromId == node.Id).Select(link => link.ToId).ToList();
            return Nodes.Where((otherNode) => nodeIds.Contains(otherNode.Id)).ToList();
        }

        public List<Node_BaseVm> NodesTraversal() => NodesTraversal(Nodes.Where((node) => RootNodeIds.Contains(node.Id)).ToList());

        public List<Node_BaseVm> NodesTraversal(Node_BaseVm startNode, bool includeSelf) { var result = NodesTraversal(GetPrimaryChildNodes(startNode)); if (includeSelf) result.Insert(0, startNode); return result; }

        public List<Node_BaseVm> NodesTraversal(List<Node_BaseVm> layerNodes)
        {
            List<Node_BaseVm> processedNodes = new List<Node_BaseVm>();

            while (layerNodes.Count > 0)
            {
                processedNodes.AddRange(layerNodes);
                layerNodes.Clear();

                foreach (var processedNode in processedNodes)
                {
                    foreach (var childNode in GetPrimaryChildNodes(processedNode))
                    {
                        if (!processedNodes.Contains(childNode) && !layerNodes.Contains(childNode)) layerNodes.Add(childNode);
                    }
                }
            }

            return processedNodes;
        }

        public List<Node_BaseVm> GetRootNodes(Node_BaseVm node)
        {
            List<Node_BaseVm> layerNodes = new List<Node_BaseVm>() { node };

            if (node.IsRoot) return layerNodes;

            List<Node_BaseVm> result = new List<Node_BaseVm>();

            List<Node_BaseVm> processedNodes = new List<Node_BaseVm>();

            while (layerNodes.Count > 0)
            {

                processedNodes.AddRange(layerNodes);
                layerNodes.Clear();

                foreach (var processedNode in processedNodes)
                {
                    foreach (var parentNode in GetPrimaryParentNodes(processedNode))
                    {
                        if (!layerNodes.Contains(parentNode) && !processedNodes.Contains(parentNode))
                        {
                            layerNodes.Add(parentNode);
                            if (parentNode.IsRoot) result.Add(parentNode);
                        }
                    }
                }
            }

            return result;
        }

        public override void SetupParenthood()
        {
            foreach (var node in Nodes)
            {
                node.IsRoot = RootNodeIds.Contains(node.Id);
                node.Parent = this;
                node.SetupParenthood();
            }

            foreach (var link in Links)
            {
                link.Parent = this;
                link.SetupParenthood();
            }
        }

        public override bool PassFilter(string filter) => 
            base.PassFilter(filter) ||
            Nodes.Any(node => node.PassFilter(filter)) || 
            Links.Any(link => link.PassFilter(filter));

        protected override void CloneInternalData(BaseVm destObj, long additionalTicks)
        {
            base.CloneInternalData(destObj, additionalTicks);

            if (destObj is TreeVm casted)
            {
                ////// TODO MORE COMPLEX
            }
        }
    }
}